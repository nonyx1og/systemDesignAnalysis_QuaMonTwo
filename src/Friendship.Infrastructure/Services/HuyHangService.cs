using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public enum LoaiDoiTuongHuy { LO_NGUYEN_LIEU, LO_BANH, DON_BUNG_COC }

/// <summary>Với DON_BUNG_COC, SoLuong bị bỏ qua: hủy trọn đơn (mỗi dòng bánh của đơn một dòng phiếu).</summary>
public record DongHuyInput(LoaiDoiTuongHuy Loai, int DoiTuongId, decimal SoLuong, LyDoHuy LyDo);

public record LuaChonHuy(List<LoNguyenLieu> LoNguyenLieu, List<LoSanPham> LoBanh, List<DonHang> DonBungCoc);

/// <summary>UC17, UC18, BR09, BR10: phiếu chỉ trừ kho và ghi chi phí sau khi quản lý duyệt.</summary>
public class HuyHangService(FriendshipDb db, TimeProvider clock, SanPhamService sanPham)
{
    const string XungDot = "Phiếu hoặc tồn kho vừa được người khác thay đổi. Vui lòng mở lại phiếu.";

    DateTime BayGio => clock.GetLocalNow().DateTime;

    IQueryable<PhieuXuatHuy> DayDu => db.PhieuXuatHuys
        .Include(p => p.NguoiLap).Include(p => p.NguoiDuyet)
        .Include(p => p.ChiTiets).ThenInclude(c => c.LoNguyenLieu).ThenInclude(l => l!.NguyenLieu)
        .Include(p => p.ChiTiets).ThenInclude(c => c.LoSanPham).ThenInclude(l => l!.SanPham)
        .Include(p => p.ChiTiets).ThenInclude(c => c.DonHang);

    public Task<PhieuXuatHuy?> LayAsync(int id) => DayDu.FirstOrDefaultAsync(p => p.Id == id);

    public Task<List<PhieuXuatHuy>> TimAsync(TrangThaiPhieu? trangThai, int? nguoiLapId = null)
    {
        var q = db.PhieuXuatHuys.Include(p => p.NguoiLap).Include(p => p.NguoiDuyet).Include(p => p.ChiTiets).AsQueryable();
        if (trangThai is { } tt) q = q.Where(p => p.TrangThai == tt);
        if (nguoiLapId is { } id) q = q.Where(p => p.NguoiLapId == id);
        return q.OrderByDescending(p => p.NgayLap).ThenByDescending(p => p.Id).ToListAsync();
    }

    public static decimal TongTonThat(PhieuXuatHuy p) => p.ChiTiets.Sum(c => c.SoLuongHuy * c.DonGiaVon);

    /// <summary>Danh sách đối tượng có thể đưa vào phiếu (UC17 bước 2).</summary>
    public async Task<LuaChonHuy> LuaChonAsync()
    {
        var nl = await db.LoNguyenLieus.Include(l => l.NguyenLieu).Where(l => l.SoLuongCon > 0)
            .OrderBy(l => l.HanSuDung).ThenBy(l => l.Id).ToListAsync();
        var banh = await db.LoSanPhams.Include(l => l.SanPham).Where(l => l.SoLuongTon > 0)
            .OrderBy(l => l.HanSuDung).ThenBy(l => l.Id).ToListAsync();
        var don = await db.DonHangs.Where(d => d.TrangThai == TrangThaiDon.BUNG_COC
                && !db.ChiTietXuatHuys.Any(c => c.DonHangId == d.Id && c.PhieuXuatHuy.TrangThai != TrangThaiPhieu.TU_CHOI))
            .OrderBy(d => d.Id).ToListAsync();
        return new LuaChonHuy(nl, banh, don);
    }

    /// <summary>UC17 bước 1–3: kiểm từng dòng, lấy đơn giá vốn, lưu phiếu ở trạng thái chờ duyệt. Có dòng sai thì không lưu gì.</summary>
    public async Task<PhieuXuatHuy> LapAsync(int nguoiLapId, IReadOnlyList<DongHuyInput> dong)
    {
        if (dong.Count == 0) throw new NghiepVuException("Phiếu hủy phải có ít nhất một dòng.");
        var nguoiLap = await db.NhanViens.FindAsync(nguoiLapId) ?? throw new NghiepVuException("Không tìm thấy người lập phiếu.");
        if (nguoiLap.VaiTro is not (VaiTro.BAN_HANG or VaiTro.SAN_XUAT or VaiTro.KHO))
            throw new NghiepVuException("Vai trò này không lập được phiếu hủy.");
        var ct = new List<ChiTietXuatHuy>();
        var daDung = new Dictionary<(LoaiDoiTuongHuy, int), decimal>();     // cộng dồn nhiều dòng cùng một lô khi kiểm tồn
        var von = new Dictionary<int, decimal>();                           // giá vốn bánh theo sản phẩm, tính một lần

        async Task<decimal> GiaVonBanh(int sanPhamId)
        {
            if (!von.TryGetValue(sanPhamId, out var v)) von[sanPhamId] = v = (await sanPham.GiaVonAsync(sanPhamId)).GiaVon;
            return v;
        }

        for (var i = 0; i < dong.Count; i++)
        {
            var d = dong[i];
            var p = $"Dòng {i + 1}: ";
            if (!Enum.IsDefined(d.LyDo)) throw new NghiepVuException(p + "lý do hủy không hợp lệ.");
            if (!Enum.IsDefined(d.Loai)) throw new NghiepVuException(p + "loại đối tượng không hợp lệ.");
            switch (d.Loai)
            {
                case LoaiDoiTuongHuy.LO_NGUYEN_LIEU:
                {
                    var lo = await db.LoNguyenLieus.Include(l => l.NguyenLieu).FirstOrDefaultAsync(l => l.Id == d.DoiTuongId)
                        ?? throw new NghiepVuException(p + "không tìm thấy lô nguyên liệu.");
                    if (d.LyDo is LyDoHuy.KHACH_BUNG_COC or LyDoHuy.KHACH_TRA_HANG) throw new NghiepVuException(p + "lý do bùng cọc hoặc trả hàng chỉ dùng cho dòng theo đơn.");
                    KiemSoLuong(p, d.SoLuong, soNguyen: false);
                    var tong = daDung[(d.Loai, lo.Id)] = daDung.GetValueOrDefault((d.Loai, lo.Id)) + d.SoLuong;
                    if (tong > lo.SoLuongCon)
                        throw new NghiepVuException($"{p}số lượng hủy ({tong:0.###}) vượt tồn lô #{lo.Id} của {lo.NguyenLieu.TenGoi} (còn {lo.SoLuongCon:0.###}).");
                    ct.Add(new ChiTietXuatHuy { LoNguyenLieuId = lo.Id, SoLuongHuy = d.SoLuong, DonGiaVon = lo.DonGiaNhap, LyDo = d.LyDo });
                    break;
                }
                case LoaiDoiTuongHuy.LO_BANH:
                {
                    var lo = await db.LoSanPhams.Include(l => l.SanPham).FirstOrDefaultAsync(l => l.Id == d.DoiTuongId)
                        ?? throw new NghiepVuException(p + "không tìm thấy lô bánh.");
                    if (d.LyDo is LyDoHuy.KHACH_BUNG_COC or LyDoHuy.KHACH_TRA_HANG) throw new NghiepVuException(p + "lý do bùng cọc hoặc trả hàng chỉ dùng cho dòng theo đơn.");
                    KiemSoLuong(p, d.SoLuong, soNguyen: true);
                    var tong = daDung[(d.Loai, lo.Id)] = daDung.GetValueOrDefault((d.Loai, lo.Id)) + d.SoLuong;
                    if (tong > lo.SoLuongTon)
                        throw new NghiepVuException($"{p}số lượng hủy ({tong:0.###}) vượt tồn lô #{lo.Id} của {lo.SanPham.TenBanh} (còn {lo.SoLuongTon}).");
                    ct.Add(new ChiTietXuatHuy { LoSanPhamId = lo.Id, SoLuongHuy = d.SoLuong, DonGiaVon = await GiaVonBanh(lo.SanPhamId), LyDo = d.LyDo });
                    break;
                }
                default:
                {
                    var don = await db.DonHangs.Include(x => x.ChiTiets).FirstOrDefaultAsync(x => x.Id == d.DoiTuongId)
                        ?? throw new NghiepVuException(p + "không tìm thấy đơn.");
                    if (don.TrangThai != TrangThaiDon.BUNG_COC) throw new NghiepVuException(p + $"đơn {don.Ma} chưa chuyển sang bùng cọc.");
                    if (d.LyDo != LyDoHuy.KHACH_BUNG_COC) throw new NghiepVuException(p + "đơn bùng cọc phải dùng lý do bùng cọc.");
                    if (ct.Any(c => c.DonHangId == don.Id)
                        || await db.ChiTietXuatHuys.AnyAsync(c => c.DonHangId == don.Id && c.PhieuXuatHuy.TrangThai != TrangThaiPhieu.TU_CHOI))
                        throw new NghiepVuException(p + $"đơn {don.Ma} đã có trong một phiếu hủy khác.");
                    foreach (var l in don.ChiTiets)
                        ct.Add(new ChiTietXuatHuy { DonHangId = don.Id, SoLuongHuy = l.SoLuong, DonGiaVon = await GiaVonBanh(l.SanPhamId), LyDo = LyDoHuy.KHACH_BUNG_COC });
                    break;
                }
            }
        }

        var phieu = new PhieuXuatHuy { NguoiLapId = nguoiLapId, NgayLap = BayGio, TrangThai = TrangThaiPhieu.CHO_DUYET, ChiTiets = ct };
        db.PhieuXuatHuys.Add(phieu);
        await db.SaveChangesAsync();
        return phieu;
    }

    static void KiemSoLuong(string tienTo, decimal sl, bool soNguyen)
    {
        if (sl <= 0) throw new NghiepVuException(tienTo + "số lượng hủy phải lớn hơn 0.");
        if (soNguyen && sl != decimal.Truncate(sl)) throw new NghiepVuException(tienTo + "số lượng bánh hủy phải là số nguyên.");
        SoLuongRule.Kiem(sl, "Số lượng hủy");
    }

    /// <summary>Phiếu nháp (ví dụ phiếu tự tạo khi bùng cọc) → chờ duyệt.</summary>
    public async Task GuiDuyetAsync(int phieuId)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.NHAP, "Chỉ gửi duyệt được phiếu đang nháp.");
        if (p.ChiTiets.Count == 0) throw new NghiepVuException("Phiếu hủy phải có ít nhất một dòng mới gửi duyệt được.");
        p.TrangThai = TrangThaiPhieu.CHO_DUYET;
        await LuuAsync();
    }

    public async Task HuyNhapAsync(int phieuId)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.NHAP, "Chỉ xóa được phiếu đang nháp.");
        db.PhieuXuatHuys.Remove(p);
        await LuuAsync();
    }

    /// <summary>UC18 bước 3: kiểm tồn hiện tại đủ cho từng lô (cộng dồn các dòng cùng lô), rồi trừ tồn và đóng phiếu. Dòng theo đơn chỉ ghi chi phí.</summary>
    public async Task DuyetAsync(int phieuId, int nguoiDuyetId)
    {
        await KiemQuanLyAsync(nguoiDuyetId);
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.CHO_DUYET, "Chỉ duyệt được phiếu đang chờ duyệt.");
        var nl = p.ChiTiets.Where(c => c.LoNguyenLieu is not null).GroupBy(c => c.LoNguyenLieu!).ToList();
        var banh = p.ChiTiets.Where(c => c.LoSanPham is not null).GroupBy(c => c.LoSanPham!).ToList();
        foreach (var g in nl)
            if (g.Sum(c => c.SoLuongHuy) > g.Key.SoLuongCon)
                throw new NghiepVuException($"Lô #{g.Key.Id} của {g.Key.NguyenLieu.TenGoi} chỉ còn {g.Key.SoLuongCon:0.###} nhưng phiếu hủy {g.Sum(c => c.SoLuongHuy):0.###}. Yêu cầu người lập điều chỉnh phiếu.");
        foreach (var g in banh)
            if (g.Sum(c => c.SoLuongHuy) > g.Key.SoLuongTon)
                throw new NghiepVuException($"Lô bánh #{g.Key.Id} ({g.Key.SanPham.TenBanh}) chỉ còn {g.Key.SoLuongTon} nhưng phiếu hủy {g.Sum(c => c.SoLuongHuy):0.###}. Yêu cầu người lập điều chỉnh phiếu.");

        foreach (var g in nl) g.Key.SoLuongCon -= g.Sum(c => c.SoLuongHuy);
        foreach (var g in banh) g.Key.SoLuongTon -= (int)g.Sum(c => c.SoLuongHuy);
        p.TrangThai = TrangThaiPhieu.DA_DUYET;
        p.NguoiDuyetId = nguoiDuyetId;
        p.NgayDuyet = BayGio;
        await LuuAsync();
    }

    public async Task TuChoiAsync(int phieuId, int nguoiDuyetId, string? lyDo)
    {
        await KiemQuanLyAsync(nguoiDuyetId);
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.CHO_DUYET, "Chỉ từ chối được phiếu đang chờ duyệt.");
        if (string.IsNullOrWhiteSpace(lyDo)) throw new NghiepVuException("Phải nhập lý do từ chối.");
        p.TrangThai = TrangThaiPhieu.TU_CHOI;
        p.NguoiDuyetId = nguoiDuyetId;
        p.NgayDuyet = BayGio;
        p.LyDoTuChoi = lyDo.Trim();
        await LuuAsync();
    }

    async Task KiemQuanLyAsync(int nhanVienId)
    {
        var nv = await db.NhanViens.FindAsync(nhanVienId) ?? throw new NghiepVuException("Không tìm thấy người duyệt.");
        if (nv.VaiTro != VaiTro.QUAN_LY) throw new NghiepVuException("Chỉ quản lý mới duyệt hoặc từ chối phiếu hủy.");
    }

    async Task<PhieuXuatHuy> LayCanTrangThai(int id, TrangThaiPhieu can, string thongBaoSai)
    {
        var p = await DayDu.FirstOrDefaultAsync(x => x.Id == id) ?? throw new NghiepVuException("Không tìm thấy phiếu hủy.");
        if (p.TrangThai != can) throw new NghiepVuException(thongBaoSai);
        return p;
    }

    Task LuuAsync() => db.LuuNghiepVuAsync(XungDot);
}
