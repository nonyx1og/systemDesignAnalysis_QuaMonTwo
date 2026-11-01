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
}
