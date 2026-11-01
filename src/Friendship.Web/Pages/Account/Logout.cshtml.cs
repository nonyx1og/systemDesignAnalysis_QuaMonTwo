using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Account;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
