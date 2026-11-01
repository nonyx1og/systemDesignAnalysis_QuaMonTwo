using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Bep;

public class MeBanhModel(
    LoBanhService loBanh,
    SanPhamService sanPham,
    TimeProvider clock,
    CaNhanVienService caService,
    DinhLuongService dinhLuong) : PageModel
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

            // Tự động ghi nhận định lượng mẻ bánh vào ca bếp nếu có ca đang mở
            if (User.Identity?.IsAuthenticated == true)
            {
                var ca = await caService.DangMoAsync(User.NhanVienId(), LoaiCa.BEP_BANH);
                if (ca != null)
                {
                    await dinhLuong.GhiNhanTieuHaoMeBanhAsync(ca.Id, SanPhamId, lo.SoLuongSanXuat);
                }
            }

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
