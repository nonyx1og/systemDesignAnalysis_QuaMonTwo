using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Admin.TaiKhoan;

public class EditModel(NhanVienService nhanVien) : PageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public string? MatKhauTam { get; set; }
    public bool IsNew => Id is null;
    public bool DangHoatDong { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Nhập họ tên")] public string HoTen { get; set; } = "";
        public string? GioiTinh { get; set; }
        public DateOnly? NgaySinh { get; set; }
        [Required(ErrorMessage = "Nhập số điện thoại"), RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT gồm 10 số, bắt đầu bằng 0")]
        public string SoDienThoai { get; set; } = "";
        [EmailAddress(ErrorMessage = "Email không hợp lệ")] public string? Email { get; set; }
        [Required(ErrorMessage = "Nhập tên đăng nhập"), RegularExpression(@"^[A-Za-z0-9._]{3,30}$", ErrorMessage = "3–30 ký tự: chữ không dấu, số, dấu chấm, gạch dưới")]
        public string TenDangNhap { get; set; } = "";
        public VaiTro VaiTro { get; set; } = VaiTro.BAN_HANG;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (IsNew) return Page();
        var nv = await nhanVien.LayAsync(Id!.Value);
        if (nv is null) return NotFound();
        DangHoatDong = nv.DangHoatDong;
        Input = new InputModel
        {
            HoTen = nv.HoTen, GioiTinh = nv.GioiTinh, NgaySinh = nv.NgaySinh, SoDienThoai = nv.SoDienThoai,
            Email = nv.Email, TenDangNhap = nv.TenDangNhap, VaiTro = nv.VaiTro
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var i = new NhanVienInput(Input.HoTen, Input.GioiTinh, Input.NgaySinh, Input.SoDienThoai,
            Input.Email, Input.TenDangNhap, Input.VaiTro);
        try
        {
            if (IsNew)
            {
                var nv = await nhanVien.TaoAsync(i, MatKhauTam ?? "");
                TempData["Message"] = "Đã tạo tài khoản. Hãy báo mật khẩu tạm cho nhân viên.";
                return RedirectToPage(new { id = nv.Id });
            }
            await nhanVien.CapNhatAsync(Id!.Value, i, User.NhanVienId());
            TempData["Message"] = "Đã lưu thay đổi.";
            return RedirectToPage(new { id = Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostResetAsync(string? matKhauMoi)
    {
        try
        {
            await nhanVien.DatLaiMatKhauAsync(Id!.Value, matKhauMoi ?? "");
            if (Id == User.NhanVienId())
            {
                var nv = await nhanVien.LayAsync(Id.Value);
                if (nv is not null) await SignInHelper.SignInAsync(HttpContext, nv);
            }
            TempData["Message"] = "Đã đặt lại mật khẩu.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { id = Id });
    }
}
