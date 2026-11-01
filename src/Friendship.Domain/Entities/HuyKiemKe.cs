namespace Friendship.Domain.Entities;

public class PhieuXuatHuy
{
    public int Id { get; set; }
    public string Ma => $"PH-{Id:000000}";
    public int NguoiLapId { get; set; }
    public NhanVien NguoiLap { get; set; } = null!;
    public int? NguoiDuyetId { get; set; }
    public NhanVien? NguoiDuyet { get; set; }
    public DateTime NgayLap { get; set; }
    public TrangThaiPhieu TrangThai { get; set; } = TrangThaiPhieu.NHAP;
    public DateTime? NgayDuyet { get; set; }
    public string? LyDoTuChoi { get; set; }
    public List<ChiTietXuatHuy> ChiTiets { get; set; } = [];
    public byte[] RowVersion { get; set; } = [];
}

/// <summary>Mỗi dòng tham chiếu đúng một trong: LoNguyenLieu, LoSanPham, DonHang (CHECK xor ở DB).</summary>
public class ChiTietXuatHuy
{
    public int Id { get; set; }
    public int PhieuXuatHuyId { get; set; }
    public PhieuXuatHuy PhieuXuatHuy { get; set; } = null!;
    public int? LoNguyenLieuId { get; set; }
    public LoNguyenLieu? LoNguyenLieu { get; set; }
    public int? LoSanPhamId { get; set; }
    public LoSanPham? LoSanPham { get; set; }
    public int? DonHangId { get; set; }
    public DonHang? DonHang { get; set; }
    public decimal SoLuongHuy { get; set; }
    public decimal DonGiaVon { get; set; }
    public LyDoHuy LyDo { get; set; }
}

public class PhieuKiemKe
{
    public int Id { get; set; }
    public string Ma => $"KK-{Id:000000}";
    public int NguoiThucHienId { get; set; }
    public NhanVien NguoiThucHien { get; set; } = null!;
    public int? NguoiDuyetId { get; set; }
    public NhanVien? NguoiDuyet { get; set; }
    public DateTime NgayKiemKe { get; set; }
    public TrangThaiPhieu TrangThai { get; set; } = TrangThaiPhieu.NHAP;
    public DateTime? NgayDuyet { get; set; }
    public string? LyDoTuChoi { get; set; }
    public List<ChiTietKiemKe> ChiTiets { get; set; } = [];
    public byte[] RowVersion { get; set; } = [];
}

public class ChiTietKiemKe
{
    public int Id { get; set; }
    public int PhieuKiemKeId { get; set; }
    public PhieuKiemKe PhieuKiemKe { get; set; } = null!;
    public int LoNguyenLieuId { get; set; }
    public LoNguyenLieu LoNguyenLieu { get; set; } = null!;
    public decimal SoLuongHeThong { get; set; }
    public decimal SoLuongThucTe { get; set; }
    /// <summary>Bắt buộc khi SoLuongThucTe ≠ SoLuongHeThong (BR26).</summary>
    public string? LyDo { get; set; }
}
