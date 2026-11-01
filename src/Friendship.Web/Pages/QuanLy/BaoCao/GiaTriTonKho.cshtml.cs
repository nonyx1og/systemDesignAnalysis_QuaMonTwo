using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.BaoCao;

public class GiaTriTonKhoModel(BaoCaoService baoCao) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? TuKhoa { get; set; }
    public BaoCaoGiaTriTonKho? Bao { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Bao = await baoCao.GiaTriTonKhoAsync(TuKhoa);
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
        }
    }
}
