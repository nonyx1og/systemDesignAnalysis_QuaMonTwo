using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KhuyenMaiEntity = Friendship.Domain.Entities.KhuyenMai;

namespace Friendship.Web.Pages.QuanLy.KhuyenMai;

public class IndexModel(KhuyenMaiService khuyenMai, TimeProvider clock) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    public List<KhuyenMaiEntity> Items { get; set; } = [];
    public DateOnly HomNay => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    public async Task OnGetAsync() => Items = await khuyenMai.TimAsync(Q);

    public async Task<IActionResult> OnPostToggleAsync(int id, bool apDung)
    {
        try
        {
            await khuyenMai.DatApDungAsync(id, apDung);
            TempData["Message"] = apDung ? "Đã bật lại voucher." : "Đã ngừng voucher.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { Q });
    }
}
