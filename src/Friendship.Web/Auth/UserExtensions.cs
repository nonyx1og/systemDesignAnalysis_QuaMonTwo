using System.Security.Claims;
using Friendship.Domain;

namespace Friendship.Web.Auth;

public static class UserExtensions
{
    public static int NhanVienId(this ClaimsPrincipal u) => int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public static VaiTro VaiTroHienTai(this ClaimsPrincipal u) => Enum.Parse<VaiTro>(u.FindFirstValue(ClaimTypes.Role)!);
}
