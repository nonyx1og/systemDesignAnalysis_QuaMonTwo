using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.QuanLy.SanPham;

public class EditModel(SanPhamService sanPham, NguyenLieuService nguyenLieu) : PageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool IsNew => Id is null;
    public List<NguyenLieuTon> NguyenLieus { get; set; } = [];
    public GiaVonUocTinh? GiaVon { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Nhập tên bánh")] public string TenBanh { get; set; } = "";
        [Required(ErrorMessage = "Nhập kích thước")] public string KichThuoc { get; set; } = "";
        [Range(1, 999999999, ErrorMessage = "Đơn giá phải lớn hơn 0")] public decimal DonGia { get; set; }
        [Range(1, 100000, ErrorMessage = "Hạn sử dụng (giờ) phải lớn hơn 0")] public int HanSuDungGio { get; set; }
        public List<BomRow> Bom { get; set; } = [];
    }

    public class BomRow
    {
        public int NguyenLieuId { get; set; }
        public decimal? DinhMuc { get; set; }   // nullable: dòng trống gửi chuỗi rỗng không được gây lỗi bind
    }

    public async Task<IActionResult> OnGetAsync()
    {
        NguyenLieus = await nguyenLieu.TimAsync(null);
        if (IsNew) { Input.Bom.Add(new BomRow()); return Page(); }
        var sp = await sanPham.LayAsync(Id!.Value);
        if (sp is null) return NotFound();
        Input = new InputModel
        {
            TenBanh = sp.TenBanh, KichThuoc = sp.KichThuoc, DonGia = sp.DonGia, HanSuDungGio = sp.HanSuDungGio,
            Bom = sp.DinhLuongs.OrderBy(d => d.NguyenLieu.TenGoi)
                .Select(d => new BomRow { NguyenLieuId = d.NguyenLieuId, DinhMuc = d.DinhMuc }).ToList()
        };
        GiaVon = await sanPham.GiaVonAsync(sp.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        NguyenLieus = await nguyenLieu.TimAsync(null);
        // Bỏ dòng trống hoàn toàn; dòng đã nhập định mức mà chưa chọn nguyên liệu là lỗi, không bỏ lặng lẽ.
        var dongLoi = Input.Bom.Select((b, i) => (b, i)).Where(x => x.b.NguyenLieuId == 0 && (x.b.DinhMuc ?? 0) != 0)
            .Select(x => x.i + 1).ToList();
        if (dongLoi.Count > 0)
        {
            ModelState.AddModelError("", $"Dòng công thức {string.Join(", ", dongLoi)}: chọn nguyên liệu.");
            return Page();
        }
        Input.Bom = Input.Bom.Where(b => b.NguyenLieuId != 0).ToList();
        if (!ModelState.IsValid) return Page();
        try
        {
            var sp = await sanPham.LuuAsync(new SanPhamInput(Id, Input.TenBanh, Input.KichThuoc, Input.DonGia,
                Input.HanSuDungGio, Input.Bom.Select(b => new DinhLuongInput(b.NguyenLieuId, b.DinhMuc ?? 0)).ToList()));
            var gv = await sanPham.GiaVonAsync(sp.Id);
            TempData["Message"] = $"Đã lưu sản phẩm. Giá vốn ước tính {Fmt.Tien(gv.GiaVon)}, biên lợi nhuận ước tính {Fmt.So(Math.Round((sp.DonGia - gv.GiaVon) / sp.DonGia * 100, 1))}%.";
            return RedirectToPage("Edit", new { id = sp.Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            if (Input.Bom.Count == 0) Input.Bom.Add(new BomRow());
            return Page();
        }
    }
}
