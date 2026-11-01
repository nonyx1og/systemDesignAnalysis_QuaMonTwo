using Friendship.Domain;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Api;

public record TienDoApi(TrangThaiDon TrangThai);
public record MeBanhApi(int SanPhamId, int SoLuong, DateTime? GioRaLo);
public record XaHangApi(List<int>? LoIds, decimal PhanTram);

public static class BepApi
{
    public static void MapBepApi(this RouteGroupBuilder api)
    {
        var don = api.MapGroup("/don-hang");
        don.MapGet("/can-lam", async (DateOnly? ngay, BepService s) =>
            (await s.DonCanLamAsync(ngay)).Select(b => new DonBepDto(b.Don.Id, b.Don.Ma, b.Don.TrangThai, b.Don.NgayGioLay, b.TongBanh,
                b.KhanCap, b.Don.ChiTiets.Select(c => c.ToDto()).ToList())))
            .RequireAuthorization(Policies.SanXuat);

        // UC11: bếp chỉ đẩy đơn sang Đang làm hoặc Chờ giao; thứ tự hợp lệ do TrangThaiDonRule kiểm.
        don.MapPatch("/{id:int}/tien-do", async (int id, TienDoApi b, BepService s) =>
        {
            if (b.TrangThai == TrangThaiDon.DANG_LAM) await s.BatDauLamAsync(id);
            else if (b.TrangThai == TrangThaiDon.CHO_GIAO) await s.ChoGiaoAsync(id);
            else throw new NghiepVuException("Bếp chỉ chuyển đơn sang DANG_LAM hoặc CHO_GIAO.");
            return Results.NoContent();
        }).RequireAuthorization(Policies.SanXuat);

        var lo = api.MapGroup("/lo-san-pham");
        // Lô còn bán được (còn tồn, chưa hết hạn), hạn gần nhất trước.
        lo.MapGet("/", async (FriendshipDb db, TimeProvider clock) =>
        {
            var bay = clock.GetLocalNow().DateTime;
            return (await db.LoSanPhams.Include(l => l.SanPham).Where(l => l.SoLuongTon > 0 && l.HanSuDung > bay)
                .OrderBy(l => l.HanSuDung).ThenBy(l => l.Id).ToListAsync()).Select(l => l.ToDto());
        }).RequireAuthorization(p => p.RequireRole(Policies.BanHang, Policies.SanXuat, Policies.QuanLy));

        lo.MapGet("/{id:int}", async (int id, FriendshipDb db) =>
            await db.LoSanPhams.Include(l => l.SanPham).FirstOrDefaultAsync(l => l.Id == id) is { } l ? Results.Ok(l.ToDto()) : Results.NotFound())
            .RequireAuthorization(p => p.RequireRole(Policies.BanHang, Policies.SanXuat, Policies.QuanLy));

        lo.MapPost("/", async (MeBanhApi b, LoBanhService s, FriendshipDb db) =>
        {
            var l = await s.GhiNhanMeAsync(b.SanPhamId, b.SoLuong, b.GioRaLo);
            await db.Entry(l).Reference(x => x.SanPham).LoadAsync();
            return Results.Created($"/api/lo-san-pham/{l.Id}", l.ToDto());
        }).RequireAuthorization(Policies.SanXuat);

        lo.MapGet("/xa-hang", async (XaHangService s) =>
            (await s.DanhSachAsync()).Select(x => new LoXaHangDto(x.Lo.ToDto(), x.ConLai.TotalMinutes, x.GiaGoc, x.GiaSauGiam)))
            .RequireAuthorization(Policies.QuanLy);

        lo.MapPut("/xa-hang", async (XaHangApi b, XaHangService s) =>
            new { soLo = await s.ThietLapAsync(b.LoIds ?? [], b.PhanTram) })
            .RequireAuthorization(Policies.QuanLy);
    }
}
