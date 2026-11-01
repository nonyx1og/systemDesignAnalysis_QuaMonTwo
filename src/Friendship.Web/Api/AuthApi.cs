using System.Security.Claims;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;

namespace Friendship.Web.Api;

public record DangNhapApi(string? TenDangNhap, string? MatKhau);
public record DoiMatKhauApi(string? MatKhauCu, string? MatKhauMoi);

public static class AuthApi
{
    public static void MapAuthApi(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/auth");

        // Token gắn với danh tính hiện tại: lấy lại sau khi đăng nhập hoặc đăng xuất.
        g.MapGet("/csrf", (HttpContext http, IAntiforgery af) => new { token = af.GetAndStoreTokens(http).RequestToken })
            .AllowAnonymous();

        g.MapPost("/login", async (DangNhapApi b, TaiKhoanService tk, HttpContext http) =>
        {
            NhanVien nv;
            try { nv = await tk.DangNhapAsync(b.TenDangNhap, b.MatKhau); }
            catch (NghiepVuException e) { return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: e.Message); }
            await SignInHelper.SignInAsync(http, nv);
            return Results.Ok(nv.ToDto());
        }).AllowAnonymous();

        g.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync();
            return Results.NoContent();
        });

        g.MapGet("/me", async (ClaimsPrincipal u, NhanVienService nv) => (await nv.LayAsync(u.NhanVienId()))!.ToDto());

        g.MapPost("/doi-mat-khau", async (DoiMatKhauApi b, ClaimsPrincipal u, TaiKhoanService tk, HttpContext http) =>
        {
            var nv = await tk.DoiMatKhauAsync(u.NhanVienId(), b.MatKhauCu, b.MatKhauMoi);
            await SignInHelper.SignInAsync(http, nv);         // phiên này giữ lại, cookie cũ ở nơi khác mất hiệu lực
            return Results.NoContent();
        });
    }
}
