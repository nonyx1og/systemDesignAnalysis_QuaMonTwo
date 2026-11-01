using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Bep;

public class IndexModel(BepService bep, TimeProvider clock, ThietKeService thietKe, AnhService anh) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? Ngay { get; set; }
    [BindProperty(SupportsGet = true)] public string? TrangThai { get; set; }
    public List<DonBep> Dons { get; set; } = [];
    public DateTime BayGio => clock.GetLocalNow().DateTime;

    public async Task OnGetAsync() =>
        Dons = TrangThai == "CHO_GIAO" ? await bep.DonChoGiaoAsync(Ngay) : await bep.DonCanLamAsync(Ngay);

    /// <summary>Phần danh sách để trang tự làm mới bằng fetch (UC10 bước 3).</summary>
    public async Task<IActionResult> OnGetDanhSachAsync()
    {
        Dons = TrangThai == "CHO_GIAO" ? await bep.DonChoGiaoAsync(Ngay) : await bep.DonCanLamAsync(Ngay);
        return Partial("_DanhSach", this);
    }

    public Task<IActionResult> OnPostBatDauAsync(int id) => DoiAsync(() => bep.BatDauLamAsync(id), "Đã bắt đầu làm");
    public Task<IActionResult> OnPostChoGiaoAsync(int id) => DoiAsync(() => bep.ChoGiaoAsync(id), "Đã chuyển sang chờ giao");

    public async Task<IActionResult> OnPostTaiAnhThanhPhamAsync(int donHangId, IFormFile? anhFile)
    {
        if (anhFile == null || anhFile.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file ảnh thành phẩm.";
            return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
        }

        try
        {
            using var stream = anhFile.OpenReadStream();
            var tenFile = await anh.LuuAsync(stream);
            await thietKe.LuuAnhThanhPhamAsync(donHangId, tenFile);
            TempData["Message"] = "Đã lưu ảnh thành phẩm thành công.";
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
        }
        return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
    }

    async Task<IActionResult> DoiAsync(Func<Task<DonHang>> lam, string thongBao)
    {
        try
        {
            var d = await lam();
            TempData["Message"] = $"{thongBao}: {d.Ma}.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
    }
}
