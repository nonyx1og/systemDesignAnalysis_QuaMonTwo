using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;

namespace Friendship.Web.Api;

public record SanPhamApi(string TenBanh, string KichThuoc, decimal DonGia, int HanSuDungGio, List<DinhLuongInput>? Bom);
public record KinhDoanhApi(bool KinhDoanh);
public record NguyenLieuApi(string TenGoi, string DonViTinh, decimal MucCanhBaoTon);
public record KhuyenMaiApi(string Ma, string Ten, LoaiGiam Loai, decimal GiaTri, decimal DonToiThieu, DateOnly NgayBatDau, DateOnly NgayKetThuc);
public record ApDungApi(bool ApDung);

public static class DanhMucApi
{
    public static void MapDanhMucApi(this RouteGroupBuilder api)
    {
        // ----- Sản phẩm: ai đăng nhập cũng đọc được (POS, bếp, kho đều cần); ghi chỉ quản lý -----
        var sp = api.MapGroup("/san-pham");
        sp.MapGet("/", async (string? q, bool? dangKinhDoanh, SanPhamService s) =>
            (await s.TimAsync(q, dangKinhDoanh)).Select(x => x.ToDto()));
        sp.MapGet("/{id:int}", async (int id, SanPhamService s) =>
            await s.LayAsync(id) is { } x ? Results.Ok(x.ToDto()) : Results.NotFound());
        sp.MapGet("/{id:int}/gia-von", async (int id, SanPhamService s) =>
            await s.LayAsync(id) is null ? Results.NotFound() : Results.Ok(await s.GiaVonAsync(id)))
            .RequireAuthorization(Policies.QuanLy);
        sp.MapPost("/", async (SanPhamApi b, SanPhamService s) =>
        {
            var x = await s.LuuAsync(new(null, b.TenBanh, b.KichThuoc, b.DonGia, b.HanSuDungGio, b.Bom ?? []));
            return Results.Created($"/api/san-pham/{x.Id}", (await s.LayAsync(x.Id))!.ToDto());
        }).RequireAuthorization(Policies.QuanLy);
        sp.MapPut("/{id:int}", async (int id, SanPhamApi b, SanPhamService s) =>
        {
            await s.LuuAsync(new(id, b.TenBanh, b.KichThuoc, b.DonGia, b.HanSuDungGio, b.Bom ?? []));
            return Results.Ok((await s.LayAsync(id))!.ToDto());
        }).RequireAuthorization(Policies.QuanLy);
        sp.MapPatch("/{id:int}/kinh-doanh", async (int id, KinhDoanhApi b, SanPhamService s) =>
        {
            await s.DatKinhDoanhAsync(id, b.KinhDoanh);
            return Results.NoContent();
        }).RequireAuthorization(Policies.QuanLy);

        // ----- Nguyên liệu: kho và quản lý đọc; quản lý ghi -----
        var nl = api.MapGroup("/nguyen-lieu").RequireAuthorization(p => p.RequireRole(Policies.Kho, Policies.QuanLy));
        nl.MapGet("/", async (string? q, NguyenLieuService s) =>
            (await s.TimAsync(q)).Select(x => x.NguyenLieu.ToDto(x.TongTon)));
        nl.MapGet("/{id:int}", async (int id, NguyenLieuService s) =>
            await s.LayAsync(id) is { } x ? Results.Ok(x.ToDto()) : Results.NotFound());
        nl.MapPost("/", async (NguyenLieuApi b, NguyenLieuService s) =>
        {
            var x = await s.LuuAsync(new(null, b.TenGoi, b.DonViTinh, b.MucCanhBaoTon));
            return Results.Created($"/api/nguyen-lieu/{x.Id}", x.ToDto());
        }).RequireAuthorization(Policies.QuanLy);
        nl.MapPut("/{id:int}", async (int id, NguyenLieuApi b, NguyenLieuService s) =>
            Results.Ok((await s.LuuAsync(new(id, b.TenGoi, b.DonViTinh, b.MucCanhBaoTon))).ToDto()))
            .RequireAuthorization(Policies.QuanLy);
        nl.MapDelete("/{id:int}", async (int id, NguyenLieuService s) =>
        {
            await s.XoaAsync(id);
            return Results.NoContent();
        }).RequireAuthorization(Policies.QuanLy);

        // ----- Khuyến mãi: quản lý -----
        var km = api.MapGroup("/khuyen-mai").RequireAuthorization(Policies.QuanLy);
        km.MapGet("/", async (string? q, KhuyenMaiService s, TimeProvider clock) =>
        {
            var homNay = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
            return (await s.TimAsync(q)).Select(x => x.ToDto(homNay));
        });
        km.MapGet("/{id:int}", async (int id, KhuyenMaiService s, TimeProvider clock) =>
            await s.LayAsync(id) is { } x ? Results.Ok(x.ToDto(DateOnly.FromDateTime(clock.GetLocalNow().DateTime))) : Results.NotFound());
        km.MapPost("/", async (KhuyenMaiApi b, KhuyenMaiService s, TimeProvider clock) =>
        {
            var x = await s.LuuAsync(new(null, b.Ma, b.Ten, b.Loai, b.GiaTri, b.DonToiThieu, b.NgayBatDau, b.NgayKetThuc));
            return Results.Created($"/api/khuyen-mai/{x.Id}", x.ToDto(DateOnly.FromDateTime(clock.GetLocalNow().DateTime)));
        });
        km.MapPut("/{id:int}", async (int id, KhuyenMaiApi b, KhuyenMaiService s, TimeProvider clock) =>
            Results.Ok((await s.LuuAsync(new(id, b.Ma, b.Ten, b.Loai, b.GiaTri, b.DonToiThieu, b.NgayBatDau, b.NgayKetThuc)))
                .ToDto(DateOnly.FromDateTime(clock.GetLocalNow().DateTime))));
        km.MapPatch("/{id:int}/ap-dung", async (int id, ApDungApi b, KhuyenMaiService s) =>
        {
            await s.DatApDungAsync(id, b.ApDung);
            return Results.NoContent();
        });
    }
}
