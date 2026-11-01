using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.DuyetLoi;

public class IndexModel(DinhLuongService dinhLuongService) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "cho_duyet";
    public List<PhieuBaoLoiMeBanh> PhieuChoDuyet { get; set; } = [];
    public List<PhieuBaoLoiMeBanh> PhieuLichSu { get; set; } = [];

    [BindProperty] public int PhieuId { get; set; }
    [BindProperty] public string? GhiChuQuanLy { get; set; }

    public async Task OnGetAsync()
    {
        await NapDuLieuAsync();
    }

    public async Task<IActionResult> OnPostDuyetAsync(int id, string? ghiChu)
    {
        try
        {
            await dinhLuongService.DuyetPhieuBaoLoiAsync(id, User.NhanVienId(), true, ghiChu);
            TempData["Message"] = "Đã duyệt phiếu báo lỗi mẻ bánh thành công. Định lượng hao phí đã được cộng vào ca làm việc của bếp.";
            return RedirectToPage(new { tab = "cho_duyet" });
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            await NapDuLieuAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostTuChoiAsync(int id, string? ghiChu)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ghiChu))
            {
                throw new NghiepVuException("Vui lòng ghi rõ lý do từ chối phiếu báo lỗi.");
            }
            await dinhLuongService.DuyetPhieuBaoLoiAsync(id, User.NhanVienId(), false, ghiChu);
            TempData["Message"] = "Đã từ chối phiếu báo lỗi mẻ bánh.";
            return RedirectToPage(new { tab = "cho_duyet" });
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            await NapDuLieuAsync();
            return Page();
        }
    }

    async Task NapDuLieuAsync()
    {
        PhieuChoDuyet = await dinhLuongService.LayDanhSachPhieuLoiAsync(TrangThaiPhieu.CHO_DUYET);
        var tatCa = await dinhLuongService.LayDanhSachPhieuLoiAsync();
        PhieuLichSu = tatCa.Where(p => p.TrangThai != TrangThaiPhieu.CHO_DUYET).ToList();
    }
}
