using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.NguyenLieu;

public class EditModel(NguyenLieuService nguyenLieu) : PageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool IsNew => Id is null;

    public class InputModel
    {
        [Required(ErrorMessage = "Nhập tên nguyên liệu")] public string TenGoi { get; set; } = "";
        [Required(ErrorMessage = "Nhập đơn vị tính")] public string DonViTinh { get; set; } = "";
        [Range(0, 999999999, ErrorMessage = "Mức cảnh báo phải từ 0 trở lên")] public decimal MucCanhBaoTon { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (IsNew) return Page();
        var nl = await nguyenLieu.LayAsync(Id!.Value);
        if (nl is null) return NotFound();
        Input = new InputModel { TenGoi = nl.TenGoi, DonViTinh = nl.DonViTinh, MucCanhBaoTon = nl.MucCanhBaoTon };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await nguyenLieu.LuuAsync(new NguyenLieuInput(Id, Input.TenGoi, Input.DonViTinh, Input.MucCanhBaoTon));
            TempData["Message"] = "Đã lưu nguyên liệu.";
            return RedirectToPage("Index");
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
