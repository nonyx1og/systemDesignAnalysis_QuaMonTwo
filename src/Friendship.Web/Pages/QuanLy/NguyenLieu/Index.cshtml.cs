using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.NguyenLieu;

public class IndexModel(NguyenLieuService nguyenLieu) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    public List<NguyenLieuTon> Items { get; set; } = [];

    public async Task OnGetAsync() => Items = await nguyenLieu.TimAsync(Q);

    public async Task<IActionResult> OnPostXoaAsync(int id)
    {
        try
        {
            await nguyenLieu.XoaAsync(id);
            TempData["Message"] = "Đã xóa nguyên liệu.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { Q });
    }
}
