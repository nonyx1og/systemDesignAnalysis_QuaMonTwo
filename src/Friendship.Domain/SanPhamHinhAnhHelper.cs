namespace Friendship.Domain;

public static class SanPhamHinhAnhHelper
{
    public static readonly (string Ten, string File)[] DanhSachAnhMau =
    [
        ("Bánh kem sinh nhật", "/img/san-pham/banh-kem-sinh-nhat.jpg"),
        ("Bánh kem matcha dâu", "/img/san-pham/banh-kem-matcha-dau.jpg"),
        ("Bánh kem tùy chỉnh", "/img/san-pham/banh-kem-tuy-chinh.jpg"),
        ("Bánh su kem", "/img/san-pham/banh-su-kem.jpg"),
        ("Tart trứng", "/img/san-pham/tart-trung.jpg"),
        ("Bông lan", "/img/san-pham/bong-lan.jpg"),
        ("Bông lan trứng muối", "/img/san-pham/bong-lan-trung-muoi.jpg"),
        ("Croissant", "/img/san-pham/croissant.jpg"),
        ("Bánh mì bơ tỏi", "/img/san-pham/banh-mi-bo-toi.jpg"),
        ("Bánh flan", "/img/san-pham/banh-flan.jpg"),
        ("Cookie socola", "/img/san-pham/cookie-socola.jpg")
    ];

    public static string LayHinhAnh(string? tenBanh, string? hinhAnh = null)
    {
        if (!string.IsNullOrWhiteSpace(hinhAnh)) return hinhAnh;
        var ten = (tenBanh ?? "").Trim().ToLowerInvariant();
        if (ten.Contains("su kem")) return "/img/san-pham/banh-su-kem.jpg";
        if (ten.Contains("tart")) return "/img/san-pham/tart-trung.jpg";
        if (ten.Contains("trứng muối")) return "/img/san-pham/bong-lan-trung-muoi.jpg";
        if (ten.Contains("bông lan")) return "/img/san-pham/bong-lan.jpg";
        if (ten.Contains("matcha")) return "/img/san-pham/banh-kem-matcha-dau.jpg";
        if (ten.Contains("tùy chỉnh") || ten.Contains("tuy chinh")) return "/img/san-pham/banh-kem-tuy-chinh.jpg";
        if (ten.Contains("sinh nhật") || ten.Contains("bánh kem") || ten.Contains("gato")) return "/img/san-pham/banh-kem-sinh-nhat.jpg";
        if (ten.Contains("croissant")) return "/img/san-pham/croissant.jpg";
        if (ten.Contains("bơ tỏi") || ten.Contains("bánh mì")) return "/img/san-pham/banh-mi-bo-toi.jpg";
        if (ten.Contains("flan")) return "/img/san-pham/banh-flan.jpg";
        if (ten.Contains("cookie") || ten.Contains("bánh quy")) return "/img/san-pham/cookie-socola.jpg";
        return "/img/san-pham/banh-kem-sinh-nhat.jpg";
    }
}
