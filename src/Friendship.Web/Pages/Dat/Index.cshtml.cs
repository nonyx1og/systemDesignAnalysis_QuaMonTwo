using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Dat;

/// <summary>Một mẫu gợi ý. Thành phần khớp với MAU_THIET_KE trong thiet-ke.js: đổi bên đó thì đổi ở đây.</summary>
public record MauGoiY(
    int Id, string Ten, string MoTa, string TomTat,
    string TuKhoaCot, string? TuKhoaVien, int SoDau, bool CoAnhIn,
    string Chu, string? TenKem, int SoMacaron = 0, int SoCherry = 0, int SoSocola = 0)
{
    /// <summary>Giá thấp nhất của mẫu trên các bánh nền đang bán; null khi danh mục chưa có đủ dữ liệu.</summary>
    public decimal? GiaTu { get; init; }
    public string? CoGiaTu { get; init; }
}

public class IndexModel(ThanhPhanService thanhPhanService, SanPhamService sanPhamService) : PageModel
{
    static readonly MauGoiY[] Mau =
    [
        new(1, "Dâu tây & Macaron Hoàng Gia",
            "Kem vani satin, viền kem sò Pháp, vương miện dâu tây đỏ mọng kết hợp bánh macaron kiểu Pháp ngọt ngào.",
            "Cốt vani, viền kem xoắn, 4 dâu tây, 3 macaron",
            "Cốt vani", "Viền kem xoắn", 4, false, "Happy Birthday", "Kem vani", SoMacaron: 3),

        new(2, "Socola & Cherry Dạ Tiệc",
            "Kem ganache socola đen bóng mượt, sốt socola rủ, thanh socola vát nghệ thuật, cherry ruby và dâu tây tươi.",
            "Cốt socola, viền kem xoắn, 3 cherry, 3 socola miếng, 2 dâu tây",
            "Cốt socola", "Viền kem xoắn", 2, false, "Happy Birthday!", "Socola", SoCherry: 3, SoSocola: 3),

        new(3, "Khung Kỷ Niệm Bách Hoa",
            "Khung ảnh in ăn được trang trọng ở trung tâm, viền kem sò ngọc trai, điểm xuyết macaron pastel và cherry tươi.",
            "Cốt vani, viền kem xoắn, khung in ảnh, 2 macaron, 2 cherry",
            "Cốt vani", "Viền kem xoắn", 0, true, "Family Love", "Trắng sữa", SoMacaron: 2, SoCherry: 2),
    ];

    public List<MauGoiY> Maus { get; private set; } = [];

    /// <summary>Phiếu ví dụ ở màn hình đầu: mẫu 1 trên bánh 20cm, lấy giá thật từ danh mục.</summary>
    public string? PhieuCoBanh { get; private set; }
    public decimal? PhieuTong { get; private set; }
    public decimal? PhieuCoc { get; private set; }

    public async Task OnGetAsync()
    {
        var thanhPhans = await thanhPhanService.TimAsync(null, true);
        var nenIds = thanhPhans.Where(t => t.Nhom == NhomThanhPhan.COT).Select(t => t.SanPhamNenId).ToHashSet();
        var nens = (await sanPhamService.TimAsync(null, true)).Where(s => nenIds.Contains(s.Id)).ToList();

        Maus = Mau.Select(m =>
        {
            var best = nens.Select(n => (Nen: n, Gia: Gia(m, n, thanhPhans))).Where(x => x.Gia is not null)
                .OrderBy(x => x.Gia).FirstOrDefault();
            return best.Nen is null ? m : m with { GiaTu = best.Gia, CoGiaTu = best.Nen.KichThuoc };
        }).ToList();

        var nen20 = nens.FirstOrDefault(n => n.KichThuoc == "20cm");
        if (nen20 is not null && Gia(Mau[0], nen20, thanhPhans) is { } tong)
        {
            PhieuCoBanh = nen20.KichThuoc;
            PhieuTong = tong;
            PhieuCoc = CocRule.Khoang(tong).Min;
        }
    }

    /// <summary>Giá tạm tính của mẫu trên một bánh nền, cùng cách chọn thành phần như napMau() ở thiet-ke.js.</summary>
    static decimal? Gia(MauGoiY m, SanPham nen, List<ThanhPhanTrangTri> tps)
    {
        ThanhPhanTrangTri? Tim(NhomThanhPhan nhom, string tuKhoa, bool theoNen) =>
            tps.FirstOrDefault(t => t.Nhom == nhom
                && t.Ten.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)
                && (!theoNen || t.SanPhamNenId is null || t.SanPhamNenId == nen.Id));

        var cot = Tim(NhomThanhPhan.COT, m.TuKhoaCot, true);
        if (cot is null) return null;

        var tong = nen.DonGia + cot.GiaBan;
        if (m.TuKhoaVien is not null && Tim(NhomThanhPhan.VIEN, m.TuKhoaVien, true) is { } vien) tong += vien.GiaBan;
        if (m.SoDau > 0 && Tim(NhomThanhPhan.TOPPING, "Dâu tây", false) is { } dau)
            tong += dau.GiaBan * Math.Min(m.SoDau, dau.SoLuongToiDa);
        if (m.SoMacaron > 0 && Tim(NhomThanhPhan.TOPPING, "Macaron", false) is { } macaron)
            tong += macaron.GiaBan * Math.Min(m.SoMacaron, macaron.SoLuongToiDa);
        if (m.SoCherry > 0 && Tim(NhomThanhPhan.TOPPING, "Cherry", false) is { } cherry)
            tong += cherry.GiaBan * Math.Min(m.SoCherry, cherry.SoLuongToiDa);
        if (m.SoSocola > 0 && Tim(NhomThanhPhan.TOPPING, "Socola", false) is { } socola)
            tong += socola.GiaBan * Math.Min(m.SoSocola, socola.SoLuongToiDa);
        if (m.CoAnhIn && tps.FirstOrDefault(t => t.Nhom == NhomThanhPhan.IN_AN) is { } inAn) tong += inAn.GiaBan;
        return tong;
    }
}
