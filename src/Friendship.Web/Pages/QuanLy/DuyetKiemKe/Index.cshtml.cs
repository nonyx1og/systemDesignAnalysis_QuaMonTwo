using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.DuyetKiemKe;

public class IndexModel(KiemKeService kiemKe) : PageModel
{
    public List<PhieuKiemKe> ChoDuyet { get; set; } = [];
    public List<PhieuKiemKe> DaXuLy { get; set; } = [];

    public async Task OnGetAsync()
    {
        ChoDuyet = await kiemKe.TimAsync(TrangThaiPhieu.CHO_DUYET);
        DaXuLy = (await kiemKe.TimAsync(null))
            .Where(p => p.TrangThai is TrangThaiPhieu.DA_DUYET or TrangThaiPhieu.TU_CHOI).Take(20).ToList();
    }
}
