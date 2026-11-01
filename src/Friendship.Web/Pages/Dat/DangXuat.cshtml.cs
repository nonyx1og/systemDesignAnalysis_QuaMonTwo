using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Dat;

public class DangXuatModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await CustomerAuthHelper.SignOutCustomerAsync(HttpContext);
        TempData["Message"] = "Bạn đã đăng xuất tài khoản.";
        return RedirectToPage("/Dat/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CustomerAuthHelper.SignOutCustomerAsync(HttpContext);
        TempData["Message"] = "Bạn đã đăng xuất tài khoản.";
        return RedirectToPage("/Dat/Index");
    }
}
