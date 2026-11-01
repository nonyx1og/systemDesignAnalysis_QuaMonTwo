using Friendship.Domain.Entities;

namespace Friendship.Domain;

public static class ThietKeRule
{
    public const int ChuToiDa = 30;
    public const int LoiNhanToiDa = 200;
    public const int GioDatTruocToiThieu = 48;
    public const int GioHanBaoGia = 24;
    public const decimal PhuPhiToiDa = 0.30m;
    public const int JsonToiDa = 1024 * 1024; // 1 MB
    public const int AnhDungLuongToiDa = 5 * 1024 * 1024; // 5 MB
    public const int AnhDataUrlDungLuongToiDa = 2 * 1024 * 1024; // 2 MB
    public const int AnhKichThuocToiDa = 4096; // 4096x4096 px
    public const int ToppingToiDa = 30;
    public const int SoNenToiDa = 10;

    static readonly Dictionary<TrangThaiThietKe, TrangThaiThietKe[]> Cho = new()
    {
        [TrangThaiThietKe.NHAP] = [TrangThaiThietKe.CHO_DUYET, TrangThaiThietKe.KHACH_HUY],
        [TrangThaiThietKe.CHO_DUYET] = [TrangThaiThietKe.DA_BAO_GIA, TrangThaiThietKe.CAN_SUA, TrangThaiThietKe.TU_CHOI, TrangThaiThietKe.KHACH_HUY],
        [TrangThaiThietKe.CAN_SUA] = [TrangThaiThietKe.CHO_DUYET, TrangThaiThietKe.KHACH_HUY],
        [TrangThaiThietKe.DA_BAO_GIA] = [TrangThaiThietKe.DA_DAT, TrangThaiThietKe.HET_HAN, TrangThaiThietKe.KHACH_HUY],
    };

    public static bool ChoPhep(TrangThaiThietKe tu, TrangThaiThietKe den) =>
        Cho.TryGetValue(tu, out var ds) && ds.Contains(den);

    public static void Chuyen(ThietKeBanh tk, TrangThaiThietKe den)
    {
        if (!ChoPhep(tk.TrangThai, den))
            throw new NghiepVuException($"Không thể chuyển thiết kế từ {TenTrangThai(tk.TrangThai)} sang {TenTrangThai(den)}.");
        tk.TrangThai = den;
    }

    public static bool KhachSuaDuoc(TrangThaiThietKe t) =>
        t is TrangThaiThietKe.NHAP or TrangThaiThietKe.CAN_SUA;

    public static string TenTrangThai(TrangThaiThietKe t) => t switch
    {
        TrangThaiThietKe.NHAP => "Nháp",
        TrangThaiThietKe.CHO_DUYET => "Chờ duyệt",
        TrangThaiThietKe.CAN_SUA => "Cần sửa",
        TrangThaiThietKe.DA_BAO_GIA => "Đã báo giá",
        TrangThaiThietKe.DA_DAT => "Đã đặt",
        TrangThaiThietKe.TU_CHOI => "Từ chối",
        TrangThaiThietKe.HET_HAN => "Hết hạn",
        TrangThaiThietKe.KHACH_HUY => "Khách hủy",
        _ => t.ToString()
    };
}

public record DongThanhPhanGia(
    int ThanhPhanId,
    string Ten,
    NhomThanhPhan Nhom,
    int? SanPhamNenId,
    decimal GiaBan,
    int SoLuongToiDa,
    bool DangKinhDoanh,
    int SoLuong);

public record KetQuaGiaThietKe(decimal GiaNen, decimal GiaThanhPhan, decimal GiaTamTinh);

public static class TinhGiaThietKe
{
    public static KetQuaGiaThietKe Tinh(int sanPhamNenId, decimal giaNen, IReadOnlyList<DongThanhPhanGia> dong)
    {
        var cotCount = dong.Count(d => d.Nhom == NhomThanhPhan.COT);
        if (cotCount == 0) throw new NghiepVuException("Thiết kế phải có cốt bánh.");
        if (cotCount > 1) throw new NghiepVuException("Thiết kế chỉ được chọn một cốt bánh.");

        if (dong.Count(d => d.Nhom == NhomThanhPhan.VIEN) > 1)
            throw new NghiepVuException("Chỉ được chọn tối đa một kiểu viền bánh.");
        if (dong.Count(d => d.Nhom == NhomThanhPhan.IN_AN) > 1)
            throw new NghiepVuException("Chỉ được chọn tối đa một dịch vụ in ảnh.");
        if (dong.Count(d => d.Nhom == NhomThanhPhan.TANG) > 1)
            throw new NghiepVuException("Chỉ được chọn tối đa một tầng bánh phụ.");

        var tongTopping = dong.Where(d => d.Nhom == NhomThanhPhan.TOPPING).Sum(d => d.SoLuong);
        if (tongTopping > ThietKeRule.ToppingToiDa)
            throw new NghiepVuException($"Số lượng topping tối đa là {ThietKeRule.ToppingToiDa}.");

        if (dong.GroupBy(d => d.ThanhPhanId).FirstOrDefault(g => g.Count() > 1) is { } lap)
            throw new NghiepVuException($"Thành phần '{lap.First().Ten}' bị lặp; gộp vào một dòng với tổng số lượng.");

        foreach (var d in dong)
        {
            if (d.SanPhamNenId.HasValue && d.SanPhamNenId.Value != sanPhamNenId)
                throw new NghiepVuException($"Thành phần '{d.Ten}' không dùng được cho kích thước này.");

            if (!d.DangKinhDoanh)
                throw new NghiepVuException($"Thành phần '{d.Ten}' đã ngừng kinh doanh.");

            if (d.SoLuong < 1 || d.SoLuong > d.SoLuongToiDa)
                throw new NghiepVuException($"Số lượng thành phần '{d.Ten}' không hợp lệ (tối đa {d.SoLuongToiDa}).");
        }

        var giaThanhPhan = dong.Sum(d => d.GiaBan * d.SoLuong);
        return new KetQuaGiaThietKe(giaNen, giaThanhPhan, giaNen + giaThanhPhan);
    }
}
