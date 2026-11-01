namespace Friendship.Domain.Entities;

public class GopY
{
    public int Id { get; set; }
    public int? KhachHangId { get; set; }
    public KhachHang? KhachHang { get; set; }
    public string HoTen { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public string? Email { get; set; }
    public int DiemDanhGia { get; set; } = 5; // 1 đến 5 sao
    public string TieuDe { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public DateTime NgayGui { get; set; } = DateTime.Now;
    public string TrangThai { get; set; } = "Moi"; // Moi, DaXem
}
