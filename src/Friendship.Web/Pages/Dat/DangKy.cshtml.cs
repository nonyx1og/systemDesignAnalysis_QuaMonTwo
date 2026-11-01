using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Dat;

public class DangKyModel(KhachHangAuthService auth) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    public string HoTen { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng số 0")]
    public string SoDienThoai { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string MatKhau { get; set; } = "";

    [BindProperty, Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string NhapLaiMatKhau { get; set; } = "";

    [BindProperty, EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; set; }

    [BindProperty]
    public string? DiaChi { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var kh = await auth.DangKyAsync(HoTen, SoDienThoai, MatKhau, Email, DiaChi);
            await CustomerAuthHelper.SignInCustomerAsync(HttpContext, kh);
            TempData["Message"] = $"Chúc mừng bạn {kh.TenKhachHang} đã đăng ký tài khoản thành công!";

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);

            return RedirectToPage("/Dat/Menu");
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }
}
