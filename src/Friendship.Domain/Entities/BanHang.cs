namespace Friendship.Domain.Entities;

public class DonHang
{
    public int Id { get; set; }
    public string Ma => $"DH-{Id:000000}";
    public LoaiDon LoaiDon { get; set; }
    public DateTime NgayLap { get; set; }
    public DateTime? NgayGioLay { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal TienGiamGia { get; set; }
    public int DiemSuDung { get; set; }
    public decimal TongThanhToan { get; set; }
    public TrangThaiDon TrangThai { get; set; }
    public int NhanVienId { get; set; }
    public NhanVien NhanVien { get; set; } = null!;
    public int? KhachHangId { get; set; }
    public KhachHang? KhachHang { get; set; }
    public int? KhuyenMaiId { get; set; }
    public KhuyenMai? KhuyenMai { get; set; }
    public List<ChiTietDonHang> ChiTiets { get; set; } = [];
    public List<ThanhToan> ThanhToans { get; set; } = [];
    public string? LyDoTra { get; set; }
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Tổng đã thu (hoàn cọc là số âm). Cần nạp <see cref="ThanhToans"/> trước khi dùng.</summary>
    public decimal DaThanhToan => ThanhToans.Sum(t => t.SoTien);
    public decimal ConLai => TrangThai is TrangThaiDon.DA_HUY or TrangThaiDon.BUNG_COC or TrangThaiDon.DA_TRA
        ? 0 : TongThanhToan - DaThanhToan;   // đơn đã đóng không còn phải thu
}

public class ChiTietDonHang
{
    public int Id { get; set; }
    public int DonHangId { get; set; }
    public DonHang DonHang { get; set; } = null!;
    public int SanPhamId { get; set; }
    public SanPham SanPham { get; set; } = null!;
    public int? LoSanPhamId { get; set; }
    public LoSanPham? LoSanPham { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal TienGiam { get; set; }
    public decimal ThanhTien { get; set; }
    /// <summary>Lời chúc riêng cho từng bánh (Q13).</summary>
    public string? LoiChuc { get; set; }
    public int? ThietKeBanhId { get; set; }
    public ThietKeBanh? ThietKeBanh { get; set; }
}

public class KhuyenMai
{
    public int Id { get; set; }
    public string MaKhuyenMai { get; set; } = "";
    public string TenChuongTrinh { get; set; } = "";
    public LoaiGiam LoaiGiam { get; set; }
    public decimal GiaTriGiam { get; set; }
    public decimal GiaTriDonToiThieu { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public bool DangApDung { get; set; } = true;

    public TinhTrangKhuyenMai TinhTrang(DateOnly homNay) =>
        !DangApDung ? TinhTrangKhuyenMai.NGUNG
        : homNay < NgayBatDau ? TinhTrangKhuyenMai.SAP_TOI
        : homNay > NgayKetThuc ? TinhTrangKhuyenMai.HET_HAN
        : TinhTrangKhuyenMai.DANG_AP_DUNG;
}

public class ThanhToan
{
    public int Id { get; set; }
    public int DonHangId { get; set; }
    public DonHang DonHang { get; set; } = null!;
    public int CaThuNganId { get; set; }
    public CaThuNgan CaThuNgan { get; set; } = null!;
    public LoaiThanhToan Loai { get; set; }
    public PhuongThucTT PhuongThuc { get; set; }
    /// <summary>Hoàn cọc lưu số âm.</summary>
    public decimal SoTien { get; set; }
    public DateTime ThoiGian { get; set; }
}

public class CaThuNgan
{
    public int Id { get; set; }
    public string Ma => $"CA-{Id:000000}";
    public int NhanVienId { get; set; }
    public NhanVien NhanVien { get; set; } = null!;
    public DateTime GioMoCa { get; set; }
    public DateTime? GioDongCa { get; set; }
    public decimal TienDauCa { get; set; }
    public decimal TienMatHeThong { get; set; }
    public decimal ChuyenKhoanHeThong { get; set; }
    public decimal? TienMatThucTe { get; set; }
    public decimal? ChuyenKhoanThucTe { get; set; }
    public string? GhiChuChenhLech { get; set; }
    public TrangThaiCa TrangThai { get; set; } = TrangThaiCa.DANG_MO;
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Chênh lệch khi chốt ca (thực tế − hệ thống); null khi ca chưa chốt.</summary>
    public decimal? ChenhLechTienMat => TienMatThucTe is null ? null : TienMatThucTe - TienMatHeThong;
    public decimal? ChenhLechChuyenKhoan => ChuyenKhoanThucTe is null ? null : ChuyenKhoanThucTe - ChuyenKhoanHeThong;
}
