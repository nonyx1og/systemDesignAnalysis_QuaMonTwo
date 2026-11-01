using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Friendship.Web.Pages.Dat;

[EnableRateLimiting("khach")]
public class TimModel(ThietKeService thietKeService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Sdt { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? BonKyTuCuoi { get; set; }

    public List<ThietKeBanh> KetQua { get; set; } = [];
    public bool DaTim { get; set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Sdt) || !string.IsNullOrWhiteSpace(BonKyTuCuoi))
        {
            DaTim = true;
            KetQua = await thietKeService.TimAsync(Sdt ?? "", BonKyTuCuoi ?? "");
        }
    }
}
