using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.XaHang;

public class IndexModel(XaHangService xaHang) : PageModel
{
    [BindProperty] public List<int> Ids { get; set; } = [];
    [BindProperty] public decimal? PhanTram { get; set; }
    public List<LoXaHang> Los { get; set; } = [];

    public async Task OnGetAsync() => Los = await xaHang.DanhSachAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (PhanTram is null) throw new NghiepVuException("Nhập mức giảm (0 để bỏ xả hàng).");
            var n = await xaHang.ThietLapAsync(Ids, PhanTram.Value);
            TempData["Message"] = PhanTram == 0 ? $"Đã bỏ xả hàng cho {n} lô." : $"Đã đặt giảm {Fmt.So(PhanTram.Value)}% cho {n} lô. Quầy bán theo giá mới ngay.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            Los = await xaHang.DanhSachAsync();
            return Page();
        }
    }
}
