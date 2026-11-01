using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang;

/// <summary>Tra khách theo SĐT cho ô nhập số điện thoại (UC02 bước 2, UC03 bước 4). Luôn trả JSON, kể cả khi SĐT sai định dạng.</summary>
public class KhachModel(KhachHangService khach) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? sdt)
    {
        string? ten = null, loi = null;
        var diem = 0;
        try
        {
            var kh = await khach.TimAsync(sdt);
            if (kh is not null) { ten = kh.TenKhachHang; diem = kh.DiemTichLuy; }
        }
        catch (NghiepVuException e) { loi = e.Message; }
        return new JsonResult(new { ten, diem, loi });
    }
}
