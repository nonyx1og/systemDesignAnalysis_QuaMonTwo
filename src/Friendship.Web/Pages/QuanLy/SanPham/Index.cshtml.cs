using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanPhamEntity = Friendship.Domain.Entities.SanPham;

namespace Friendship.Web.Pages.QuanLy.SanPham;

public class IndexModel(SanPhamService sanPham) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public bool? KinhDoanh { get; set; }
    public List<SanPhamEntity> Items { get; set; } = [];

    public async Task OnGetAsync() => Items = await sanPham.TimAsync(Q, KinhDoanh);

    public async Task<IActionResult> OnPostToggleAsync(int id, bool kinhDoanh)
    {
        try
        {
            await sanPham.DatKinhDoanhAsync(id, kinhDoanh);
            TempData["Message"] = kinhDoanh ? "Đã bán lại sản phẩm." : "Đã ngừng bán sản phẩm.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { Q, KinhDoanh });
    }
}
