using Friendship.Domain.Entities;

namespace Friendship.Domain;

public static class Tien
{
    /// <summary>1234567 → "1.234.567đ" (không phụ thuộc locale máy).</summary>
    public static string Dong(decimal v) =>
        decimal.Round(v, 0, MidpointRounding.AwayFromZero).ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.') + "đ";
}

/// <summary>BR01: cọc từ 30% đến 50% tổng thanh toán; làm tròn vào trong để mọi số đều nằm trong khoảng.</summary>
public static class CocRule
{
    public const decimal TiLeToiThieu = 0.30m, TiLeToiDa = 0.50m;

    public static (decimal Min, decimal Max) Khoang(decimal tong) =>
        (Math.Ceiling(tong * TiLeToiThieu), Math.Floor(tong * TiLeToiDa));

    public static void Kiem(decimal tong, decimal coc)
    {
        var (min, max) = Khoang(tong);
        if (coc < min || coc > max)
            throw new NghiepVuException($"Tiền cọc phải từ {Tien.Dong(min)} đến {Tien.Dong(max)} (30%–50% tổng thanh toán {Tien.Dong(tong)}).");
    }
}

/// <summary>BR13: 10.000đ thanh toán = 1 điểm; 1 điểm = 1.000đ khi trừ.</summary>
public static class DiemRule
{
    public const decimal TienMotDiemCong = 10_000m, TienMotDiemTru = 1_000m;
    public static int DiemCong(decimal tongThanhToan) => (int)Math.Floor(tongThanhToan / TienMotDiemCong);
    public static decimal TienGiam(int diem) => diem * TienMotDiemTru;
}

public record DongGia(decimal ThanhTien, bool XaHang);
public record KetQuaGia(decimal TongTienHang, decimal TienGiamGia, decimal TienDiem, decimal TongThanhToan);

/// <summary>UC06: tối đa một voucher cộng với điểm (BR21); voucher và đơn tối thiểu chỉ tính trên dòng không xả hàng (BR22).</summary>
public static class GiamGiaRule
{
    public static KetQuaGia Tinh(IReadOnlyCollection<DongGia> dong, KhuyenMai? km, DateOnly homNay, int diemSuDung, int diemHienCo)
    {
        if (dong.Count == 0) throw new NghiepVuException("Đơn phải có ít nhất một dòng hàng.");
        var tong = dong.Sum(d => d.ThanhTien);
        decimal giam = 0;
        if (km is not null)
        {
            var tinhTrang = km.TinhTrang(homNay);
            if (tinhTrang != TinhTrangKhuyenMai.DANG_AP_DUNG)
                throw new NghiepVuException(tinhTrang switch
                {
                    TinhTrangKhuyenMai.NGUNG => "Voucher đã ngừng áp dụng.",
                    TinhTrangKhuyenMai.SAP_TOI => "Voucher chưa đến ngày áp dụng.",
                    _ => "Voucher đã hết hạn."
                });
            var coGia = dong.Where(d => !d.XaHang).Sum(d => d.ThanhTien);
            if (coGia == 0) throw new NghiepVuException("Voucher không áp dụng cho bánh đang giảm giá xả hàng.");
            if (coGia < km.GiaTriDonToiThieu)
                throw new NghiepVuException($"Đơn chưa đạt giá trị tối thiểu {Tien.Dong(km.GiaTriDonToiThieu)} (không tính bánh xả hàng).");
            giam = km.LoaiGiam == LoaiGiam.PHAN_TRAM
                ? Math.Round(coGia * km.GiaTriGiam / 100m, 0, MidpointRounding.AwayFromZero)
                : Math.Min(km.GiaTriGiam, coGia);
        }
        if (diemSuDung < 0) throw new NghiepVuException("Số điểm sử dụng không được âm.");
        if (diemSuDung > diemHienCo) throw new NghiepVuException($"Khách chỉ có {diemHienCo} điểm.");
        var tienDiem = DiemRule.TienGiam(diemSuDung);
        if (tienDiem > tong - giam) throw new NghiepVuException("Số điểm quy đổi vượt quá số tiền cần thanh toán.");
        return new KetQuaGia(tong, giam, tienDiem, tong - giam - tienDiem);
    }

    /// <summary>BR17/BR24: giá bán của lô đang xả hàng; tyLe là phân số 0–1.</summary>
    public static decimal GiaXaHang(decimal giaGoc, decimal tyLe) =>
        Math.Round(giaGoc * (1 - tyLe), 0, MidpointRounding.AwayFromZero);
}

/// <summary>Sơ đồ trạng thái đơn đặt trước (tài liệu mục 5.4, thêm hủy khi bếp đang làm và khi chờ giao theo UC07).</summary>
public static class TrangThaiDonRule
{
    static readonly Dictionary<TrangThaiDon, TrangThaiDon[]> Cho = new()
    {
        [TrangThaiDon.CHO_COC] = [TrangThaiDon.DA_COC, TrangThaiDon.DA_HUY],
        [TrangThaiDon.DA_COC] = [TrangThaiDon.DANG_LAM, TrangThaiDon.CHO_GIAO, TrangThaiDon.DA_HUY],
        [TrangThaiDon.DANG_LAM] = [TrangThaiDon.CHO_GIAO, TrangThaiDon.DA_HUY],
        [TrangThaiDon.CHO_GIAO] = [TrangThaiDon.DA_GIAO, TrangThaiDon.BUNG_COC, TrangThaiDon.DA_HUY],
        [TrangThaiDon.DA_THANH_TOAN] = [TrangThaiDon.DA_TRA],
    };

    public static bool ChoPhep(TrangThaiDon tu, TrangThaiDon den) => Cho.TryGetValue(tu, out var ds) && ds.Contains(den);

    public static void Chuyen(DonHang don, TrangThaiDon moi)
    {
        if (!ChoPhep(don.TrangThai, moi))
            throw new NghiepVuException($"Không thể chuyển đơn từ '{Ten(don.TrangThai)}' sang '{Ten(moi)}'.");
        don.TrangThai = moi;
    }

    public static string Ten(TrangThaiDon t) => t switch
    {
        TrangThaiDon.CHO_COC => "Chờ cọc",
        TrangThaiDon.DA_COC => "Đã cọc",
        TrangThaiDon.DANG_LAM => "Đang làm",
        TrangThaiDon.CHO_GIAO => "Chờ giao",
        TrangThaiDon.DA_GIAO => "Đã giao",
        TrangThaiDon.DA_THANH_TOAN => "Đã thanh toán",
        TrangThaiDon.DA_HUY => "Đã hủy",
        TrangThaiDon.BUNG_COC => "Bùng cọc",
        TrangThaiDon.DA_TRA => "Đã trả",
        _ => t.ToString()
    };
}

/// <summary>BR11, BR23.</summary>
public static class HuyDonRule
{
    public static readonly TimeSpan Han = TimeSpan.FromHours(24);
    public static bool HoanCoc(DateTime bayGio, DateTime ngayGioLay) => ngayGioLay - bayGio >= Han;
    public static bool BungCoc(DateTime bayGio, DateTime ngayGioLay) => bayGio - ngayGioLay > Han;
}
