using System.Globalization;
using Friendship.Domain;
using Friendship.Domain.Entities;

namespace Friendship.Web;

/// <summary>Định dạng hiển thị theo chuẩn Việt Nam (tách khỏi culture của máy chủ, vốn dùng Invariant để bind số).</summary>
public static class Fmt
{
    static readonly CultureInfo Vi = new("vi-VN");

    public static string Tien(decimal v) => v.ToString("N0", Vi) + " ₫";
    public static string So(decimal v) => v.ToString("0.###", Vi);
    public static string Ngay(DateOnly d) => d.ToString("dd/MM/yyyy", Vi);
    public static string NgayGio(DateTime d) => d.ToString("dd/MM/yyyy HH:mm", Vi);

    public static (string Nhan, string Css) Date(TrangThaiDate t) => t switch
    {
        TrangThaiDate.HET_HAN => ("Hết hạn", "danger"),
        TrangThaiDate.CAN_DATE => ("Cận date", "warn"),
        _ => ("Còn hạn", "ok")
    };

    public static (string Nhan, string Css) Phieu(TrangThaiPhieu t) => t switch
    {
        TrangThaiPhieu.NHAP => ("Nháp", ""),
        TrangThaiPhieu.CHO_DUYET => ("Chờ duyệt", "warn"),
        TrangThaiPhieu.DA_DUYET => ("Đã duyệt", "ok"),
        _ => ("Từ chối", "danger")
    };

    public static (string Nhan, string Css) KhuyenMai(TinhTrangKhuyenMai t) => t switch
    {
        TinhTrangKhuyenMai.DANG_AP_DUNG => ("Đang áp dụng", "ok"),
        TinhTrangKhuyenMai.SAP_TOI => ("Sắp tới", "warn"),
        TinhTrangKhuyenMai.HET_HAN => ("Hết hạn", "danger"),
        _ => ("Đã ngừng", "")
    };


    public static (string Nhan, string Css) Don(TrangThaiDon t) => t switch
    {
        TrangThaiDon.CHO_COC => ("Chờ cọc", "warn"),
        TrangThaiDon.DA_COC => ("Đã cọc", ""),
        TrangThaiDon.DANG_LAM => ("Đang làm", "warn"),
        TrangThaiDon.CHO_GIAO => ("Chờ giao", "ok"),
        TrangThaiDon.DA_GIAO => ("Đã giao", "ok"),
        TrangThaiDon.DA_THANH_TOAN => ("Đã thanh toán", "ok"),
        TrangThaiDon.DA_HUY => ("Đã hủy", "danger"),
        TrangThaiDon.DA_TRA => ("Đã trả", "danger"),
        _ => ("Bùng cọc", "danger")
    };

    public static string PhuongThuc(PhuongThucTT p) => p == PhuongThucTT.TIEN_MAT ? "Tiền mặt" : "Chuyển khoản";

    public static string LoaiTT(LoaiThanhToan l) => l switch
    {
        LoaiThanhToan.COC => "Cọc",
        LoaiThanhToan.TAT_TOAN => "Tất toán",
        LoaiThanhToan.THANH_TOAN_DU => "Thanh toán đủ",
        LoaiThanhToan.HOAN_COC => "Hoàn cọc",
        LoaiThanhToan.HOAN_TIEN => "Hoàn tiền",
        _ => l.ToString()
    };

    /// <summary>Khoảng cách tới giờ hẹn: dương = còn, âm = đã quá giờ.</summary>
    public static string KhoangGio(TimeSpan t)
    {
        var phut = (int)Math.Floor(Math.Abs(t.TotalMinutes));
        var gio = phut / 60;
        var s = gio > 0 ? $"{gio} giờ {phut % 60} phút" : $"{phut} phút";
        return t < TimeSpan.Zero ? $"Quá giờ {s}" : $"Còn {s}";
    }

    public static string LyDoHuy(LyDoHuy ly) => ly switch
    {
        Domain.LyDoHuy.QUA_HAN => "Quá hạn",
        Domain.LyDoHuy.LOI_SAN_XUAT => "Lỗi sản xuất",
        Domain.LyDoHuy.KHACH_TRA_HANG => "Khách trả hàng",
        _ => "Khách bùng cọc"
    };

    /// <summary>Tên đối tượng của một dòng phiếu hủy (UC17 bước 2).</summary>
    public static string DoiTuongHuy(ChiTietXuatHuy c) =>
        c.LoNguyenLieu is { } nl ? $"{nl.NguyenLieu.TenGoi} (lô #{nl.Id})"
        : c.LoSanPham is { } sp ? $"{sp.SanPham.TenBanh} (lô bánh #{sp.Id})"
        : $"Đơn {c.DonHang!.Ma} ({(c.LyDo == Domain.LyDoHuy.KHACH_TRA_HANG ? "trả hàng" : "bùng cọc")})";
}
