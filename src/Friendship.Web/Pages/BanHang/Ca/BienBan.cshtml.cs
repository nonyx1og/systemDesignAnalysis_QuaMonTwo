using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang.Ca;

public class BienBanModel(CaService ca) : PageModel
{
    public TongHopCa Tong { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try { Tong = await ca.TongHopAsync(id); }
        catch (NghiepVuException) { return NotFound(); }
        return Tong.Ca.NhanVienId == User.NhanVienId() ? Page() : NotFound();     // chỉ người phụ trách ca (BR25)
    }
}
