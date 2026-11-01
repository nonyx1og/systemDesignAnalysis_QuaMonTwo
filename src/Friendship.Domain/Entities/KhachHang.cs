namespace Friendship.Domain.Entities;

public class KhachHang
{
    public int Id { get; set; }
    public string TenKhachHang { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public int DiemTichLuy { get; set; }
    /// <summary>Chống hai thu ngân cùng dùng/cộng điểm của một khách và ghi đè nhau.</summary>
    public byte[] RowVersion { get; set; } = [];
}
