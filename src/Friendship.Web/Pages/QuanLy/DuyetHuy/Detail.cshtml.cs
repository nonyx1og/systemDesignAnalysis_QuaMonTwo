using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.DuyetHuy;

public class DetailModel(HuyHangService huy) : PageModel
{
    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public PhieuXuatHuy? Phieu { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Phieu = await huy.LayAsync(Id);
        return Phieu is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostDuyetAsync()
    {
        try
        {
            await huy.DuyetAsync(Id, User.NhanVienId());
            TempData["Message"] = "Đã duyệt phiếu hủy. Tồn kho đã được cập nhật.";
            return RedirectToPage("Index");
        }
        catch (NghiepVuException e) { return await LoiAsync(e.Message); }
    }

    public async Task<IActionResult> OnPostTuChoiAsync(string? lyDo)
    {
        try
        {
            await huy.TuChoiAsync(Id, User.NhanVienId(), lyDo);
            TempData["Message"] = "Đã từ chối phiếu hủy. Tồn kho không đổi.";
            return RedirectToPage("Index");
        }
        catch (NghiepVuException e) { return await LoiAsync(e.Message); }
    }

    async Task<IActionResult> LoiAsync(string thongBao)
    {
        Phieu = await huy.LayAsync(Id);
        if (Phieu is null) return NotFound();
        ModelState.AddModelError("", thongBao);
        return Page();
    }
}
