using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

public class HoaDonModel(FriendshipDb db) : PageModel
{
    public DonHang? Don { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Don = await db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiets)
                .ThenInclude(c => c.SanPham)
            .Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (Don is null)
        {
            TempData["Error"] = "Không tìm thấy hoá đơn yêu cầu.";
            return RedirectToPage("/Dat/DonHang");
        }

        return Page();
    }
}
