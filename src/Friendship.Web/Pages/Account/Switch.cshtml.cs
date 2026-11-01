using Friendship.Domain;
using Friendship.Infrastructure;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Account;

[Authorize]
public class SwitchModel(FriendshipDb db) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int id, string? returnUrl)
    {
        var nv = await db.NhanViens.FirstOrDefaultAsync(x => x.Id == id && x.DangHoatDong);
        if (nv is not null)
        {
            await SignInHelper.SignInAsync(HttpContext, nv);
            TempData["Message"] = $"Đã chuyển tài khoản làm việc sang: {nv.HoTen} ({nv.VaiTro.TenHienThi()}).";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                // Nếu quay về /Admin mà không phải QUAN_TRI thì điều hướng về trang phù hợp với vai trò
                if (returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && nv.VaiTro != VaiTro.QUAN_TRI)
                {
                    return LocalRedirect(LayTrangMacDinh(nv.VaiTro));
                }
                return LocalRedirect(returnUrl);
            }

            return LocalRedirect(LayTrangMacDinh(nv.VaiTro));
        }

        TempData["Error"] = "Tài khoản không hợp lệ hoặc đã bị khóa.";
        return Redirect("/TongQuan");
    }

    public async Task<IActionResult> OnPostAsync(int id, string? returnUrl) => await OnGetAsync(id, returnUrl);

    static string LayTrangMacDinh(VaiTro vaiTro) => vaiTro switch
    {
        VaiTro.QUAN_TRI => "/Admin/TaiKhoan",
        VaiTro.BAN_HANG => "/BanHang/DonHang",
        VaiTro.SAN_XUAT => "/Bep/ThietKe",
        VaiTro.KHO => "/Kho/TonKho",
        _ => "/TongQuan"
    };
}
