using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Bep.Ca;

public class IndexModel(CaNhanVienService caService, TimeProvider clock) : PageModel
{
    public CaNhanVien? CaHienTai { get; set; }
    public TongHopDinhLuongCa? TongHop { get; set; }
    public List<CaNhanVien> LichSuCa { get; set; } = [];

    [BindProperty] public BuoiCa BuoiCa { get; set; } = BuoiCa.CA_SANG;
    [BindProperty] public string? GhiChuMoCa { get; set; }
    [BindProperty] public string? GhiChuChotCa { get; set; }

    public async Task OnGetAsync()
    {
        var bay = clock.GetLocalNow().DateTime;
        BuoiCa = bay.Hour < 14 ? BuoiCa.CA_SANG : BuoiCa.CA_CHIEU;
        await NapDuLieuAsync();
    }

    public async Task<IActionResult> OnPostMoCaAsync()
    {
        try
        {
            var ca = await caService.MoCaAsync(User.NhanVienId(), LoaiCa.BEP_BANH, BuoiCa, GhiChuMoCa);
            TempData["Message"] = $"Đã mở {ca.BuoiCa.TenNgan()} thành công ({ca.Ma}). Các sản phẩm làm xong sẽ được tự động cộng vào định lượng ca này.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            await NapDuLieuAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChotCaAsync()
    {
        try
        {
            var ca = await caService.CanDangMoAsync(User.NhanVienId(), LoaiCa.BEP_BANH);
            var phieu = await caService.ChotCaVaXuatPhieuDinhLuongAsync(ca.Id, User.NhanVienId(), GhiChuChotCa);
            TempData["Message"] = $"Đã chốt ca làm việc và xuất phiếu định lượng {phieu.Ma} thành công. Phiếu đã được chuyển sang bộ phận Kho để đối chiếu.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
            await NapDuLieuAsync();
            return Page();
        }
    }

    async Task NapDuLieuAsync()
    {
        var nvId = User.NhanVienId();
        CaHienTai = await caService.DangMoAsync(nvId, LoaiCa.BEP_BANH);
        if (CaHienTai != null)
        {
            TongHop = await caService.TongHopCaAsync(CaHienTai.Id);
        }
        LichSuCa = await caService.LayLichSuCaAsync(nvId, LoaiCa.BEP_BANH);
    }
}
