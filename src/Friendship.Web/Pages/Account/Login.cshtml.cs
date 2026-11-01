using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Account;

public class LoginModel(TaiKhoanService taiKhoan) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nhập tên đăng nhập")] public string TenDangNhap { get; set; } = "";
    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu")] public string MatKhau { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var nv = await taiKhoan.DangNhapAsync(TenDangNhap, MatKhau);
            await SignInHelper.SignInAsync(HttpContext, nv);
            var target = !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) && ReturnUrl != "/" 
                ? ReturnUrl 
                : "/TongQuan";
            return LocalRedirect(target);
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
