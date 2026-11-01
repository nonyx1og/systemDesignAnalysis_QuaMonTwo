namespace Friendship.Domain.Entities;

public class KhachHang
{
    public int Id { get; set; }
    public string TenKhachHang { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public int DiemTichLuy { get; set; }
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string? MatKhauHash { get; set; }
    public DateTime? NgayTao { get; set; } = DateTime.Now;

    /// <summary>Chống hai thu ngân cùng dùng/cộng điểm của một khách và ghi đè nhau.</summary>
    public byte[] RowVersion { get; set; } = [];
}
