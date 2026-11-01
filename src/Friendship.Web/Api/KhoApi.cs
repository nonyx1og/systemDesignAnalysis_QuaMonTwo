using System.Security.Claims;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Friendship.Web.Api;

public record TuChoiApi(string? LyDo);

public static class KhoApi
{
    public static void MapKhoApi(this RouteGroupBuilder api)
    {
        var kho = api.MapGroup("/kho");
        kho.MapPost("/nhap", async ([FromBody] List<DongNhapKho> dong, KhoService s) =>
            (await s.NhapKhoAsync(dong)).Select(l => l.ToDto()))
            .RequireAuthorization(Policies.Kho);
        kho.MapPost("/xuat/ke-hoach", async ([FromBody] List<YeuCauXuat> yc, KhoService s) =>
            (await s.LapKeHoachXuatAsync(yc)).ToDto())
            .RequireAuthorization(Policies.Kho);
        kho.MapPost("/xuat", async ([FromBody] List<YeuCauXuat> yc, KhoService s) =>
        {
            var kq = await s.XuatAsync(yc);
            return new KetQuaXuatDto(kq.KeHoach.ToDto(), kq.CanhBaoTon);
        }).RequireAuthorization(Policies.Kho);
        kho.MapGet("/don/{id:int}/yeu-cau-xuat", async (int id, DonDatService s) => await s.YeuCauXuatTuDonAsync(id))
            .RequireAuthorization(Policies.Kho);
        kho.MapGet("/de-xuat-nhap", async (KhoService s) =>
            (await s.LayDeXuatNhapAsync()).ToDto())
            .RequireAuthorization(p => p.RequireRole(Policies.Kho, Policies.QuanLy));

        // ----- Kiểm kê (UC16): kho lập, quản lý duyệt -----
        var kk = kho.MapGroup("/kiem-ke");
        kk.MapGet("/", async (TrangThaiPhieu? trangThai, KiemKeService s) =>
            (await s.TimAsync(trangThai)).Select(p => p.ToDto()))           // danh sách: ChiTiets rỗng, xem chi tiết ở GET /{id}
            .RequireAuthorization(p => p.RequireRole(Policies.Kho, Policies.QuanLy));
        kk.MapGet("/{id:int}", async (int id, KiemKeService s) =>
            await s.LayAsync(id) is { } p ? Results.Ok(p.ToDto()) : Results.NotFound())
            .RequireAuthorization(p => p.RequireRole(Policies.Kho, Policies.QuanLy));
        kk.MapPost("/", async (ClaimsPrincipal u, KiemKeService s) =>
        {
            var p = await s.TaoPhieuAsync(u.NhanVienId());
            return Results.Created($"/api/kho/kiem-ke/{p.Id}", (await s.LayAsync(p.Id))!.ToDto());
        }).RequireAuthorization(Policies.Kho);
        kk.MapPut("/{id:int}/so-lieu", async (int id, [FromBody] List<DongKiemKeInput> dong, KiemKeService s) =>
        {
            await s.LuuSoLieuAsync(id, dong);
            return Results.NoContent();
        }).RequireAuthorization(Policies.Kho);
        kk.MapPost("/{id:int}/gui-duyet", async (int id, KiemKeService s) => { await s.GuiDuyetAsync(id); return Results.NoContent(); })
            .RequireAuthorization(Policies.Kho);
        kk.MapDelete("/{id:int}", async (int id, KiemKeService s) => { await s.HuyNhapAsync(id); return Results.NoContent(); })
            .RequireAuthorization(Policies.Kho);
        kk.MapPost("/{id:int}/duyet", async (int id, ClaimsPrincipal u, KiemKeService s) =>
        {
            await s.DuyetAsync(id, u.NhanVienId());
            return Results.NoContent();
        }).RequireAuthorization(Policies.QuanLy);
        kk.MapPost("/{id:int}/tu-choi", async (int id, TuChoiApi b, ClaimsPrincipal u, KiemKeService s) =>
        {
            await s.TuChoiAsync(id, u.NhanVienId(), b.LyDo);
            return Results.NoContent();
        }).RequireAuthorization(Policies.QuanLy);
    }
}
