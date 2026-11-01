using System.Security.Claims;
using Friendship.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Friendship.Web.Auth;

public static class SignInHelper
{
    public const string ClaimMaXacThuc = "ma_xac_thuc";

    /// <summary>Cấp cookie đăng nhập; mã xác thực gắn với mật khẩu hiện tại để đổi mật khẩu làm cookie cũ mất hiệu lực.</summary>
    public static Task SignInAsync(HttpContext ctx, NhanVien nv) => ctx.SignInAsync(new ClaimsPrincipal(
        new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, nv.Id.ToString()),
            new Claim(ClaimTypes.Name, nv.HoTen),
            new Claim(ClaimTypes.Role, nv.VaiTro.ToString()),
            new Claim(ClaimMaXacThuc, NhanVien.MaXacThuc(nv.MatKhauHash)),
        ], CookieAuthenticationDefaults.AuthenticationScheme)));
}
