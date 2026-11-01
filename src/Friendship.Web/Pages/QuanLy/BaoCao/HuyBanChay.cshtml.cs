using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.BaoCao;

public class HuyBanChayModel(BaoCaoService baoCao, TimeProvider clock) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? Tu { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? Den { get; set; }
    [BindProperty(SupportsGet = true)] public int? Top { get; set; }
    [BindProperty(SupportsGet = true)] public LocHuy? Loc { get; set; }
    public BaoCaoHuyBanChay? Bao { get; set; }

    public async Task OnGetAsync()
    {
        var homNay = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        Tu ??= new DateOnly(homNay.Year, homNay.Month, 1);
        Den ??= homNay;
        Top ??= 5;
        Loc ??= LocHuy.TatCa;
        try { Bao = await baoCao.HuyVaBanChayAsync(Tu.Value, Den.Value, Top.Value, Loc.Value); }
        catch (NghiepVuException e) { ModelState.AddModelError("", e.Message); }
    }
}
