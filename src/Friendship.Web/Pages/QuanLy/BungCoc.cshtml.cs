using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy;

public class BungCocModel(DonDatService donDat) : PageModel
{
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var ds = await donDat.QuetBungCocAsync(User.NhanVienId());
            TempData["Message"] = ds.Count == 0
                ? "Không có đơn nào quá 24 giờ sau giờ hẹn."
                : $"Đã chuyển {ds.Count} đơn sang bùng cọc ({string.Join(", ", ds.Select(d => d.Ma))}); phiếu hủy nháp đã được tạo.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage();
    }
}
