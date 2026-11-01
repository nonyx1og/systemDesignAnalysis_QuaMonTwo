namespace Friendship.Domain.Entities;

public class ThanhPhanTrangTri
{
    public int Id { get; set; }
    public NhomThanhPhan Nhom { get; set; }
    public string Ten { get; set; } = "";
    public string? AnhMinhHoa { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongToiDa { get; set; }
    public int? SanPhamNenId { get; set; }
    public SanPham? SanPhamNen { get; set; }
    public bool DangKinhDoanh { get; set; } = true;
    public List<DinhLuongThanhPhan> DinhLuongs { get; set; } = [];
}

public class DinhLuongThanhPhan
{
    public int ThanhPhanId { get; set; }
    public ThanhPhanTrangTri ThanhPhan { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal DinhMuc { get; set; }
}

public class ThietKeBanh
{
    public int Id { get; set; }
    public string Ma => $"TK-{Id:000000}";
    public string MaTraCuu { get; set; } = "";
    public int KhachHangId { get; set; }
    public KhachHang KhachHang { get; set; } = null!;
    public int SanPhamNenId { get; set; }
    public SanPham SanPhamNen { get; set; } = null!;
    public string DuLieuJson { get; set; } = "";
    public string AnhThietKe { get; set; } = "";
    public string? AnhInAn { get; set; }
    public string? ChuTrenBanh { get; set; }
    public string? LoiNhan { get; set; }
    public DateTime NgayGioLay { get; set; }
    public TrangThaiThietKe TrangThai { get; set; } = TrangThaiThietKe.NHAP;
    public decimal GiaTamTinh { get; set; }
    public decimal PhuPhi { get; set; }
    public string? LyDoPhuPhi { get; set; }
    public decimal GiaCuoi { get; set; }
    public decimal GiaVonDuKien { get; set; }
    public string? GhiChuBep { get; set; }
    public DateTime? NgayGui { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public int? NguoiDuyetId { get; set; }
    public NhanVien? NguoiDuyet { get; set; }
    public DateTime? HanBaoGia { get; set; }
    public int? DonHangId { get; set; }
    public DonHang? DonHang { get; set; }
    public string? AnhThanhPham { get; set; }
    public List<ThietKeThanhPhan> ThanhPhans { get; set; } = [];
    public byte[] RowVersion { get; set; } = [];
}

public class ThietKeThanhPhan
{
    public int Id { get; set; }
    public int ThietKeId { get; set; }
    public ThietKeBanh ThietKe { get; set; } = null!;
    public int ThanhPhanId { get; set; }
    public ThanhPhanTrangTri ThanhPhan { get; set; } = null!;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
}
