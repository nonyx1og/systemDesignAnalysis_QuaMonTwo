using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;

namespace Friendship.Web.Api;

public record NhanVienDto(int Id, string HoTen, string? GioiTinh, DateOnly? NgaySinh, string SoDienThoai, string? Email,
    string TenDangNhap, VaiTro VaiTro, bool DangHoatDong);
public record DinhLuongDto(int NguyenLieuId, string? TenNguyenLieu, decimal DinhMuc);
public record SanPhamDto(int Id, string TenBanh, string KichThuoc, decimal DonGia, int HanSuDungGio, bool DangKinhDoanh, List<DinhLuongDto> DinhLuongs);
public record NguyenLieuDto(int Id, string TenGoi, string DonViTinh, decimal MucCanhBaoTon, decimal? TongTon);
public record KhuyenMaiDto(int Id, string Ma, string Ten, LoaiGiam Loai, decimal GiaTri, decimal DonToiThieu,
    DateOnly NgayBatDau, DateOnly NgayKetThuc, bool DangApDung, TinhTrangKhuyenMai TinhTrang);
public record KhachHangDto(int Id, string Ten, string SoDienThoai, int DiemTichLuy);
public record ChiTietDonDto(int SanPhamId, string? TenBanh, int SoLuong, decimal DonGia, decimal TienGiam, decimal ThanhTien, string? LoiChuc, int? LoSanPhamId);
public record ThanhToanDto(int Id, LoaiThanhToan Loai, PhuongThucTT PhuongThuc, decimal SoTien, DateTime ThoiGian, int CaThuNganId);
public record DonHangDto(int Id, string Ma, LoaiDon LoaiDon, TrangThaiDon TrangThai, DateTime NgayLap, DateTime? NgayGioLay,
    KhachHangDto? KhachHang, string? MaVoucher, int DiemSuDung, decimal TongTienHang, decimal TienGiamGia, decimal TongThanhToan,
    decimal DaThanhToan, decimal ConLai, List<ChiTietDonDto> ChiTiets, List<ThanhToanDto> ThanhToans, string? LyDoTra = null);
public record DonTomTatDto(int Id, string Ma, LoaiDon LoaiDon, TrangThaiDon TrangThai, DateTime NgayLap, DateTime? NgayGioLay,
    string? TenKhach, string? SoDienThoai, decimal TongThanhToan);
public record DonBepDto(int Id, string Ma, TrangThaiDon TrangThai, DateTime? NgayGioLay, int TongBanh, bool KhanCap, List<ChiTietDonDto> ChiTiets);
public record CaDto(int Id, string Ma, int NhanVienId, TrangThaiCa TrangThai, DateTime GioMoCa, DateTime? GioDongCa, decimal TienDauCa,
    decimal TienMatHeThong, decimal ChuyenKhoanHeThong, decimal? TienMatThucTe, decimal? ChuyenKhoanThucTe,
    decimal? ChenhLechTienMat, decimal? ChenhLechChuyenKhoan, string? GhiChuChenhLech);
public record TongHopCaDto(CaDto Ca, decimal ThuTienMat, decimal HoanTienMat, decimal TienMatHeThong, decimal ChuyenKhoanHeThong, int SoGiaoDich);
public record LoNguyenLieuDto(int Id, int NguyenLieuId, string? TenNguyenLieu, DateOnly NgayNhap, DateOnly HanSuDung,
    decimal SoLuongNhap, decimal SoLuongCon, decimal DonGiaNhap);
public record LoSanPhamDto(int Id, int SanPhamId, string? TenBanh, DateTime NgaySanXuat, DateTime HanSuDung,
    int SoLuongSanXuat, int SoLuongTon, decimal TyLeGiamXaHang);
public record LoXaHangDto(LoSanPhamDto Lo, double PhutConLai, decimal GiaGoc, decimal GiaSauGiam);
public record ChiTietHuyDto(int Id, LoaiDoiTuongHuy Loai, int DoiTuongId, string? DoiTuong, decimal SoLuongHuy, decimal DonGiaVon, LyDoHuy LyDo);
public record PhieuHuyDto(int Id, string Ma, TrangThaiPhieu TrangThai, int NguoiLapId, DateTime NgayLap, int? NguoiDuyetId,
    DateTime? NgayDuyet, string? LyDoTuChoi, decimal TongTonThat, List<ChiTietHuyDto> ChiTiets);
public record LuaChonHuyDto(List<LoNguyenLieuDto> LoNguyenLieu, List<LoSanPhamDto> LoBanh, List<DonTomTatDto> DonBungCoc);
public record ChiTietKiemKeDto(int Id, int LoNguyenLieuId, string? TenNguyenLieu, decimal SoLuongHeThong, decimal SoLuongThucTe, string? LyDo);
public record PhieuKiemKeDto(int Id, string Ma, TrangThaiPhieu TrangThai, int NguoiThucHienId, DateTime NgayKiemKe, int? NguoiDuyetId,
    DateTime? NgayDuyet, string? LyDoTuChoi, List<ChiTietKiemKeDto> ChiTiets);
public record LoLayDto(int LoId, DateOnly HanSuDung, decimal SoLuong);
public record DongKeHoachDto(int NguyenLieuId, string TenNguyenLieu, string DonViTinh, decimal Can, decimal Thieu, List<LoLayDto> Lay);
public record KeHoachXuatDto(bool DuHang, List<DongKeHoachDto> Dong);
public record KetQuaXuatDto(KeHoachXuatDto KeHoach, List<string> CanhBaoTon);
public record CanhBaoNguyenLieuDto(LoNguyenLieuDto Lo, TrangThaiDate TrangThai, int NgayConLai);
public record CanhBaoBanhDto(LoSanPhamDto Lo, TrangThaiDate TrangThai, double PhutConLai);
public record DongDeXuatNhapDto(int NguyenLieuId, string TenNguyenLieu, string DonViTinh, decimal TonHienTai, decimal MucCanhBaoTon, decimal SoLuongDeXuat, decimal? DonGiaNhapGanNhat, decimal? ThanhTienUocTinh)
{
    public string TenGoi => TenNguyenLieu;
}
public record DeXuatNhapDto(decimal TongTienUocTinh, List<DongDeXuatNhapDto> Dong);

/// <summary>Đổi entity sang DTO. Navigation chưa nạp thì tên để null, không ném lỗi.</summary>
public static class Map
{
    public static NhanVienDto ToDto(this NhanVien x) =>
        new(x.Id, x.HoTen, x.GioiTinh, x.NgaySinh, x.SoDienThoai, x.Email, x.TenDangNhap, x.VaiTro, x.DangHoatDong);

    public static SanPhamDto ToDto(this SanPham x) => new(x.Id, x.TenBanh, x.KichThuoc, x.DonGia, x.HanSuDungGio, x.DangKinhDoanh,
        x.DinhLuongs.Select(d => new DinhLuongDto(d.NguyenLieuId, d.NguyenLieu?.TenGoi, d.DinhMuc)).ToList());

    public static NguyenLieuDto ToDto(this NguyenLieu x, decimal? tongTon = null) => new(x.Id, x.TenGoi, x.DonViTinh, x.MucCanhBaoTon, tongTon);

    public static KhuyenMaiDto ToDto(this KhuyenMai x, DateOnly homNay) => new(x.Id, x.MaKhuyenMai, x.TenChuongTrinh, x.LoaiGiam,
        x.GiaTriGiam, x.GiaTriDonToiThieu, x.NgayBatDau, x.NgayKetThuc, x.DangApDung, x.TinhTrang(homNay));

    public static KhachHangDto ToDto(this KhachHang x) => new(x.Id, x.TenKhachHang, x.SoDienThoai, x.DiemTichLuy);

    public static ChiTietDonDto ToDto(this ChiTietDonHang c) =>
        new(c.SanPhamId, c.SanPham?.TenBanh, c.SoLuong, c.DonGia, c.TienGiam, c.ThanhTien, c.LoiChuc, c.LoSanPhamId);

    /// <summary>Cần nạp ChiTiets, ThanhToans, KhachHang, KhuyenMai (dùng <c>DonDatService.LayAsync</c>).</summary>
    public static DonHangDto ToDto(this DonHang d) => new(d.Id, d.Ma, d.LoaiDon, d.TrangThai, d.NgayLap, d.NgayGioLay,
        d.KhachHang?.ToDto(), d.KhuyenMai?.MaKhuyenMai, d.DiemSuDung, d.TongTienHang, d.TienGiamGia, d.TongThanhToan,
        d.DaThanhToan, d.ConLai, d.ChiTiets.Select(c => c.ToDto()).ToList(),
        d.ThanhToans.OrderBy(t => t.ThoiGian).ThenBy(t => t.Id)
            .Select(t => new ThanhToanDto(t.Id, t.Loai, t.PhuongThuc, t.SoTien, t.ThoiGian, t.CaThuNganId)).ToList(),
        d.LyDoTra);

    public static DonTomTatDto ToTomTat(this DonHang d) => new(d.Id, d.Ma, d.LoaiDon, d.TrangThai, d.NgayLap, d.NgayGioLay,
        d.KhachHang?.TenKhachHang, d.KhachHang?.SoDienThoai, d.TongThanhToan);

    public static CaDto ToDto(this CaThuNgan c) => new(c.Id, c.Ma, c.NhanVienId, c.TrangThai, c.GioMoCa, c.GioDongCa, c.TienDauCa,
        c.TienMatHeThong, c.ChuyenKhoanHeThong, c.TienMatThucTe, c.ChuyenKhoanThucTe, c.ChenhLechTienMat, c.ChenhLechChuyenKhoan, c.GhiChuChenhLech);

    public static TongHopCaDto ToDto(this TongHopCa t) =>
        new(t.Ca.ToDto(), t.ThuTienMat, t.HoanTienMat, t.TienMatHeThong, t.ChuyenKhoanHeThong, t.SoGiaoDich);

    public static LoNguyenLieuDto ToDto(this LoNguyenLieu l) =>
        new(l.Id, l.NguyenLieuId, l.NguyenLieu?.TenGoi, l.NgayNhap, l.HanSuDung, l.SoLuongNhap, l.SoLuongCon, l.DonGiaNhap);

    public static LoSanPhamDto ToDto(this LoSanPham l) =>
        new(l.Id, l.SanPhamId, l.SanPham?.TenBanh, l.NgaySanXuat, l.HanSuDung, l.SoLuongSanXuat, l.SoLuongTon, l.TyLeGiamXaHang);

    public static ChiTietHuyDto ToDto(this ChiTietXuatHuy c) =>
        c.LoNguyenLieuId is { } nl ? new(c.Id, LoaiDoiTuongHuy.LO_NGUYEN_LIEU, nl, c.LoNguyenLieu?.NguyenLieu?.TenGoi, c.SoLuongHuy, c.DonGiaVon, c.LyDo)
        : c.LoSanPhamId is { } b ? new(c.Id, LoaiDoiTuongHuy.LO_BANH, b, c.LoSanPham?.SanPham?.TenBanh, c.SoLuongHuy, c.DonGiaVon, c.LyDo)
        : new(c.Id, LoaiDoiTuongHuy.DON_BUNG_COC, c.DonHangId ?? 0, c.DonHang?.Ma, c.SoLuongHuy, c.DonGiaVon, c.LyDo);

    public static PhieuHuyDto ToDto(this PhieuXuatHuy p) => new(p.Id, p.Ma, p.TrangThai, p.NguoiLapId, p.NgayLap, p.NguoiDuyetId,
        p.NgayDuyet, p.LyDoTuChoi, HuyHangService.TongTonThat(p), p.ChiTiets.Select(c => c.ToDto()).ToList());

    public static PhieuKiemKeDto ToDto(this PhieuKiemKe p) => new(p.Id, p.Ma, p.TrangThai, p.NguoiThucHienId, p.NgayKiemKe,
        p.NguoiDuyetId, p.NgayDuyet, p.LyDoTuChoi,
        p.ChiTiets.Select(c => new ChiTietKiemKeDto(c.Id, c.LoNguyenLieuId, c.LoNguyenLieu?.NguyenLieu?.TenGoi,
            c.SoLuongHeThong, c.SoLuongThucTe, c.LyDo)).ToList());

    public static KeHoachXuatDto ToDto(this KeHoachXuat k) => new(k.DuHang, k.Dong.Select(d => new DongKeHoachDto(
        d.NguyenLieu.Id, d.NguyenLieu.TenGoi, d.NguyenLieu.DonViTinh, d.Can, d.Thieu,
        d.Lay.Select(l => new LoLayDto(l.Lo.Id, l.Lo.HanSuDung, l.SoLuong)).ToList())).ToList());

    public static DongDeXuatNhapDto ToDto(this DongDeXuatNhap d) =>
        new(d.NguyenLieuId, d.TenGoi, d.DonViTinh, d.TonHienTai, d.MucCanhBaoTon, d.SoLuongDeXuat, d.DonGiaNhapGanNhat, d.ThanhTienUocTinh);

    public static DeXuatNhapDto ToDto(this BangDeXuatNhap b) =>
        new(b.TongTienUocTinh, b.Dong.Select(d => d.ToDto()).ToList());
}
