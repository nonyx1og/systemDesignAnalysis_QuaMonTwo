using System.Security.Claims;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Auth;

public static class CustomerAuthHelper
{
    public const string Scheme = "KhachHang";
    public const string ClaimLoai = "loai_nguoidung";
    public const string GiaTriLoai = "khach_hang";

    public static async Task SignInCustomerAsync(HttpContext ctx, KhachHang kh)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kh.Id.ToString()),
            new(ClaimTypes.Name, kh.TenKhachHang),
            new(ClaimTypes.MobilePhone, kh.SoDienThoai),
            new(ClaimLoai, GiaTriLoai)
        };
        if (!string.IsNullOrEmpty(kh.Email))
            claims.Add(new Claim(ClaimTypes.Email, kh.Email));

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);

        await ctx.SignInAsync(Scheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        });
    }

    public static async Task SignOutCustomerAsync(HttpContext ctx) =>
        await ctx.SignOutAsync(Scheme);

    public static async Task<KhachHang?> GetCurrentCustomerAsync(HttpContext ctx, FriendshipDb db)
    {
        var authResult = await ctx.AuthenticateAsync(Scheme);
        if (!authResult.Succeeded || authResult.Principal is null) return null;

        var idStr = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idStr, out var id))
        {
            return await db.KhachHangs.FirstOrDefaultAsync(k => k.Id == id);
        }
        return null;
    }
}
