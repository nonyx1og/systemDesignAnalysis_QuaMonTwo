using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.KhuyenMai;

public class EditModel(KhuyenMaiService khuyenMai, TimeProvider clock) : PageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool IsNew => Id is null;

    public class InputModel
    {
        [Required(ErrorMessage = "Nhập mã voucher")] public string Ma { get; set; } = "";
        [Required(ErrorMessage = "Nhập tên chương trình")] public string Ten { get; set; } = "";
        public LoaiGiam Loai { get; set; } = LoaiGiam.PHAN_TRAM;
        [Range(0.001, 999999999, ErrorMessage = "Giá trị giảm phải lớn hơn 0")] public decimal GiaTri { get; set; }
        [Range(0, 999999999, ErrorMessage = "Đơn tối thiểu không được âm")] public decimal DonToiThieu { get; set; }
        [Required(ErrorMessage = "Chọn ngày bắt đầu")] public DateOnly? NgayBatDau { get; set; }
        [Required(ErrorMessage = "Chọn ngày kết thúc")] public DateOnly? NgayKetThuc { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (IsNew)
        {
            var homNay = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
            Input.NgayBatDau = homNay;
            Input.NgayKetThuc = homNay.AddDays(30);
            return Page();
        }
        var km = await khuyenMai.LayAsync(Id!.Value);
        if (km is null) return NotFound();
        Input = new InputModel
        {
            Ma = km.MaKhuyenMai, Ten = km.TenChuongTrinh, Loai = km.LoaiGiam, GiaTri = km.GiaTriGiam,
            DonToiThieu = km.GiaTriDonToiThieu, NgayBatDau = km.NgayBatDau, NgayKetThuc = km.NgayKetThuc
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await khuyenMai.LuuAsync(new KhuyenMaiInput(Id, Input.Ma, Input.Ten, Input.Loai, Input.GiaTri,
                Input.DonToiThieu, Input.NgayBatDau!.Value, Input.NgayKetThuc!.Value));
            TempData["Message"] = "Đã lưu voucher.";
            return RedirectToPage("Index");
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
