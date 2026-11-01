using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang.Don;

public class IndexModel(DonDatService donDat) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public TrangThaiDon? TrangThai { get; set; }
    [BindProperty(SupportsGet = true)] public LoaiDon? Loai { get; set; }
    public List<DonHang> Items { get; set; } = [];

    public async Task OnGetAsync()
    {
        try   // spec mục 9: quét bùng cọc mỗi lần mở danh sách đơn
        {
            var chuyen = await donDat.QuetBungCocAsync(User.NhanVienId());
            if (chuyen.Count > 0)
                TempData["Message"] = $"Đã chuyển {chuyen.Count} đơn quá hạn sang bùng cọc ({string.Join(", ", chuyen.Select(d => d.Ma))}); phiếu hủy nháp đã được tạo.";
        }
        catch (NghiepVuException) { }   // người khác vừa quét xong
        Items = await donDat.TimAsync(Q, TrangThai, Loai);
    }
}
