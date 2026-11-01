using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Account;

public class ChangePasswordModel(TaiKhoanService taiKhoan) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu hiện tại")] public string MatKhauCu { get; set; } = "";
    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu mới"), MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
    public string MatKhauMoi { get; set; } = "";
    [BindProperty, Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại không khớp")] public string NhapLai { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var nv = await taiKhoan.DoiMatKhauAsync(User.NhanVienId(), MatKhauCu, MatKhauMoi);
            await SignInHelper.SignInAsync(HttpContext, nv); // phiên hiện tại giữ lại, các cookie cũ khác mất hiệu lực
            TempData["Message"] = "Đã đổi mật khẩu.";
            return RedirectToPage("/Index");
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
