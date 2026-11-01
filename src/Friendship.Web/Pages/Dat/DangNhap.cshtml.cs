using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Dat;

public class DangNhapModel(KhachHangAuthService auth, TaiKhoanService taiKhoan) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Vui lòng nhập số điện thoại hoặc tên đăng nhập")]
    public string SoDienThoai { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string MatKhau { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var kh = await auth.DangNhapAsync(SoDienThoai, MatKhau);
            await CustomerAuthHelper.SignInCustomerAsync(HttpContext, kh);
            TempData["Message"] = $"Chào mừng bạn {kh.TenKhachHang} quay trở lại!";

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);

            return RedirectToPage("/Dat/DonHang");
        }
        catch (NghiepVuException)
        {
            try
            {
                var nv = await taiKhoan.DangNhapAsync(SoDienThoai, MatKhau);
                await SignInHelper.SignInAsync(HttpContext, nv);
                TempData["Message"] = $"Đăng nhập nhân viên {nv.HoTen} ({nv.VaiTro}) thành công.";

                var target = !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) && ReturnUrl != "/" 
                    ? ReturnUrl 
                    : "/TongQuan";
                return LocalRedirect(target);
            }
            catch
            {
                ModelState.AddModelError("", "Sai số điện thoại/tên đăng nhập hoặc mật khẩu.");
                return Page();
            }
        }
    }
}
