using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Admin.TaiKhoan;

public class IndexModel(NhanVienService nhanVien) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public VaiTro? Role { get; set; }
    public List<NhanVien> Items { get; set; } = [];

    public async Task OnGetAsync() => Items = await nhanVien.TimAsync(Q, Role);

    public async Task<IActionResult> OnPostToggleAsync(int id, bool active)
    {
        try
        {
            await nhanVien.DatTrangThaiAsync(id, active, User.NhanVienId());
            TempData["Message"] = active ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { Q, Role });
    }

    public async Task<IActionResult> OnPostDangNhapNhanhAsync(int id)
    {
        var nv = await nhanVien.LayAsync(id);
        if (nv is null || !nv.DangHoatDong)
        {
            TempData["Error"] = "Tài khoản không tồn tại hoặc đã bị khóa.";
            return RedirectToPage(new { Q, Role });
        }

        await SignInHelper.SignInAsync(HttpContext, nv);
        TempData["Message"] = $"Đã chuyển phiên làm việc sang: {nv.HoTen} ({nv.VaiTro.TenHienThi()}).";

        var targetUrl = nv.VaiTro switch
        {
            VaiTro.QUAN_TRI => "/Admin/TaiKhoan",
            VaiTro.BAN_HANG => "/BanHang/DonHang",
            VaiTro.SAN_XUAT => "/Bep/ThietKe",
            VaiTro.KHO => "/Kho/TonKho",
            _ => "/TongQuan"
        };

        return LocalRedirect(targetUrl);
    }
}
