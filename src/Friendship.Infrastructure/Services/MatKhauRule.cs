using Friendship.Domain;

namespace Friendship.Infrastructure.Services;

public static class MatKhauRule
{
    public const int DoDaiToiThieu = 8;

    public static void Kiem(string? matKhau)
    {
        if (matKhau is null || matKhau.Length < DoDaiToiThieu)
            throw new NghiepVuException($"Mật khẩu phải có ít nhất {DoDaiToiThieu} ký tự.");
    }
}
