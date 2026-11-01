using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho.DoiChieu;

public class IndexModel(CaNhanVienService caService, KhoService khoService) : PageModel
{
    public List<PhieuDinhLuongCa> PhieuChoDoiChieu { get; set; } = [];
    public List<BienBanDoiChieuKho> DanhSachBienBan { get; set; } = [];

    public async Task OnGetAsync()
    {
        PhieuChoDoiChieu = await caService.LayPhieuChoDoiChieuAsync();
        DanhSachBienBan = await khoService.LayDanhSachBienBanDoiChieuAsync();
    }
}
