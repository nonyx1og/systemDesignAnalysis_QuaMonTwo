using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.BaoCao;

public class DoanhThuModel(BaoCaoService baoCao, TimeProvider clock) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? Tu { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? Den { get; set; }
    [BindProperty(SupportsGet = true)] public string? Tab { get; set; }
    public BaoCaoDoanhThu? Bao { get; set; }
    public BaoCaoTuyChinh? TuyChinh { get; set; }
    public List<CaThuNgan> Cas { get; set; } = [];
    public bool TabCa => Tab == "ca";

    public async Task OnGetAsync()
    {
        var homNay = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        Tu ??= new DateOnly(homNay.Year, homNay.Month, 1);       // mặc định: từ đầu tháng đến hôm nay
        Den ??= homNay;
        try
        {
            if (TabCa) Cas = await baoCao.CaTrongKyAsync(Tu.Value, Den.Value);
            else
            {
                Bao = await baoCao.DoanhThuAsync(Tu.Value, Den.Value);
                TuyChinh = await baoCao.TuyChinhAsync(Tu.Value, Den.Value);
            }
        }
        catch (NghiepVuException e) { ModelState.AddModelError("", e.Message); }
    }
}
