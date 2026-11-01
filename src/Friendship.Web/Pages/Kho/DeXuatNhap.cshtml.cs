using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho;

public class DeXuatNhapModel(KhoService kho) : PageModel
{
    public BangDeXuatNhap Bang { get; set; } = new(0, []);

    public async Task OnGetAsync()
    {
        Bang = await kho.LayDeXuatNhapAsync();
    }
}
