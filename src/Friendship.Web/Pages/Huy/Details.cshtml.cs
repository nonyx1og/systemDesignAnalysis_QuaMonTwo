using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Huy;

public class DetailsModel(HuyHangService huy) : PageModel
{
    public PhieuXuatHuy Phieu { get; set; } = null!;
    public bool LaNguoiLap => Phieu.NguoiLapId == User.NhanVienId();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await huy.LayAsync(id);
        if (p is null || p.NguoiLapId != User.NhanVienId()) return NotFound();   // phiếu của người khác coi như không có; quản lý xem ở Duyệt phiếu hủy
        Phieu = p;
        return Page();
    }

    public Task<IActionResult> OnPostGuiDuyetAsync(int id) =>
        LamAsync(id, () => huy.GuiDuyetAsync(id), "Đã gửi phiếu cho quản lý duyệt.", veDanhSach: false);

    public Task<IActionResult> OnPostXoaNhapAsync(int id) =>
        LamAsync(id, () => huy.HuyNhapAsync(id), "Đã xóa phiếu nháp.", veDanhSach: true);

    async Task<IActionResult> LamAsync(int id, Func<Task> lam, string thongBao, bool veDanhSach)
    {
        var p = await huy.LayAsync(id);
        if (p is null) return NotFound();
        if (p.NguoiLapId != User.NhanVienId()) return Forbid();                      // chỉ người lập thao tác trên phiếu của mình
        try
        {
            await lam();
            TempData["Message"] = thongBao;
            return veDanhSach ? RedirectToPage("Index") : RedirectToPage(new { id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            var moi = await huy.LayAsync(id);
            if (moi is null) return NotFound();
            Phieu = moi;
            return Page();
        }
    }
}
