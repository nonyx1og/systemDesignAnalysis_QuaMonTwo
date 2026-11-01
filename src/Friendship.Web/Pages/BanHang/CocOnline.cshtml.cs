using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang;

[Authorize(Roles = $"{nameof(VaiTro.BAN_HANG)},{nameof(VaiTro.QUAN_LY)},{nameof(VaiTro.QUAN_TRI)}")]
public class CocOnlineModel(ThietKeService thietKeService, CaService caService) : PageModel
{
    public List<ThietKeBanh> Items { get; set; } = [];
    public bool CoCa { get; set; }

    public async Task OnGetAsync()
    {
        CoCa = await caService.DangMoAsync(User.NhanVienId()) is not null;
        Items = await thietKeService.DaBaoGiaAsync();
    }

    public async Task<IActionResult> OnPostXacNhanAsync(int thietKeId, decimal tienCoc)
    {
        CoCa = await caService.DangMoAsync(User.NhanVienId()) is not null;
        if (!CoCa)
        {
            TempData["Error"] = "Thu ngân phải mở ca trước khi xác nhận nhận cọc.";
            return RedirectToPage();
        }

        try
        {
            var don = await thietKeService.XacNhanCocAsync(thietKeId, User.NhanVienId(), tienCoc);
            TempData["Message"] = $"Đã xác nhận cọc thành công! Đơn hàng {don.Ma} đã được tạo và gửi tới bếp.";
            return Redirect($"/BanHang/Don/Details/{don.Id}");
        }
        catch (NghiepVuException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage();
        }
    }
}
