using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongBanLe(int SanPhamId, int SoLuong);
public record DongThanhToan(PhuongThucTT PhuongThuc, decimal SoTien, decimal? TienKhachDua = null);
public record YeuCauBanLe(int NhanVienId, string? SoDienThoai, string? TenKhach, IReadOnlyList<DongBanLe> Dong,
    string? MaVoucher, int DiemSuDung, PhuongThucTT PhuongThuc, decimal? TienKhachDua,
    IReadOnlyList<DongThanhToan>? ThanhToans = null);
public record KetQuaBanLe(DonHang Don, decimal TienThoi);

/// <summary>UC03 (kèm UC06 và UC05): chọn lô FEFO, giá xả hàng, voucher, điểm, thu tiền, trừ tồn, tích điểm trong một lần lưu.</summary>
public class BanLeService(FriendshipDb db, TimeProvider clock, KhachHangService khach, KhuyenMaiService voucher,
    CaService ca, ThanhToanService thanhToan, SanPhamService sanPham)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;
    DateOnly HomNay => DateOnly.FromDateTime(BayGio);

    /// <summary>Dựng đơn và tính giá; không thêm gì vào context nên không đổi dữ liệu.</summary>
    public async Task<DonHang> XemTruocAsync(YeuCauBanLe y) => (await LapAsync(y)).Don;

    /// <summary>Trả toàn bộ đơn bán lẻ đã thanh toán, hoàn tiền trong ca đang mở, thu hồi/hoàn điểm, giữ nguyên tồn kho do ATTP.</summary>
    public async Task<DonHang> TraHangAsync(int donHangId, int nhanVienId, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new NghiepVuException("Lý do trả hàng là bắt buộc.");
        if (lyDo.Trim().Length > 500)
            throw new NghiepVuException("Lý do trả hàng không được quá 500 ký tự.");

        var don = await db.DonHangs
            .Include(d => d.ChiTiets)
            .Include(d => d.ThanhToans)
            .Include(d => d.KhachHang)
            .FirstOrDefaultAsync(d => d.Id == donHangId) ?? throw new NghiepVuException("Không tìm thấy đơn.");

        if (don.LoaiDon != LoaiDon.BAN_LE)
            throw new NghiepVuException("Chỉ cho phép trả hàng cho đơn bán lẻ.");

        if (don.TrangThai == TrangThaiDon.DA_TRA)
            throw new NghiepVuException("Đơn hàng đã được trả trước đó.");

        if (don.TrangThai != TrangThaiDon.DA_THANH_TOAN)
            throw new NghiepVuException("Chỉ trả hàng cho đơn bán lẻ đã thanh toán.");

        if (DateOnly.FromDateTime(don.NgayLap) != HomNay)
            throw new NghiepVuException("Chỉ trả hàng trong ngày bán.");

        // Kiểm tra ca đang mở của thu ngân (BR: mọi khoản tiền gắn một ca đang mở)
        await ca.CanDangMoDeThuAsync(nhanVienId);

        // Điểm tích lũy: hoàn lại điểm đã dùng (+DiemSuDung), thu hồi điểm đã cộng (-DiemCong)
        if (don.KhachHang is { } kh)
        {
            var diemCong = DiemRule.DiemCong(don.TongThanhToan);
            var diemDung = don.DiemSuDung;
            var delta = diemDung - diemCong;
            if (kh.DiemTichLuy + delta < 0)
                throw new NghiepVuException("Khách hàng không đủ điểm tích lũy để thu hồi (khách đã dùng điểm thưởng).");
            kh.DiemTichLuy += delta;
        }

        // Hoàn tiền: ghi nhận khoản chi âm trong ca đang mở
        await thanhToan.GhiHoanTienBanLeAsync(don, nhanVienId);

        // Chuyển trạng thái sang DA_TRA và ghi lý do trả
        TrangThaiDonRule.Chuyen(don, TrangThaiDon.DA_TRA);
        don.LyDoTra = lyDo.Trim();

        // Tồn kho: bánh đã bán KHÔNG nhập lại kho (an toàn thực phẩm); lập phiếu hủy nháp để giá vốn vào báo cáo hủy khi được duyệt
        var phieu = new PhieuXuatHuy { NguoiLapId = nhanVienId, NgayLap = BayGio, TrangThai = TrangThaiPhieu.NHAP };
        foreach (var ct in don.ChiTiets)
            phieu.ChiTiets.Add(new ChiTietXuatHuy
            {
                DonHang = don, SoLuongHuy = ct.SoLuong, LyDo = LyDoHuy.KHACH_TRA_HANG,
                DonGiaVon = (await sanPham.GiaVonAsync(ct.SanPhamId)).GiaVon
            });
        db.PhieuXuatHuys.Add(phieu);

        await db.LuuNghiepVuAsync("Đơn hàng hoặc ca vừa được người khác cập nhật. Vui lòng tải lại trang và thử lại.");
        return don;
    }

    public async Task<KetQuaBanLe> BanAsync(YeuCauBanLe y)
    {
        await ca.CanDangMoAsync(y.NhanVienId);                         // BR18: không có ca thì không làm gì cả
        var (don, kh, moi) = await LapAsync(y);

        var thoi = 0m;
        if (don.TongThanhToan > 0)                                     // kiểm tra tiền trước khi sửa tồn và điểm
        {
            var dongs = y.ThanhToans is { Count: > 0 }
                ? y.ThanhToans
                : [new DongThanhToan(y.PhuongThuc, don.TongThanhToan, y.TienKhachDua)];
            thoi = await thanhToan.GhiNhanNhieuAsync(don, LoaiThanhToan.THANH_TOAN_DU, dongs, y.NhanVienId);
        }

        foreach (var ct in don.ChiTiets) ct.LoSanPham!.SoLuongTon -= ct.SoLuong;
        if (kh is not null)
        {
            if (moi) db.KhachHangs.Add(kh);
            kh.DiemTichLuy += DiemRule.DiemCong(don.TongThanhToan) - don.DiemSuDung;
        }
        db.DonHangs.Add(don);
        await db.LuuNghiepVuAsync("Tồn kho hoặc thông tin khách vừa được người khác thay đổi. Vui lòng bán lại.");
        return new KetQuaBanLe(don, thoi);
    }

    async Task<(DonHang Don, KhachHang? Kh, bool Moi)> LapAsync(YeuCauBanLe y)
    {
        var bay = BayGio;
        if (y.Dong.Count == 0) throw new NghiepVuException("Chưa chọn bánh nào.");
        if (y.Dong.Any(d => d.SoLuong <= 0)) throw new NghiepVuException("Số lượng bánh phải lớn hơn 0.");
        if (y.Dong.Any(d => d.SoLuong > LoBanhService.SoLuongToiDa))
            throw new NghiepVuException($"Số lượng bánh quá lớn (tối đa {LoBanhService.SoLuongToiDa} mỗi dòng).");

        var gop = y.Dong.GroupBy(d => d.SanPhamId).Select(g => (Id: g.Key, Sl: g.Sum(x => x.SoLuong))).ToList();
        var ids = gop.Select(g => g.Id).ToList();
        var sps = await db.SanPhams.Where(s => ids.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        var los = await db.LoSanPhams.Where(l => ids.Contains(l.SanPhamId) && l.SoLuongTon > 0).ToListAsync();

        var don = new DonHang
        {
            LoaiDon = LoaiDon.BAN_LE, NgayLap = bay, NhanVienId = y.NhanVienId, TrangThai = TrangThaiDon.DA_THANH_TOAN
        };
        foreach (var (id, sl) in gop)
        {
            var sp = sps.GetValueOrDefault(id) ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
            if (!sp.DangKinhDoanh) throw new NghiepVuException($"Sản phẩm '{sp.TenBanh}' đã ngừng kinh doanh.");
            var cua = los.Where(l => l.SanPhamId == id).ToList();
            var (lay, thieu) = FefoBanh.PhanBo(cua, sl, bay);
            if (thieu > 0)
            {
                var duoc = sl - thieu;
                if (duoc == 0 && cua.Count > 0)
                    throw new NghiepVuException($"'{sp.TenBanh}' chỉ còn lô hết hạn, không được bán. Hãy lập phiếu hủy.");
                throw new NghiepVuException(duoc == 0
                    ? $"'{sp.TenBanh}' đã hết hàng."
                    : $"'{sp.TenBanh}' chỉ còn bán được tối đa {duoc}.");
            }
            foreach (var (lo, n) in lay)
            {
                var gia = GiamGiaRule.GiaXaHang(sp.DonGia, lo.TyLeGiamXaHang);
                don.ChiTiets.Add(new ChiTietDonHang
                {
                    SanPhamId = id, SanPham = sp, LoSanPhamId = lo.Id, LoSanPham = lo, SoLuong = n,
                    DonGia = gia, TienGiam = (sp.DonGia - gia) * n, ThanhTien = gia * n
                });
            }
        }

        var (kh, moi) = await khach.TimHoacTaoAsync(y.SoDienThoai, y.TenKhach);
        if (y.DiemSuDung > 0 && kh is null)
            throw new NghiepVuException("Cần nhập số điện thoại khách để dùng điểm tích lũy.");
        var km = await voucher.TimTheoMaAsync(y.MaVoucher);
        var gia2 = GiamGiaRule.Tinh(don.ChiTiets.Select(c => new DongGia(c.ThanhTien, c.TienGiam > 0)).ToList(),
            km, HomNay, y.DiemSuDung, kh?.DiemTichLuy ?? 0);

        don.KhachHang = kh;
        don.KhuyenMai = km;
        don.DiemSuDung = y.DiemSuDung;
        don.TongTienHang = gia2.TongTienHang;
        don.TienGiamGia = gia2.TienGiamGia;
        don.TongThanhToan = gia2.TongThanhToan;
        return (don, kh, moi);
    }
}
