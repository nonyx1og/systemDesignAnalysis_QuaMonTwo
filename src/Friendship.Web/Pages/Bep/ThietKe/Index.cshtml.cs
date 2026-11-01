using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Bep.ThietKe;

[Authorize(Roles = $"{nameof(VaiTro.SAN_XUAT)},{nameof(VaiTro.QUAN_LY)},{nameof(VaiTro.QUAN_TRI)}")]
public class IndexModel(ThietKeService thietKeService) : PageModel
{
    public List<ThietKeBanh> Items { get; set; } = [];

    public async Task OnGetAsync()
    {
        Items = await thietKeService.ChoDuyetAsync();
    }
}
