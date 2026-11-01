using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

[EnableRateLimiting("khach")]
public class TheoModel(ThietKeService thietKeService, FriendshipDb db) : PageModel
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

    public async Task<IActionResult> OnPostXacNhanCocAsync()
    {
        if (string.IsNullOrWhiteSpace(Ma)) return NotFound();
        var tk = await thietKeService.LayTheoMaAsync(Ma);
        if (tk is null) return NotFound();
        if (tk.TrangThai != TrangThaiThietKe.DA_BAO_GIA)
        {
            TempData["Error"] = "Thiết kế chưa được duyệt báo giá hoặc không ở trạng thái chờ cọc.";
            return Redirect($"/Dat/Theo/{Ma}");
        }

        try
        {
            var khoangCoc = CocRule.Khoang(tk.GiaCuoi);
            var nhanVienId = await db.NhanViens
                .Where(n => n.DangHoatDong)
                .OrderBy(n => n.VaiTro == VaiTro.BAN_HANG ? 0 : 1)
                .Select(n => n.Id)
                .FirstOrDefaultAsync();

            if (nhanVienId == 0)
            {
                TempData["Error"] = "Không tìm thấy tài khoản nhân viên xử lý đơn.";
                return Redirect($"/Dat/Theo/{Ma}");
            }

            var don = await thietKeService.XacNhanCocAsync(tk.Id, nhanVienId, khoangCoc.Min);
            TempData["Message"] = $"Xác nhận đặt cọc thành công {khoangCoc.Min:N0} ₫! Bánh đã được đưa vào danh sách làm của bếp bánh.";
        }
        catch (NghiepVuException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect($"/Dat/Theo/{Ma}");
    }
}

