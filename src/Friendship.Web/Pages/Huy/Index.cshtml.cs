using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Huy;

public class IndexModel(HuyHangService huy) : PageModel
{
    public List<PhieuXuatHuy> Items { get; set; } = [];

    public async Task OnGetAsync() => Items = await huy.TimAsync(null, User.NhanVienId());
}
