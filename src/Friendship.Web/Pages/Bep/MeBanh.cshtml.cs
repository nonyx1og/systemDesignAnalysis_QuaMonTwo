using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Bep;

public class MeBanhModel(LoBanhService loBanh, SanPhamService sanPham, TimeProvider clock) : PageModel
{
    [BindProperty] public int SanPhamId { get; set; }
    [BindProperty] public int? SoLuong { get; set; }
    [BindProperty] public DateTime? GioRaLo { get; set; }
    public List<SanPham> SanPhams { get; set; } = [];
    public DateTime BayGio => clock.GetLocalNow().DateTime;

    public async Task OnGetAsync() => SanPhams = await sanPham.TimAsync(null, true);

    public async Task<IActionResult> OnPostAsync()
    {
        SanPhams = await sanPham.TimAsync(null, true);
        try
        {
            if (SanPhamId == 0) throw new NghiepVuException("Chọn bánh vừa ra lò.");
            var lo = await loBanh.GhiNhanMeAsync(SanPhamId, SoLuong ?? 0, GioRaLo);
            var sp = SanPhams.FirstOrDefault(s => s.Id == lo.SanPhamId);
            TempData["Message"] = $"Đã ghi nhận {lo.SoLuongSanXuat} {sp?.TenBanh}, hạn sử dụng {Fmt.NgayGio(lo.HanSuDung)}. Quầy bán được ngay.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
