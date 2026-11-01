using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record NgayBaoCao(DateOnly Ngay, decimal DoanhThu, decimal GiaVon, decimal ChiPhiHuy)
{
    public decimal LoiNhuanGop => DoanhThu - GiaVon - ChiPhiHuy;
}

public record BaoCaoDoanhThu(DateOnly Tu, DateOnly Den, decimal DoanhThu, decimal TienMat, decimal ChuyenKhoan,
    decimal GiaVon, decimal ChiPhiHuy, List<NgayBaoCao> TheoNgay)
{
    public decimal LoiNhuanGop => DoanhThu - GiaVon - ChiPhiHuy;
}

public record DongBan(DateOnly Ngay, int SanPhamId, int SoLuong, decimal ThanhTien);

public record BaoCaoTuyChinh(int SoDon, decimal DoanhThu, decimal GiaVon)
{
    public decimal TyLeLoiNhuan => DoanhThu == 0 ? 0 : (DoanhThu - GiaVon) / DoanhThu;
}

/// <summary>UC21, UC22: chỉ đọc. Doanh thu theo thời điểm thu tiền; giá vốn là ước tính theo BOM hiện tại.</summary>
public partial class BaoCaoService(FriendshipDb db, SanPhamService sanPham, TimeProvider? clock = null)
{
    public const int SoNgayToiDa = 366;

    /// <summary>Kỳ [tu 00:00, den+1 00:00): ngày kết thúc tính trọn ngày.</summary>
    static (DateTime Tu, DateTime Den) Khoang(DateOnly tu, DateOnly den)
    {
        if (tu > den) throw new NghiepVuException("Ngày bắt đầu không được sau ngày kết thúc.");
        if (den == DateOnly.MaxValue) throw new NghiepVuException("Ngày kết thúc quá lớn.");
        if (den.DayNumber - tu.DayNumber + 1 > SoNgayToiDa) throw new NghiepVuException($"Khoảng ngày tối đa {SoNgayToiDa} ngày.");
        return (tu.ToDateTime(TimeOnly.MinValue), den.AddDays(1).ToDateTime(TimeOnly.MinValue));
    }

    public async Task<BaoCaoDoanhThu> DoanhThuAsync(DateOnly tu, DateOnly den)
    {
        var (tuGio, denGio) = Khoang(tu, den);
        var thu = await db.ThanhToans.Where(t => t.ThoiGian >= tuGio && t.ThoiGian < denGio)
            .Select(t => new { t.ThoiGian, t.PhuongThuc, t.SoTien }).ToListAsync();
        var ban = await LayDongBanAsync(tuGio, denGio);
        var von = new Dictionary<int, decimal>();
        foreach (var id in ban.Select(b => b.SanPhamId).Distinct()) von[id] = (await sanPham.GiaVonAsync(id)).GiaVon;
        var huy = await db.PhieuXuatHuys
            .Where(p => p.TrangThai == TrangThaiPhieu.DA_DUYET && p.NgayDuyet >= tuGio && p.NgayDuyet < denGio)
            .Select(p => new { p.NgayDuyet, Tien = p.ChiTiets.Sum(c => c.SoLuongHuy * c.DonGiaVon) }).ToListAsync();

        var ngay = new List<NgayBaoCao>();
        for (var d = tu; d <= den; d = d.AddDays(1))
            ngay.Add(new NgayBaoCao(d,
                thu.Where(t => DateOnly.FromDateTime(t.ThoiGian) == d).Sum(t => t.SoTien),
                ban.Where(b => b.Ngay == d).Sum(b => b.SoLuong * von[b.SanPhamId]),
                huy.Where(h => DateOnly.FromDateTime(h.NgayDuyet!.Value) == d).Sum(h => h.Tien)));

        return new BaoCaoDoanhThu(tu, den,
            thu.Sum(t => t.SoTien),
            thu.Where(t => t.PhuongThuc == PhuongThucTT.TIEN_MAT).Sum(t => t.SoTien),
            thu.Where(t => t.PhuongThuc == PhuongThucTT.CHUYEN_KHOAN).Sum(t => t.SoTien),
            ngay.Sum(x => x.GiaVon), ngay.Sum(x => x.ChiPhiHuy), ngay);
    }

    /// <summary>UC21 A1: các ca mở trong kỳ, mới nhất trước.</summary>
    public async Task<List<CaThuNgan>> CaTrongKyAsync(DateOnly tu, DateOnly den)
    {
        var (tuGio, denGio) = Khoang(tu, den);
        return await db.CaThuNgans.Include(c => c.NhanVien)
            .Where(c => c.GioMoCa >= tuGio && c.GioMoCa < denGio)
            .OrderByDescending(c => c.GioMoCa).ThenByDescending(c => c.Id).ToListAsync();
    }

    /// <summary>Dòng bánh đã bán trong kỳ: bán lẻ theo ngày lập; đơn đặt đã giao theo khoản thanh toán cuối cùng. Đơn hủy, bùng cọc không có dòng bán.</summary>
    internal async Task<List<DongBan>> LayDongBanAsync(DateTime tu, DateTime den)
    {
        var ds = await db.DonHangs.Include(d => d.ChiTiets).Include(d => d.ThanhToans)
            .Where(d => (d.TrangThai == TrangThaiDon.DA_THANH_TOAN && d.NgayLap >= tu && d.NgayLap < den)
                     || (d.TrangThai == TrangThaiDon.DA_GIAO && d.ThanhToans.Any(t => t.ThoiGian >= tu && t.ThoiGian < den)))
            .ToListAsync();
        var kq = new List<DongBan>();
        foreach (var d in ds)
        {
            var luc = d.TrangThai == TrangThaiDon.DA_THANH_TOAN ? d.NgayLap : d.ThanhToans.Max(t => t.ThoiGian);
            if (luc < tu || luc >= den) continue;
            foreach (var c in d.ChiTiets) kq.Add(new DongBan(DateOnly.FromDateTime(luc), c.SanPhamId, c.SoLuong, c.ThanhTien));
        }
        return kq;
    }

    public async Task<BaoCaoTuyChinh> TuyChinhAsync(DateOnly tu, DateOnly den)
    {
        var (tuGio, denGio) = Khoang(tu, den);
        var ds = await db.DonHangs
            .Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh)
            .Include(d => d.ThanhToans)
            .Where(d => d.TrangThai == TrangThaiDon.DA_GIAO
                     && d.ChiTiets.Any(c => c.ThietKeBanhId != null)
                     && d.ThanhToans.Any(t => t.ThoiGian >= tuGio && t.ThoiGian < denGio))
            .ToListAsync();

        var donTrongKy = ds.Where(d =>
        {
            if (d.ThanhToans.Count == 0) return false;
            var luc = d.ThanhToans.Max(t => t.ThoiGian);
            return luc >= tuGio && luc < denGio;
        }).ToList();

        var soDon = donTrongKy.Count;
        var doanhThu = donTrongKy.Sum(d => d.TongThanhToan);
        var giaVon = donTrongKy.Sum(d => d.ChiTiets
            .Where(c => c.ThietKeBanh != null)
            .Sum(c => c.ThietKeBanh!.GiaVonDuKien));

        return new BaoCaoTuyChinh(soDon, doanhThu, giaVon);
    }
}
