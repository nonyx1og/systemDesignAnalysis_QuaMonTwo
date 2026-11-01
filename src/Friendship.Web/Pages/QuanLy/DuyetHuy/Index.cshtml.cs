using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.DuyetHuy;

public class IndexModel(HuyHangService huy) : PageModel
{
    public List<PhieuXuatHuy> NhapCuaToi { get; set; } = [];
    public List<PhieuXuatHuy> ChoDuyet { get; set; } = [];
    public List<PhieuXuatHuy> DaXuLy { get; set; } = [];

    public async Task OnGetAsync()
    {
        NhapCuaToi = await huy.TimAsync(TrangThaiPhieu.NHAP, User.NhanVienId());
        ChoDuyet = await huy.TimAsync(TrangThaiPhieu.CHO_DUYET);
        DaXuLy = (await huy.TimAsync(null))
            .Where(p => p.TrangThai is TrangThaiPhieu.DA_DUYET or TrangThaiPhieu.TU_CHOI).Take(20).ToList();
    }

    public async Task<IActionResult> OnPostGuiDuyetAsync(int id)
    {
        var p = await huy.LayAsync(id);
        if (p is null) return NotFound();
        if (p.NguoiLapId != User.NhanVienId()) return Forbid();
        try
        {
            await huy.GuiDuyetAsync(id);
            TempData["Message"] = "Đã gửi phiếu cho quản lý duyệt.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            await OnGetAsync();
            return Page();
        }
    }
}
