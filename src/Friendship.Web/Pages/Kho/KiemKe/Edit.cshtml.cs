using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho.KiemKe;

public class EditModel(KiemKeService kiemKe) : PageModel
{
    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public List<RowInput> Rows { get; set; } = [];
    public PhieuKiemKe? Phieu { get; set; }

    public class RowInput
    {
        public int ChiTietId { get; set; }
        public decimal? SoLuongThucTe { get; set; }   // nullable: ô bị xóa trống báo lỗi thân thiện, không làm hỏng trang
        public string? LyDo { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Phieu = await kiemKe.LayAsync(Id);
        if (Phieu is null) return NotFound();
        Rows = Phieu.ChiTiets.Select(c => new RowInput { ChiTietId = c.Id, SoLuongThucTe = c.SoLuongThucTe, LyDo = c.LyDo }).ToList();
        return Page();
    }

    public Task<IActionResult> OnPostLuuAsync() => ThucHienAsync(false, "Đã lưu nháp phiếu kiểm kê.");
    public Task<IActionResult> OnPostGuiAsync() => ThucHienAsync(true, "Đã gửi phiếu kiểm kê cho quản lý duyệt.");

    public async Task<IActionResult> OnPostHuyAsync()
    {
        try
        {
            await kiemKe.HuyNhapAsync(Id);
            TempData["Message"] = "Đã hủy phiếu kiểm kê nháp.";
            return RedirectToPage("Index");
        }
        catch (NghiepVuException e) { return await LoiAsync(e); }
    }

    async Task<IActionResult> ThucHienAsync(bool gui, string thongBao)
    {
        try
        {
            if (Rows.Any(r => r.SoLuongThucTe is null))
                throw new NghiepVuException("Nhập số lượng thực tế cho mọi lô (nhập 0 nếu không còn).");
            if (Rows.Count > 0)
                await kiemKe.LuuSoLieuAsync(Id, Rows.Select(r => new DongKiemKeInput(r.ChiTietId, r.SoLuongThucTe!.Value, r.LyDo)).ToList());
            if (gui) await kiemKe.GuiDuyetAsync(Id);
            TempData["Message"] = thongBao;
            return RedirectToPage(new { id = Id });
        }
        catch (NghiepVuException e) { return await LoiAsync(e); }
    }

    async Task<IActionResult> LoiAsync(NghiepVuException e)
    {
        Phieu = await kiemKe.LayAsync(Id);
        if (Phieu is null) return NotFound();
        ModelState.AddModelError("", e.Message);
        return Page();
    }
}
