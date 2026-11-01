using System.Text.RegularExpressions;

namespace Friendship.Infrastructure.Services;

/// <summary>Định dạng hợp lệ của tên đăng nhập (đã chữ thường). Chặn cả ký tự toàn chiều mà collation SQL Server coi là trùng ký tự thường.</summary>
public static partial class TenDangNhapRule
{
    [GeneratedRegex(@"^[a-z0-9._]{3,30}$")] private static partial Regex Re();

    public static bool HopLe(string tenDaChuanHoa) => Re().IsMatch(tenDaChuanHoa);
}
