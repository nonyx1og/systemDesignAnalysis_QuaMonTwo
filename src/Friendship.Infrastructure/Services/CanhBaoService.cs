using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record CanhBaoNguyenLieu(LoNguyenLieu Lo, TrangThaiDate TrangThai, int NgayConLai);
public record CanhBaoBanh(LoSanPham Lo, TrangThaiDate TrangThai, TimeSpan ConLai);

/// <summary>UC15: chỉ đọc. Vàng = cận date, Đỏ = hết hạn (BR07); sắp theo HSD tăng dần.</summary>
public class CanhBaoService(FriendshipDb db, TimeProvider clock)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;
    DateOnly HomNay => DateOnly.FromDateTime(BayGio);

    public async Task<List<CanhBaoNguyenLieu>> NguyenLieuAsync(int? nguyenLieuId = null)
    {
        var homNay = HomNay;
        var han = homNay.AddDays(HanSuDungRule.NguongNgayNguyenLieu);
        var q = db.LoNguyenLieus.Include(l => l.NguyenLieu).Where(l => l.SoLuongCon > 0 && l.HanSuDung <= han);
        if (nguyenLieuId is { } id) q = q.Where(l => l.NguyenLieuId == id);
        var ds = await q.ToListAsync();
        return ds.OrderBy(l => l.HanSuDung).ThenBy(l => l.Id)
            .Select(l => new CanhBaoNguyenLieu(l, l.TrangThaiDate(homNay), l.HanSuDung.DayNumber - homNay.DayNumber))
            .ToList();
    }

    public async Task<List<CanhBaoBanh>> BanhAsync(int? sanPhamId = null)
    {
        var bay = BayGio;
        var han = bay.AddHours(HanSuDungRule.NguongGioBanh);
        var q = db.LoSanPhams.Include(l => l.SanPham).Where(l => l.SoLuongTon > 0 && l.HanSuDung <= han);
        if (sanPhamId is { } id) q = q.Where(l => l.SanPhamId == id);
        var ds = await q.ToListAsync();
        return ds.OrderBy(l => l.HanSuDung).ThenBy(l => l.Id)
            .Select(l => new CanhBaoBanh(l, l.TrangThaiDate(bay), l.HanSuDung - bay))
            .ToList();
    }

    /// <summary>Số cảnh báo hiển thị trên chuông, theo phạm vi vai trò (UC15 bước 1).</summary>
    public async Task<int> DemAsync(VaiTro vaiTro) => vaiTro switch
    {
        VaiTro.KHO => (await NguyenLieuAsync()).Count,
        VaiTro.BAN_HANG => (await BanhAsync()).Count,
        VaiTro.QUAN_LY => (await NguyenLieuAsync()).Count + (await BanhAsync()).Count,
        _ => 0
    };
}
