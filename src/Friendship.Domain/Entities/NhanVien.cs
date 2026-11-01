namespace Friendship.Domain.Entities;

public class NhanVien
{
    public int Id { get; set; }
    public string HoTen { get; set; } = "";
    public string? GioiTinh { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string SoDienThoai { get; set; } = "";
    public string? Email { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public VaiTro VaiTro { get; set; }
    public bool DangHoatDong { get; set; } = true;

    /// <summary>Dấu vân tay của mật khẩu hiện tại; đổi mật khẩu thì mã đổi, làm cookie đăng nhập cũ mất hiệu lực.</summary>
    public static string MaXacThuc(string matKhauHash) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(matKhauHash)))[..16];
}
