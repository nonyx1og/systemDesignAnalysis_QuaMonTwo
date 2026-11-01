using Friendship.Domain;

namespace Friendship.Web.Auth;

public record MenuItem(string Text, string Page, VaiTro[]? Roles = null, string Nhom = "", string Icon = "home");

public static class Menu
{
    // Mỗi kế hoạch sau thêm mục của mình vào đây, theo thứ tự hiển thị. Mục cùng Nhom hiển thị dưới một tiêu đề nhóm.
    static readonly MenuItem[] All =
    [
        new("Tổng quan", "/Index"),
        new("Bán lẻ", "/BanHang/BanLe", [VaiTro.BAN_HANG], "Bán hàng", "cart"),
        new("Đơn hàng", "/BanHang/Don/Index", [VaiTro.BAN_HANG], "Bán hàng", "receipt"),
        new("Tạo đơn đặt", "/BanHang/DonDat", [VaiTro.BAN_HANG], "Bán hàng", "file-plus"),
        new("Cọc online", "/BanHang/CocOnline", [VaiTro.BAN_HANG], "Bán hàng", "qr"),
        new("Ca thu ngân", "/BanHang/Ca/Index", [VaiTro.BAN_HANG], "Bán hàng", "wallet"),
        new("Đơn cần làm", "/Bep/Index", [VaiTro.SAN_XUAT], "Bếp", "pot"),
        new("Duyệt thiết kế", "/Bep/ThietKe/Index", [VaiTro.SAN_XUAT], "Bếp", "palette"),
        new("Ghi nhận mẻ bánh", "/Bep/MeBanh", [VaiTro.SAN_XUAT], "Bếp", "layers"),
        new("Nhập kho","/Kho/NhapKho", [VaiTro.KHO], "Kho", "arrow-down"),
        new("Xuất nguyên liệu", "/Kho/XuatNguyenLieu", [VaiTro.KHO], "Kho", "arrow-up"),
        new("Kiểm kê", "/Kho/KiemKe/Index", [VaiTro.KHO], "Kho", "clipboard"),
        new("Đề xuất nhập hàng", "/Kho/DeXuatNhap", [VaiTro.KHO, VaiTro.QUAN_LY], "Kho", "list"),
        new("Cảnh báo HSD", "/CanhBao/Index", [VaiTro.KHO, VaiTro.BAN_HANG, VaiTro.QUAN_LY], "Kho", "alert"),
        new("Phiếu hủy", "/Huy/Index", [VaiTro.KHO, VaiTro.BAN_HANG, VaiTro.SAN_XUAT], "Kho", "trash"),
        new("Sản phẩm","/QuanLy/SanPham/Index", [VaiTro.QUAN_LY], "Danh mục", "package"),
        new("Thành phần bánh", "/QuanLy/ThanhPhan/Index", [VaiTro.QUAN_LY], "Danh mục", "cake"),
        new("Nguyên liệu","/QuanLy/NguyenLieu/Index", [VaiTro.QUAN_LY], "Danh mục", "wheat"),
        new("Khuyến mãi", "/QuanLy/KhuyenMai/Index", [VaiTro.QUAN_LY], "Danh mục", "tag"),
        new("Xả hàng", "/QuanLy/XaHang/Index", [VaiTro.QUAN_LY], "Danh mục", "percent"),
        new("Duyệt kiểm kê", "/QuanLy/DuyetKiemKe/Index", [VaiTro.QUAN_LY], "Duyệt", "check"),
        new("Duyệt phiếu hủy", "/QuanLy/DuyetHuy/Index", [VaiTro.QUAN_LY], "Duyệt", "check-file"),
        new("Quét bùng cọc", "/QuanLy/BungCoc", [VaiTro.QUAN_LY], "Duyệt", "scan"),
        new("Báo cáo doanh thu", "/QuanLy/BaoCao/DoanhThu", [VaiTro.QUAN_LY], "Báo cáo", "chart"),
        new("Hủy và bán chạy", "/QuanLy/BaoCao/HuyBanChay", [VaiTro.QUAN_LY], "Báo cáo", "trend"),
        new("Giá trị tồn kho", "/QuanLy/BaoCao/GiaTriTonKho", [VaiTro.QUAN_LY], "Báo cáo", "boxes"),
        new("Tài khoản", "/Admin/TaiKhoan/Index", [VaiTro.QUAN_TRI], "Quản trị", "users"),
    ];

    public static IEnumerable<MenuItem> For(VaiTro vaiTro) =>
        All.Where(m => m.Roles is null || m.Roles.Contains(vaiTro));
}
