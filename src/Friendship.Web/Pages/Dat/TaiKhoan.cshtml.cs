using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

public class TaiKhoanModel(FriendshipDb db, KhachHangAuthService auth, TaiKhoanService taiKhoan) : PageModel
{
    public bool DaDangNhap { get; set; }
    public bool LaNhanVien { get; set; }
    public string? TenNhanVien { get; set; }
    public string? VaiTroNhanVien { get; set; }
    public KhachHang? Khach { get; set; }
    public int SoDonHang { get; set; }
    public int SoGopY { get; set; }

    [BindProperty]
    public CapNhatHoSoInput HoSo { get; set; } = new();

    // Dành cho trường hợp chưa đăng nhập mà đăng nhập trực tiếp tại trang tài khoản
    [BindProperty]
    public DangNhapInput LoginInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        LaNhanVien = User.Identity?.IsAuthenticated == true;
        if (LaNhanVien)
        {
            TenNhanVien = User.Identity?.Name ?? "Nhân viên";
            VaiTroNhanVien = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        }

        Khach = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);

        if (Khach is not null)
        {
            DaDangNhap = true;
            HoSo.TenKhachHang = Khach.TenKhachHang;
            HoSo.Email = Khach.Email;
            HoSo.DiaChi = Khach.DiaChi;

            var donCount = await db.DonHangs.CountAsync(d => d.KhachHangId == Khach.Id || (d.KhachHang != null && d.KhachHang.SoDienThoai == Khach.SoDienThoai));
            var thietKeCount = await db.ThietKeBanhs.CountAsync(t => (t.KhachHangId == Khach.Id || (t.KhachHang != null && t.KhachHang.SoDienThoai == Khach.SoDienThoai)) && t.DonHangId == null);
            SoDonHang = donCount + thietKeCount;
            SoGopY = await db.GopYs.CountAsync(g => g.KhachHangId == Khach.Id || g.SoDienThoai == Khach.SoDienThoai);
        }
        else
        {
            DaDangNhap = false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDangNhapAsync()
    {
        if (string.IsNullOrWhiteSpace(LoginInput.SoDienThoai) || string.IsNullOrWhiteSpace(LoginInput.MatKhau))
        {
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ số điện thoại hoặc tên đăng nhập và mật khẩu.");
            return await OnGetAsync();
        }

        try
        {
            var kh = await auth.DangNhapAsync(LoginInput.SoDienThoai, LoginInput.MatKhau);
            await CustomerAuthHelper.SignInCustomerAsync(HttpContext, kh);
            TempData["Message"] = $"Chào mừng bạn {kh.TenKhachHang} quay trở lại!";
            return RedirectToPage("/Dat/TaiKhoan");
        }
        catch (NghiepVuException)
        {
            try
            {
                var nv = await taiKhoan.DangNhapAsync(LoginInput.SoDienThoai, LoginInput.MatKhau);
                await SignInHelper.SignInAsync(HttpContext, nv);
                TempData["Message"] = $"Đăng nhập nhân viên {nv.HoTen} ({nv.VaiTro}) thành công.";
                return LocalRedirect("/TongQuan");
            }
            catch
            {
                ModelState.AddModelError("", "Sai số điện thoại/tên đăng nhập hoặc mật khẩu.");
                return await OnGetAsync();
            }
        }
    }

    public async Task<IActionResult> OnPostCapNhatAsync()
    {
        var kh = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);
        if (kh is null) return RedirectToPage("/Dat/DangNhap");

        kh.TenKhachHang = HoSo.TenKhachHang?.Trim() ?? kh.TenKhachHang;
        kh.Email = string.IsNullOrWhiteSpace(HoSo.Email) ? null : HoSo.Email.Trim();
        kh.DiaChi = string.IsNullOrWhiteSpace(HoSo.DiaChi) ? null : HoSo.DiaChi.Trim();

        await db.SaveChangesAsync();
        TempData["Message"] = "Đã cập nhật thông tin tài khoản thành công.";
        return RedirectToPage("/Dat/TaiKhoan");
    }

    public class CapNhatHoSoInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string TenKhachHang { get; set; } = "";
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
    }

    public class DangNhapInput
    {
        public string SoDienThoai { get; set; } = "";
        public string MatKhau { get; set; } = "";
    }
}
