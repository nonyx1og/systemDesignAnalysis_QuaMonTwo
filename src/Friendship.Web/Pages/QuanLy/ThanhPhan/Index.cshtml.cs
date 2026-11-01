using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.ThanhPhan;

public class IndexModel(ThanhPhanService thanhPhan) : PageModel
{
    [BindProperty(SupportsGet = true)] public NhomThanhPhan? Nhom { get; set; }
    [BindProperty(SupportsGet = true)] public bool? KinhDoanh { get; set; }
    public List<ThanhPhanTrangTri> Items { get; set; } = [];
    public Dictionary<int, decimal> GiaVons { get; set; } = [];

    public async Task OnGetAsync()
    {
        Items = await thanhPhan.TimAsync(Nhom, KinhDoanh);
        foreach (var tp in Items)
        {
            var gv = await thanhPhan.GiaVonAsync(tp.Id);
            GiaVons[tp.Id] = gv.GiaVon;
        }
    }

    public async Task<IActionResult> OnPostToggleAsync(int id, bool kinhDoanh)
    {
        try
        {
            await thanhPhan.DatKinhDoanhAsync(id, kinhDoanh);
            TempData["Message"] = kinhDoanh ? "Đã mở bán lại thành phần." : "Đã ngừng bán thành phần.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { Nhom, KinhDoanh });
    }
}
