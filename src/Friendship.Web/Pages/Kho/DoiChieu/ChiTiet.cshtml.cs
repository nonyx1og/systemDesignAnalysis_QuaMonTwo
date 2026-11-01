using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho.DoiChieu;

public class ChiTietModel(KhoService khoService) : PageModel
{
    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public BangXemTruocDoiChieu? Preview { get; set; }
    public BienBanDoiChieuKho? BienBanDaLap { get; set; }

    [BindProperty] public string? GhiChuGiaiTrinh { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        BienBanDaLap = await khoService.LayBienBanTheoPhieuDinhLuongAsync(Id);
        try
        {
            Preview = await khoService.XemTruocDoiChieuAsync(Id);
            return Page();
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostXacNhanAsync()
    {
        try
        {
            var nvId = User.NhanVienId();
            var bb = await khoService.XacNhanDoiChieuAsync(Id, nvId, GhiChuGiaiTrinh);
            TempData["Message"] = $"Đã xác nhận hoàn tất đối chiếu và lập biên bản {bb.Ma} thành công.";
            return RedirectToPage(new { id = Id });
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            return await OnGetAsync();
        }
    }
}
