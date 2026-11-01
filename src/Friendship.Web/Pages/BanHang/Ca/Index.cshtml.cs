using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang.Ca;

public class IndexModel(CaService ca) : PageModel
{
    // Nullable để ô trống không gây lỗi bind mà báo bằng thông báo nghiệp vụ.
    [BindProperty] public decimal? TienDauCa { get; set; }
    [BindProperty] public decimal? TienMatThucTe { get; set; }
    [BindProperty] public decimal? ChuyenKhoanThucTe { get; set; }
    [BindProperty] public string? GhiChu { get; set; }
    public CaThuNgan? Ca { get; set; }
    public TongHopCa? Tong { get; set; }
    public CaThuNgan? CaTruoc { get; set; }

    public Task OnGetAsync() => NapAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (TienDauCa is null) throw new NghiepVuException("Nhập tiền quỹ đầu ca (nhập 0 nếu không có).");
            await ca.MoCaAsync(User.NhanVienId(), TienDauCa.Value);
            TempData["Message"] = "Đã mở ca.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            await NapAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChotAsync()
    {
        try
        {
            if (TienMatThucTe is null || ChuyenKhoanThucTe is null)
                throw new NghiepVuException("Nhập tiền mặt đếm thực tế và tổng chuyển khoản theo ứng dụng ngân hàng (nhập 0 nếu không có).");
            var c = await ca.ChotCaAsync(User.NhanVienId(), TienMatThucTe.Value, ChuyenKhoanThucTe.Value, GhiChu);
            TempData["Message"] = "Đã chốt ca.";
            return RedirectToPage("BienBan", new { id = c.Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            await NapAsync();
            return Page();
        }
    }

    async Task NapAsync()
    {
        var id = User.NhanVienId();
        Ca = await ca.DangMoAsync(id);
        if (Ca is not null) Tong = await ca.TongHopAsync(Ca.Id);
        else CaTruoc = await ca.CaDaChotGanNhatAsync(id);
    }
}
