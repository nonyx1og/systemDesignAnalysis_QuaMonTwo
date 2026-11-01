using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DonBep(DonHang Don, int TongBanh, bool KhanCap);

/// <summary>UC10, UC11: màn hình bếp. Chỉ đơn đặt trước đã cọc (BR03), sắp theo giờ hẹn (BR04).</summary>
public class BepService(FriendshipDb db, TimeProvider clock)
{
    public static readonly TimeSpan NguongKhanCap = TimeSpan.FromHours(2);

    DateTime BayGio => clock.GetLocalNow().DateTime;

    public async Task<List<DonBep>> DonCanLamAsync(DateOnly? ngay = null)
    {
        var bay = BayGio;
        var q = db.DonHangs.Include(d => d.ChiTiets).ThenInclude(c => c.SanPham)
            .Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh).ThenInclude(tk => tk!.ThanhPhans).ThenInclude(tp => tp.ThanhPhan)
            .Where(d => d.LoaiDon == LoaiDon.DAT_TRUOC
                && (d.TrangThai == TrangThaiDon.DA_COC || d.TrangThai == TrangThaiDon.DANG_LAM));
        if (ngay is { } n)
        {
            var tu = n.ToDateTime(TimeOnly.MinValue);
            var den = tu.AddDays(1);
            q = q.Where(d => d.NgayGioLay >= tu && d.NgayGioLay < den);
        }
        var ds = await q.OrderBy(d => d.NgayGioLay).ThenBy(d => d.Id).ToListAsync();
        return ds.Select(d => new DonBep(d, d.ChiTiets.Sum(c => c.SoLuong), d.NgayGioLay!.Value - bay <= NguongKhanCap)).ToList();
    }

    public async Task<List<DonBep>> DonChoGiaoAsync(DateOnly? ngay = null)
    {
        var q = db.DonHangs.Include(d => d.ChiTiets).ThenInclude(c => c.SanPham)
            .Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh).ThenInclude(tk => tk!.ThanhPhans).ThenInclude(tp => tp.ThanhPhan)
            .Where(d => d.LoaiDon == LoaiDon.DAT_TRUOC && d.TrangThai == TrangThaiDon.CHO_GIAO);
        if (ngay is { } n)
        {
            var tu = n.ToDateTime(TimeOnly.MinValue);
            var den = tu.AddDays(1);
            q = q.Where(d => d.NgayGioLay >= tu && d.NgayGioLay < den);
        }
        var ds = await q.OrderByDescending(d => d.NgayGioLay).ThenByDescending(d => d.Id).ToListAsync();
        return ds.Select(d => new DonBep(d, d.ChiTiets.Sum(c => c.SoLuong), false)).ToList();
    }

    public Task<DonHang> BatDauLamAsync(int donId) => DoiAsync(donId, TrangThaiDon.DANG_LAM);

    /// <summary>Cho phép chạm thẳng từ DA_COC khi làm nhanh (UC11 A1).</summary>
    public Task<DonHang> ChoGiaoAsync(int donId) => DoiAsync(donId, TrangThaiDon.CHO_GIAO);

    async Task<DonHang> DoiAsync(int donId, TrangThaiDon moi)
    {
        var don = await db.DonHangs.FirstOrDefaultAsync(d => d.Id == donId) ?? throw new NghiepVuException("Không tìm thấy đơn.");
        if (don.TrangThai is TrangThaiDon.DA_HUY or TrangThaiDon.BUNG_COC)
            throw new NghiepVuException("Đơn đã bị hủy, không cập nhật được.");        // UC11 E1
        TrangThaiDonRule.Chuyen(don, moi);                                              // UC11 E2: lùi/sai thứ tự bị từ chối
        await db.LuuNghiepVuAsync("Đơn vừa được cập nhật ở nơi khác. Hãy tải lại màn hình bếp.");
        return don;
    }
}
