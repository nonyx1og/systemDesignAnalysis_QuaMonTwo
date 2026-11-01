using System.Security.Claims;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Friendship.Web.Api;

public static class HuyBaoCaoApi
{
    public static void MapHuyBaoCaoApi(this RouteGroupBuilder api)
    {
        // ----- Phiếu hủy (UC17, UC18): người lập thấy phiếu của mình, quản lý thấy mọi phiếu -----
        var huy = api.MapGroup("/huy-hang")
            .RequireAuthorization(p => p.RequireRole(Policies.BanHang, Policies.SanXuat, Policies.Kho, Policies.QuanLy));

        huy.MapGet("/", async (TrangThaiPhieu? trangThai, ClaimsPrincipal u, HuyHangService s) =>
            (await s.TimAsync(trangThai, u.IsInRole(Policies.QuanLy) ? null : u.NhanVienId())).Select(p => p.ToDto()));

        huy.MapGet("/{id:int}", async (int id, ClaimsPrincipal u, HuyHangService s) =>
            await s.LayAsync(id) is { } p && (u.IsInRole(Policies.QuanLy) || p.NguoiLapId == u.NhanVienId())
                ? Results.Ok(p.ToDto()) : Results.NotFound());

        huy.MapGet("/lua-chon", async (HuyHangService s) =>
        {
            var lc = await s.LuaChonAsync();
            return new LuaChonHuyDto(lc.LoNguyenLieu.Select(l => l.ToDto()).ToList(), lc.LoBanh.Select(l => l.ToDto()).ToList(),
                lc.DonBungCoc.Select(d => d.ToTomTat()).ToList());
        }).RequireAuthorization(Policies.HuyHang);

        huy.MapPost("/", async ([FromBody] List<DongHuyInput> dong, ClaimsPrincipal u, HuyHangService s) =>
        {
            var p = await s.LapAsync(u.NhanVienId(), dong);
            return Results.Created($"/api/huy-hang/{p.Id}", (await s.LayAsync(p.Id))!.ToDto());
        }).RequireAuthorization(Policies.HuyHang);

        huy.MapPost("/{id:int}/gui-duyet", async (int id, ClaimsPrincipal u, HuyHangService s) =>
        {
            if (await s.LayAsync(id) is not { } p || p.NguoiLapId != u.NhanVienId()) return Results.NotFound();
            await s.GuiDuyetAsync(id);
            return Results.NoContent();
        }).RequireAuthorization(Policies.HuyHang);

        huy.MapDelete("/{id:int}", async (int id, ClaimsPrincipal u, HuyHangService s) =>
        {
            if (await s.LayAsync(id) is not { } p || p.NguoiLapId != u.NhanVienId()) return Results.NotFound();
            await s.HuyNhapAsync(id);
            return Results.NoContent();
        }).RequireAuthorization(Policies.HuyHang);

        huy.MapPost("/{id:int}/duyet", async (int id, ClaimsPrincipal u, HuyHangService s) =>
        {
            await s.DuyetAsync(id, u.NhanVienId());
            return Results.NoContent();
        }).RequireAuthorization(Policies.QuanLy);

        huy.MapPost("/{id:int}/tu-choi", async (int id, TuChoiApi b, ClaimsPrincipal u, HuyHangService s) =>
        {
            await s.TuChoiAsync(id, u.NhanVienId(), b.LyDo);
            return Results.NoContent();
        }).RequireAuthorization(Policies.QuanLy);

        // ----- Cảnh báo HSD (UC15) -----
        api.MapGet("/canh-bao", async (CanhBaoService s) => new
        {
            nguyenLieu = (await s.NguyenLieuAsync()).Select(c => new CanhBaoNguyenLieuDto(c.Lo.ToDto(), c.TrangThai, c.NgayConLai)),
            banh = (await s.BanhAsync()).Select(c => new CanhBaoBanhDto(c.Lo.ToDto(), c.TrangThai, c.ConLai.TotalMinutes))
        }).RequireAuthorization(Policies.CanhBao);

        // ----- Báo cáo (UC21, UC22): quản lý -----
        var bc = api.MapGroup("/bao-cao").RequireAuthorization(Policies.QuanLy);
        bc.MapGet("/doanh-thu", async (DateOnly tu, DateOnly den, BaoCaoService s) => await s.DoanhThuAsync(tu, den));
        bc.MapGet("/ca", async (DateOnly tu, DateOnly den, BaoCaoService s) => (await s.CaTrongKyAsync(tu, den)).Select(c => c.ToDto()));
        bc.MapGet("/huy-ban-chay", async (DateOnly tu, DateOnly den, int? top, LocHuy? loc, BaoCaoService s) =>
            await s.HuyVaBanChayAsync(tu, den, top ?? 5, loc ?? LocHuy.TatCa));
        bc.MapGet("/gia-tri-ton-kho", async (string? tuKhoa, BaoCaoService s) =>
            await s.GiaTriTonKhoAsync(tuKhoa));
    }
}
