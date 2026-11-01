using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Friendship.Web.Pages.Dat;

[EnableRateLimiting("khach")]
public class TheoModel(ThietKeService thietKeService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Ma { get; set; } = "";

    public ThietKeBanh ThietKe { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Ma)) return NotFound();
        var tk = await thietKeService.LayTheoMaAsync(Ma);
        if (tk is null) return NotFound();

        ThietKe = tk;
        return Page();
    }

    public async Task<IActionResult> OnPostGuiAsync()
    {
        if (string.IsNullOrWhiteSpace(Ma)) return NotFound();
        try
        {
            await thietKeService.GuiAsync(Ma);
            TempData["Message"] = "Đã gửi thiết kế tới tiệm bánh. Bếp sẽ kiểm tra và phản hồi sớm nhất!";
        }
        catch (NghiepVuException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return Redirect($"/Dat/Theo/{Ma}");
    }

    public async Task<IActionResult> OnPostHuyAsync()
    {
        if (string.IsNullOrWhiteSpace(Ma)) return NotFound();
        try
        {
            await thietKeService.HuyAsync(Ma);
            TempData["Message"] = "Đã hủy thiết kế.";
        }
        catch (NghiepVuException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return Redirect($"/Dat/Theo/{Ma}");
    }
}
