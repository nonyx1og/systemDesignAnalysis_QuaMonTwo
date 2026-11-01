using System.ComponentModel.DataAnnotations;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

public class GopYModel(FriendshipDb db) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn")]
    public string HoTen { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập số điện thoại để tiệm phản hồi")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng số 0")]
    public string SoDienThoai { get; set; } = "";

    [BindProperty, EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; set; }

    [BindProperty, Range(1, 5, ErrorMessage = "Vui lòng chọn mức độ hài lòng từ 1 đến 5 sao")]
    public int DiemDanhGia { get; set; } = 5;

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập chủ đề góp ý")]
    public string TieuDe { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập nội dung góp ý chi tiết")]
    [MinLength(10, ErrorMessage = "Nội dung góp ý tối thiểu 10 ký tự")]
    public string NoiDung { get; set; } = "";

    public List<GopY> DanhSachGopY { get; set; } = [];

    public async Task OnGetAsync()
    {
        var khach = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);
        if (khach is not null)
        {
            HoTen = khach.TenKhachHang;
            SoDienThoai = khach.SoDienThoai;
            Email = khach.Email ?? "";
        }

        DanhSachGopY = await db.GopYs
            .Where(g => g.DiemDanhGia >= 4)
            .OrderByDescending(g => g.NgayGui)
            .Take(6)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            DanhSachGopY = await db.GopYs.Where(g => g.DiemDanhGia >= 4).OrderByDescending(g => g.NgayGui).Take(6).ToListAsync();
            return Page();
        }

        var khach = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);

        var gopY = new GopY
        {
            KhachHangId = khach?.Id,
            HoTen = HoTen.Trim(),
            SoDienThoai = SoDienThoai.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            DiemDanhGia = DiemDanhGia,
            TieuDe = TieuDe.Trim(),
            NoiDung = NoiDung.Trim(),
            NgayGui = DateTime.Now,
            TrangThai = "Moi"
        };

        db.GopYs.Add(gopY);
        await db.SaveChangesAsync();

        TempData["Message"] = "Cảm ơn bạn đã gửi góp ý quý báu! Tiệm sẽ ghi nhận và không ngừng nâng cao chất lượng phục vụ.";
        return RedirectToPage("/Dat/GopY");
    }
}
