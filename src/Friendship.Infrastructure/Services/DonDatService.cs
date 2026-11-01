using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongDat(int SanPhamId, int SoLuong, string? LoiChuc, int? ThietKeBanhId = null);
public record YeuCauDat(int NhanVienId, string? SoDienThoai, string? TenKhach, IReadOnlyList<DongDat> Dong,
    DateTime NgayGioLay, string? MaVoucher, int DiemSuDung, decimal? TienCoc, PhuongThucTT PhuongThuc, decimal? TienKhachDua);

public enum KieuHuy { KHACH_HUY, BUNG_COC }

/// <summary>UC02, UC04, UC07 và tìm đơn (kèm UC05, UC06).</summary>
public class DonDatService(FriendshipDb db, TimeProvider clock, KhachHangService khach, KhuyenMaiService voucher,
    CaService ca, ThanhToanService thanhToan, SanPhamService sanPham)
{
    public const int LoiChucToiDa = 200;
    const string XungDot = "Đơn vừa được người khác cập nhật. Tải lại đơn và thử lại.";

    DateTime BayGio => clock.GetLocalNow().DateTime;
    DateOnly HomNay => DateOnly.FromDateTime(BayGio);

    /// <summary>Dựng đơn và tính giá cho bước xem trước; chưa kiểm ca, chưa xét cọc, không Add vào context.</summary>
    public async Task<DonHang> XemTruocAsync(YeuCauDat y) => (await LapAsync(y)).Don;

    public async Task<DonHang> TaoAsync(YeuCauDat y)
    {
        await ca.CanDangMoAsync(y.NhanVienId);                                        // BR18, UC02 E4
        var (don, kh, moi) = await LapAsync(y);

        if (y.TienCoc is { } coc)
        {
            CocRule.Kiem(don.TongThanhToan, coc);                                     // BR01
            await thanhToan.GhiNhanAsync(don, LoaiThanhToan.COC, y.PhuongThuc, coc, y.TienKhachDua, y.NhanVienId);
            don.TrangThai = TrangThaiDon.DA_COC;
        }

        if (moi) db.KhachHangs.Add(kh);
        db.DonHangs.Add(don);
        await db.LuuNghiepVuAsync("Không lưu được đơn vì dữ liệu vừa thay đổi (ví dụ số điện thoại khách mới đã được tạo). Vui lòng thử lại.");
        return don;
    }

    async Task<(DonHang Don, KhachHang Kh, bool Moi)> LapAsync(YeuCauDat y)
    {
        var bay = BayGio;
        if (y.Dong.Count == 0) throw new NghiepVuException("Đơn phải có ít nhất một bánh.");
        if (y.Dong.Any(d => d.SoLuong <= 0)) throw new NghiepVuException("Số lượng bánh phải lớn hơn 0.");
        if (y.Dong.Any(d => d.SoLuong > LoBanhService.SoLuongToiDa))
            throw new NghiepVuException($"Số lượng bánh quá lớn (tối đa {LoBanhService.SoLuongToiDa} mỗi dòng).");
        if (y.Dong.Any(d => (d.LoiChuc ?? "").Trim().Length > LoiChucToiDa))
            throw new NghiepVuException($"Lời chúc tối đa {LoiChucToiDa} ký tự.");
        if (y.NgayGioLay <= bay) throw new NghiepVuException("Giờ hẹn lấy phải sau thời điểm hiện tại.");     // UC02 E1
        if (string.IsNullOrWhiteSpace(y.SoDienThoai)) throw new NghiepVuException("Đơn đặt cần số điện thoại khách.");

        var (kh, moi) = await khach.TimHoacTaoAsync(y.SoDienThoai, y.TenKhach);
        var ids = y.Dong.Select(d => d.SanPhamId).Distinct().ToList();
        var sps = await db.SanPhams.Where(s => ids.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

        var don = new DonHang
        {
            LoaiDon = LoaiDon.DAT_TRUOC, NgayLap = bay, NgayGioLay = y.NgayGioLay, NhanVienId = y.NhanVienId,
            KhachHang = kh, TrangThai = TrangThaiDon.CHO_COC
        };
        foreach (var d in y.Dong)
        {
            var sp = sps.GetValueOrDefault(d.SanPhamId) ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
            var loiChuc = (d.LoiChuc ?? "").Trim();
            if (loiChuc.Length > 0 && !sp.LaBanhKem)
                throw new NghiepVuException($"Chỉ bánh kem mới được ghi lời chúc viết lên bánh (sản phẩm '{sp.TenBanh}' không hỗ trợ).");
            if (d.ThietKeBanhId is { } tkId)
            {
                var tk = await db.ThietKeBanhs.FindAsync(tkId)
                    ?? throw new NghiepVuException("Không tìm thấy thiết kế bánh.");

                if (tk.TrangThai != TrangThaiThietKe.DA_BAO_GIA || (tk.HanBaoGia.HasValue && tk.HanBaoGia.Value < bay))
                    throw new NghiepVuException("Thiết kế chưa được báo giá hoặc báo giá đã hết hạn.");

                if (d.SoLuong != 1)
                    throw new NghiepVuException("Mỗi bánh tùy chỉnh chỉ được đặt số lượng 1.");

                if (d.SanPhamId != tk.SanPhamNenId)
                    throw new NghiepVuException("Sản phẩm nền không khớp với thiết kế.");

                if (!string.IsNullOrWhiteSpace(y.MaVoucher) || y.DiemSuDung > 0)
                    throw new NghiepVuException("Đơn đặt bánh tùy chỉnh không áp dụng voucher hoặc điểm tích lũy.");

                don.ChiTiets.Add(new ChiTietDonHang
                {
                    SanPhamId = sp.Id,
                    SanPham = sp,
                    SoLuong = 1,
                    DonGia = tk.GiaCuoi,
                    ThanhTien = tk.GiaCuoi,
                    LoiChuc = loiChuc.Length == 0 ? null : loiChuc,
                    ThietKeBanhId = tk.Id,
                    ThietKeBanh = tk
                });

                ThietKeRule.Chuyen(tk, TrangThaiThietKe.DA_DAT);
                tk.DonHang = don;
            }
            else
            {
                don.ChiTiets.Add(new ChiTietDonHang
                {
                    SanPhamId = sp.Id,
                    SanPham = sp,
                    SoLuong = d.SoLuong,
                    DonGia = sp.DonGia,
                    ThanhTien = sp.DonGia * d.SoLuong,
                    LoiChuc = loiChuc.Length == 0 ? null : loiChuc
                });
            }
        }

        var km = await voucher.TimTheoMaAsync(y.MaVoucher);
        var gia = GiamGiaRule.Tinh(don.ChiTiets.Select(c => new DongGia(c.ThanhTien, false)).ToList(),
            km, HomNay, y.DiemSuDung, kh!.DiemTichLuy);
        don.KhuyenMai = km;
        don.DiemSuDung = y.DiemSuDung;
        don.TongTienHang = gia.TongTienHang;
        don.TienGiamGia = gia.TienGiamGia;
        don.TongThanhToan = gia.TongThanhToan;
        if (don.TongThanhToan <= 0) throw new NghiepVuException("Tổng thanh toán phải lớn hơn 0. Kiểm tra lại điểm sử dụng.");
        return (don, kh, moi);
    }

    /// <summary>UC02 luồng A3: khách cọc sau — thực hiện bước 6–8 trên đơn đang chờ cọc.</summary>
    public async Task<DonHang> GhiNhanCocAsync(int donId, int nhanVienId, decimal tienCoc, PhuongThucTT phuongThuc, decimal? tienKhachDua)
    {
        var don = await LayAsync(donId) ?? throw new NghiepVuException("Không tìm thấy đơn.");
        if (don.LoaiDon != LoaiDon.DAT_TRUOC || don.TrangThai != TrangThaiDon.CHO_COC)
            throw new NghiepVuException("Chỉ ghi nhận cọc cho đơn đặt đang chờ cọc.");
        CocRule.Kiem(don.TongThanhToan, tienCoc);
        await thanhToan.GhiNhanAsync(don, LoaiThanhToan.COC, phuongThuc, tienCoc, tienKhachDua, nhanVienId);
        TrangThaiDonRule.Chuyen(don, TrangThaiDon.DA_COC);
        await db.LuuNghiepVuAsync(XungDot);
        return don;
    }

    public Task<DonHang?> LayAsync(int id) =>
        db.DonHangs.Include(d => d.ChiTiets).ThenInclude(c => c.SanPham)
            .Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh).ThenInclude(tk => tk!.ThanhPhans)
            .Include(d => d.KhachHang).Include(d => d.KhuyenMai).Include(d => d.NhanVien).Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.Id == id);

    /// <summary>Tìm theo mã đơn (DH-000123 hoặc 123), số điện thoại hoặc tên khách; mới nhất trước, tối đa 200.</summary>
    public Task<List<DonHang>> TimAsync(string? tuKhoa, TrangThaiDon? trangThai = null, LoaiDon? loai = null)
    {
        var q = db.DonHangs.Include(d => d.KhachHang).AsQueryable();
        if (trangThai is { } tt) q = q.Where(d => d.TrangThai == tt);
        if (loai is { } l) q = q.Where(d => d.LoaiDon == l);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            var thuong = k.ToLower();
            var coId = int.TryParse(k.StartsWith("DH-", StringComparison.OrdinalIgnoreCase) ? k[3..] : k, out var id);
            q = q.Where(d => (coId && d.Id == id)
                || (d.KhachHang != null && (d.KhachHang.SoDienThoai.Contains(k) || d.KhachHang.TenKhachHang.ToLower().Contains(thuong))));
        }
        return q.OrderByDescending(d => d.Id).Take(200).ToListAsync();
    }


    /// <summary>UC04: thu phần còn lại (nếu còn), giao bánh, trừ/cộng điểm. Trả tiền thối.</summary>
    public async Task<decimal> GiaoAsync(int donId, int nhanVienId, PhuongThucTT phuongThuc, decimal? tienKhachDua)
    {
        var don = await LayAsync(donId) ?? throw new NghiepVuException("Không tìm thấy đơn.");
        if (don.LoaiDon != LoaiDon.DAT_TRUOC || don.TrangThai != TrangThaiDon.CHO_GIAO)
            throw new NghiepVuException(LyDoKhongGiao(don));
        await ca.CanDangMoAsync(nhanVienId);
        if (don.KhachHang is { } k && k.DiemTichLuy < don.DiemSuDung)
            throw new NghiepVuException($"Khách không còn đủ {don.DiemSuDung} điểm để đổi (còn {k.DiemTichLuy}).");

        var thoi = 0m;
        if (don.ConLai > 0)                                                            // BR02: thu đủ phần còn lại mới giao
            thoi = (await thanhToan.GhiNhanAsync(don, LoaiThanhToan.TAT_TOAN, phuongThuc, don.ConLai, tienKhachDua, nhanVienId)).TienThoi;
        if (don.KhachHang is { } kh)
            kh.DiemTichLuy += DiemRule.DiemCong(don.TongThanhToan) - don.DiemSuDung;
        TrangThaiDonRule.Chuyen(don, TrangThaiDon.DA_GIAO);
        await db.LuuNghiepVuAsync(XungDot);
        return thoi;
    }

    static string LyDoKhongGiao(DonHang don) => don.TrangThai switch
    {
        TrangThaiDon.CHO_COC => "Đơn chưa nhận cọc, chưa thể giao.",
        TrangThaiDon.DA_COC or TrangThaiDon.DANG_LAM => "Bánh chưa làm xong, chưa thể giao.",
        TrangThaiDon.DA_GIAO => "Đơn đã được giao.",
        TrangThaiDon.DA_HUY or TrangThaiDon.BUNG_COC => "Đơn đã bị hủy.",
        _ => "Đơn này không thể giao."
    };

    /// <summary>UC07: hủy theo BR23 (hoàn cọc nếu còn ≥ 24 giờ) hoặc tính bùng cọc theo BR11.</summary>
    public async Task<DonHang> HuyAsync(int donId, int nhanVienId, KieuHuy kieu, bool xacNhanDangLam = false)
    {
        if (!Enum.IsDefined(kieu)) throw new NghiepVuException("Kiểu hủy không hợp lệ.");
        var bay = BayGio;
        var don = await LayAsync(donId) ?? throw new NghiepVuException("Không tìm thấy đơn.");
        if (don.LoaiDon != LoaiDon.DAT_TRUOC) throw new NghiepVuException("Chỉ hủy được đơn đặt trước.");
        if (don.TrangThai == TrangThaiDon.DA_GIAO) throw new NghiepVuException("Đơn đã giao, không thể hủy.");
        if (don.TrangThai is TrangThaiDon.DA_HUY or TrangThaiDon.BUNG_COC) throw new NghiepVuException("Đơn đã được hủy trước đó.");
        if (don.TrangThai == TrangThaiDon.DANG_LAM && !xacNhanDangLam)
            throw new NghiepVuException("Bếp đang làm đơn này. Hãy xác nhận lần hai để hủy.");

        if (kieu == KieuHuy.BUNG_COC)
        {
            if (don.TrangThai != TrangThaiDon.CHO_GIAO || !HuyDonRule.BungCoc(bay, don.NgayGioLay!.Value))
                throw new NghiepVuException("Chỉ tính bùng cọc khi bánh đã xong và khách chưa nhận quá 24 giờ sau giờ hẹn.");
            await ChuyenBungCocAsync(don, nhanVienId, bay);
        }
        else
        {
            if (don.TrangThai == TrangThaiDon.CHO_GIAO && HuyDonRule.BungCoc(bay, don.NgayGioLay!.Value))
                throw new NghiepVuException("Khách đã quá 24 giờ sau giờ hẹn chưa nhận bánh: hãy chọn tính bùng cọc (mất cọc, lập phiếu hủy bánh).");
            if (don.ThanhToans.Any(t => t.Loai == LoaiThanhToan.COC) && HuyDonRule.HoanCoc(bay, don.NgayGioLay!.Value))
                await thanhToan.GhiHoanCocAsync(don, nhanVienId);
            TrangThaiDonRule.Chuyen(don, TrangThaiDon.DA_HUY);
        }
        await db.LuuNghiepVuAsync(XungDot);
        return don;
    }

    /// <summary>Spec mục 9: quét các đơn chờ giao quá 24 giờ sau hẹn, chuyển BUNG_COC và lập phiếu hủy nháp. Trả các đơn đã chuyển.</summary>
    public async Task<List<DonHang>> QuetBungCocAsync(int nhanVienId)
    {
        var bay = BayGio;
        var han = bay - HuyDonRule.Han;
        var ds = await db.DonHangs.Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh)
            .Where(d => d.LoaiDon == LoaiDon.DAT_TRUOC && d.TrangThai == TrangThaiDon.CHO_GIAO && d.NgayGioLay < han)
            .ToListAsync();
        if (ds.Count == 0) return ds;
        foreach (var d in ds) await ChuyenBungCocAsync(d, nhanVienId, bay);
        await db.LuuNghiepVuAsync(XungDot);
        return ds;
    }

    async Task ChuyenBungCocAsync(DonHang don, int nhanVienId, DateTime bay)
    {
        TrangThaiDonRule.Chuyen(don, TrangThaiDon.BUNG_COC);
        var phieu = new PhieuXuatHuy { NguoiLapId = nhanVienId, NgayLap = bay, TrangThai = TrangThaiPhieu.NHAP };
        var von = new Dictionary<int, decimal>();
        foreach (var ct in don.ChiTiets)
        {
            decimal v;
            if (ct.ThietKeBanhId.HasValue)
            {
                var tk = ct.ThietKeBanh ?? await db.ThietKeBanhs.FindAsync(ct.ThietKeBanhId.Value);
                v = tk?.GiaVonDuKien ?? 0m;
            }
            else
            {
                if (!von.TryGetValue(ct.SanPhamId, out v))
                    von[ct.SanPhamId] = v = (await sanPham.GiaVonAsync(ct.SanPhamId)).GiaVon;
            }
            phieu.ChiTiets.Add(new ChiTietXuatHuy
            {
                DonHang = don, SoLuongHuy = ct.SoLuong, DonGiaVon = v, LyDo = LyDoHuy.KHACH_BUNG_COC
            });
        }
        db.PhieuXuatHuys.Add(phieu);
    }

    /// <summary>UC12 bước 1: chọn đơn → nhu cầu bánh theo từng sản phẩm để đưa vào <c>KhoService.LapKeHoachXuatAsync</c>.</summary>
    public async Task<List<YeuCauXuat>> YeuCauXuatTuDonAsync(int donId)
    {
        var don = await LayAsync(donId) ?? throw new NghiepVuException("Không tìm thấy đơn.");
        if (don.LoaiDon != LoaiDon.DAT_TRUOC || don.TrangThai is not (TrangThaiDon.DA_COC or TrangThaiDon.DANG_LAM))
            throw new NghiepVuException("Đơn chưa xuống bếp hoặc đã xong, không xuất nguyên liệu cho đơn này.");

        var ds = don.ChiTiets.GroupBy(c => c.SanPhamId).Select(g => new YeuCauXuat(g.Key, g.Sum(c => c.SoLuong))).ToList();

        foreach (var ct in don.ChiTiets)
        {
            if (ct.ThietKeBanh != null)
            {
                if (ct.ThietKeBanh.ThanhPhans.Count == 0 && ct.ThietKeBanhId.HasValue)
                {
                    await db.Entry(ct.ThietKeBanh).Collection(t => t.ThanhPhans).LoadAsync();
                }

                foreach (var tp in ct.ThietKeBanh.ThanhPhans)
                {
                    ds.Add(new YeuCauXuat(0, tp.SoLuong * ct.SoLuong, tp.ThanhPhanId));
                }
            }
        }

        return ds;
    }

    /// <summary>Đếm đơn theo trạng thái (và loại); khác <see cref="TimAsync"/> không bị giới hạn 200 dòng.</summary>
    public Task<int> DemAsync(TrangThaiDon trangThai, LoaiDon? loai = null)
    {
        var q = db.DonHangs.Where(d => d.TrangThai == trangThai);
        if (loai is { } l) q = q.Where(d => d.LoaiDon == l);
        return q.CountAsync();
    }
}
