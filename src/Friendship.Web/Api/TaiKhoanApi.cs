using System.Security.Claims;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;

namespace Friendship.Web.Api;

public record TaiKhoanApi(string HoTen, string? GioiTinh, DateOnly? NgaySinh, string SoDienThoai, string? Email,
    string TenDangNhap, VaiTro VaiTro, string? MatKhauTam);
public record TrangThaiTaiKhoanApi(bool HoatDong);
public record DatLaiMatKhauApi(string? MatKhauMoi);

public static class TaiKhoanApiMap
{
    public static void MapTaiKhoanApi(this RouteGroupBuilder api)
    {
        var tk = api.MapGroup("/tai-khoan").RequireAuthorization(Policies.QuanTri);
        static NhanVienInput Input(TaiKhoanApi b) => new(b.HoTen, b.GioiTinh, b.NgaySinh, b.SoDienThoai, b.Email, b.TenDangNhap, b.VaiTro);

        tk.MapGet("/", async (string? q, VaiTro? vaiTro, NhanVienService s) => (await s.TimAsync(q, vaiTro)).Select(x => x.ToDto()));
        tk.MapGet("/{id:int}", async (int id, NhanVienService s) =>
            await s.LayAsync(id) is { } x ? Results.Ok(x.ToDto()) : Results.NotFound());
        tk.MapPost("/", async (TaiKhoanApi b, NhanVienService s) =>
        {
            var x = await s.TaoAsync(Input(b), b.MatKhauTam ?? "");
            return Results.Created($"/api/tai-khoan/{x.Id}", x.ToDto());
        });
        tk.MapPut("/{id:int}", async (int id, TaiKhoanApi b, ClaimsPrincipal u, NhanVienService s) =>
        {
            await s.CapNhatAsync(id, Input(b), u.NhanVienId());
            return Results.Ok((await s.LayAsync(id))!.ToDto());
        });
        tk.MapPatch("/{id:int}/trang-thai", async (int id, TrangThaiTaiKhoanApi b, ClaimsPrincipal u, NhanVienService s) =>
        {
            await s.DatTrangThaiAsync(id, b.HoatDong, u.NhanVienId());
            return Results.NoContent();
        });
        tk.MapPost("/{id:int}/dat-lai-mat-khau", async (int id, DatLaiMatKhauApi b, NhanVienService s) =>
        {
            await s.DatLaiMatKhauAsync(id, b.MatKhauMoi ?? "");
            return Results.NoContent();
        });
    }
}
