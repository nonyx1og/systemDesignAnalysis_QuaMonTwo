namespace Friendship.Domain.Entities;

public class SanPham
{
    public int Id { get; set; }
    public string TenBanh { get; set; } = "";
    public string KichThuoc { get; set; } = "";
    public decimal DonGia { get; set; }
    public int HanSuDungGio { get; set; }
    public bool DangKinhDoanh { get; set; } = true;
    public List<DinhLuong> DinhLuongs { get; set; } = [];
    public List<LoSanPham> Los { get; set; } = [];
}

public class NguyenLieu
{
    public int Id { get; set; }
    public string TenGoi { get; set; } = "";
    public string DonViTinh { get; set; } = "";
    public decimal MucCanhBaoTon { get; set; }
    public List<DinhLuong> DinhLuongs { get; set; } = [];
    public List<LoNguyenLieu> Los { get; set; } = [];
}

/// <summary>Association class SanPham–NguyenLieu (BOM).</summary>
public class DinhLuong
{
    public int SanPhamId { get; set; }
    public SanPham SanPham { get; set; } = null!;
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public decimal DinhMuc { get; set; }
}

public class LoNguyenLieu
{
    public int Id { get; set; }
    public int NguyenLieuId { get; set; }
    public NguyenLieu NguyenLieu { get; set; } = null!;
    public DateOnly NgayNhap { get; set; }
    public DateOnly HanSuDung { get; set; }
    public decimal SoLuongNhap { get; set; }
    public decimal SoLuongCon { get; set; }
    public decimal DonGiaNhap { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public TrangThaiDate TrangThaiDate(DateOnly homNay) => HanSuDungRule.NguyenLieu(HanSuDung, homNay);
}

public class LoSanPham
{
    public int Id { get; set; }
    public int SanPhamId { get; set; }
    public SanPham SanPham { get; set; } = null!;
    public DateTime NgaySanXuat { get; set; }
    public DateTime HanSuDung { get; set; }
    public int SoLuongSanXuat { get; set; }
    public int SoLuongTon { get; set; }
    /// <summary>Phân số 0–1 (0,3 = giảm 30%).</summary>
    public decimal TyLeGiamXaHang { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public TrangThaiDate TrangThaiDate(DateTime bayGio) => HanSuDungRule.Banh(HanSuDung, bayGio);
}
