using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho.KiemKe;

public class IndexModel(KiemKeService kiemKe) : PageModel
{
    public PhieuKiemKe? DangMo { get; set; }
    public List<PhieuKiemKe> LichSu { get; set; } = [];

    public async Task OnGetAsync()
    {
        DangMo = await kiemKe.PhieuDangMoAsync();
        LichSu = (await kiemKe.TimAsync(null)).Take(20).ToList();
    }

    public async Task<IActionResult> OnPostTaoAsync()
    {
        var dangMo = await kiemKe.PhieuDangMoAsync();
        if (dangMo is not null)
        {
            TempData["Error"] = "Đã có phiếu kiểm kê đang mở. Hãy hoàn tất phiếu đó trước.";
            return RedirectToPage("Edit", new { id = dangMo.Id });
        }
        try
        {
            var p = await kiemKe.TaoPhieuAsync(User.NhanVienId());
            return RedirectToPage("Edit", new { id = p.Id });
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            return RedirectToPage();
        }
    }
}
