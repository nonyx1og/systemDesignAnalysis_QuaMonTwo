namespace Friendship.Domain.Entities;

/// <summary>
/// Ca làm việc nhân viên (bếp, kho, bán hàng,...), có thể tái sử dụng cho nhiều bộ phận.
/// </summary>
public class CaNhanVien
{
    public int Id { get; set; }
    public string Ma => $"CNV-{Id:000000}";
    public int NhanVienId { get; set; }
    public NhanVien NhanVien { get; set; } = null!;
    public LoaiCa LoaiCa { get; set; } = LoaiCa.BEP_BANH;
    public BuoiCa BuoiCa { get; set; } = BuoiCa.CA_SANG;
    public DateTime GioMoCa { get; set; }
    public DateTime? GioDongCa { get; set; }
    public TrangThaiCa TrangThai { get; set; } = TrangThaiCa.DANG_MO;
    public string? GhiChu { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public List<TieuHaoDinhLuongCa> TieuHaos { get; set; } = [];
    public List<PhieuBaoLoiMeBanh> PhieuBaoLois { get; set; } = [];
    public List<PhieuDinhLuongCa> PhieuDinhLuongs { get; set; } = [];
}

/// <summary>
/// Định lượng tiêu hao thực tế/quy đổi trong ca của nhân viên (bếp).
/// Nguồn có thể từ sản phẩm có sẵn BOM, phiếu bổ sung (cho bánh chưa có BOM), hoặc mẻ lỗi được quản lý duyệt.
/// </summary>
public class TieuHaoDinhLuongCa
{
    public int Id { get; set; }
    public int CaNhanVienId { get; set; }
    public CaNhanVien CaNhanVien { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal SoLuong { get; set; }
    public NguonTieuHao Nguon { get; set; } = NguonTieuHao.SAN_PHAM_CO_BOM;
    public int? DonHangId { get; set; }
    public DonHang? DonHang { get; set; }
    public int? SanPhamId { get; set; }
    public SanPham? SanPham { get; set; }
    public int? PhieuBaoLoiId { get; set; }
    public PhieuBaoLoiMeBanh? PhieuBaoLoi { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? GhiChu { get; set; }
}

/// <summary>
/// Phiếu báo mẻ bánh bị lỗi / hư hỏng trong ca làm việc.
/// </summary>
public class PhieuBaoLoiMeBanh
{
    public int Id { get; set; }
    public string Ma => $"PL-{Id:000000}";
    public int CaNhanVienId { get; set; }
    public CaNhanVien CaNhanVien { get; set; } = null!;
    public int NhanVienLapId { get; set; }
    public NhanVien NhanVienLap { get; set; } = null!;
    public int? NguoiDuyetId { get; set; }
    public NhanVien? NguoiDuyet { get; set; }
    public int? SanPhamId { get; set; }
    public SanPham? SanPham { get; set; }
    public int SoLuongHong { get; set; }
    public string LyDoLoi { get; set; } = "";
    public string? GhiChuQuanLy { get; set; }
    public TrangThaiPhieu TrangThai { get; set; } = TrangThaiPhieu.CHO_DUYET;
    public DateTime NgayLap { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public List<ChiTietBaoLoi> ChiTiets { get; set; } = [];
}

public class ChiTietBaoLoi
{
    public int Id { get; set; }
    public int PhieuBaoLoiId { get; set; }
    public PhieuBaoLoiMeBanh PhieuBaoLoi { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal SoLuong { get; set; }
}

/// <summary>
/// Phiếu định lượng chốt ca xuất gửi sang kho.
/// </summary>
public class PhieuDinhLuongCa
{
    public int Id { get; set; }
    public string Ma => $"PDL-{Id:000000}";
    public int CaNhanVienId { get; set; }
    public CaNhanVien CaNhanVien { get; set; } = null!;
    public int NguoiLapId { get; set; }
    public NhanVien NguoiLap { get; set; } = null!;
    public DateTime ThoiGianXuat { get; set; }
    public TrangThaiPhieuDinhLuong TrangThai { get; set; } = TrangThaiPhieuDinhLuong.CHO_DOI_CHIEU;
    public string? GhiChu { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public List<ChiTietDinhLuongCa> ChiTiets { get; set; } = [];
    public BienBanDoiChieuKho? BienBanDoiChieu { get; set; }
}

public class ChiTietDinhLuongCa
{
    public int Id { get; set; }
    public int PhieuDinhLuongCaId { get; set; }
    public PhieuDinhLuongCa PhieuDinhLuongCa { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal SoLuongDinhLuong { get; set; }
}

/// <summary>
/// Phiếu xuất kho nguyên liệu phát sinh trong ca.
/// </summary>
public class PhieuXuatKho
{
    public int Id { get; set; }
    public string Ma => $"PXK-{Id:000000}";
    public int NhanVienXuatId { get; set; }
    public NhanVien NhanVienXuat { get; set; } = null!;
    public DateTime NgayXuat { get; set; }
    public string? GhiChu { get; set; }
    public List<ChiTietXuatKho> ChiTiets { get; set; } = [];
}

public class ChiTietXuatKho
{
    public int Id { get; set; }
    public int PhieuXuatKhoId { get; set; }
    public PhieuXuatKho PhieuXuatKho { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal SoLuong { get; set; }
}

/// <summary>
/// Biên bản đối chiếu giữa kho và phiếu định lượng ca bếp.
/// </summary>
public class BienBanDoiChieuKho
{
    public int Id { get; set; }
    public string Ma => $"DC-{Id:000000}";
    public int PhieuDinhLuongCaId { get; set; }
    public PhieuDinhLuongCa PhieuDinhLuongCa { get; set; } = null!;
    public int NguoiDoiChieuId { get; set; }
    public NhanVien NguoiDoiChieu { get; set; } = null!;
    public DateTime ThoiGianDoiChieu { get; set; }
    public TrangThaiDoiChieu TrangThai { get; set; } = TrangThaiDoiChieu.HOP_LE;
    public string? GhiChuGiaiTrinh { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public List<ChiTietDoiChieuKho> ChiTiets { get; set; } = [];
}

public class ChiTietDoiChieuKho
{
    public int Id { get; set; }
    public int BienBanDoiChieuKhoId { get; set; }
    public BienBanDoiChieuKho BienBanDoiChieuKho { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal SoLuongXuatKho { get; set; }
    public decimal SoLuongDinhLuong { get; set; }
    public decimal ChenhLech => SoLuongXuatKho - SoLuongDinhLuong;
}
