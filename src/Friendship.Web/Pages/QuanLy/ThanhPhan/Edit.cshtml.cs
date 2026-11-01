using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanPhamEntity = Friendship.Domain.Entities.SanPham;

namespace Friendship.Web.Pages.QuanLy.ThanhPhan;

public class EditModel(
    ThanhPhanService thanhPhan,
    NguyenLieuService nguyenLieu,
    SanPhamService sanPham,
    IWebHostEnvironment env) : PageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool IsNew => Id is null;
    public List<NguyenLieuTon> NguyenLieus { get; set; } = [];
    public List<SanPhamEntity> SanPhams { get; set; } = [];
    public List<string> Icons { get; set; } = [];
    public GiaVonUocTinh? GiaVon { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Chọn nhóm thành phần")]
        public NhomThanhPhan Nhom { get; set; } = NhomThanhPhan.TOPPING;

        [Required(ErrorMessage = "Nhập tên thành phần")]
        public string Ten { get; set; } = "";

        public string? AnhMinhHoa { get; set; }

        [Range(0, 999999999, ErrorMessage = "Giá bán không được âm")]
        public decimal GiaBan { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int SoLuongToiDa { get; set; } = 1;

        public int? SanPhamNenId { get; set; }

        public List<BomRow> Bom { get; set; } = [];
    }

    public class BomRow
    {
        public int NguyenLieuId { get; set; }
        public decimal? DinhMuc { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadRefsAsync();
        if (IsNew)
        {
            Input.Bom.Add(new BomRow());
            return Page();
        }
        var tp = await thanhPhan.LayAsync(Id!.Value);
        if (tp is null) return NotFound();
        Input = new InputModel
        {
            Nhom = tp.Nhom,
            Ten = tp.Ten,
            AnhMinhHoa = tp.AnhMinhHoa,
            GiaBan = tp.GiaBan,
            SoLuongToiDa = tp.SoLuongToiDa,
            SanPhamNenId = tp.SanPhamNenId,
            Bom = tp.DinhLuongs.OrderBy(d => d.NguyenLieu.TenGoi)
                .Select(d => new BomRow { NguyenLieuId = d.NguyenLieuId, DinhMuc = d.DinhMuc }).ToList()
        };
        if (Input.Bom.Count == 0) Input.Bom.Add(new BomRow());
        GiaVon = await thanhPhan.GiaVonAsync(tp.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadRefsAsync();
        var dongLoi = Input.Bom.Select((b, i) => (b, i)).Where(x => x.b.NguyenLieuId == 0 && (x.b.DinhMuc ?? 0) != 0)
            .Select(x => x.i + 1).ToList();
        if (dongLoi.Count > 0)
        {
            ModelState.AddModelError("", $"Dòng định lượng {string.Join(", ", dongLoi)}: chọn nguyên liệu.");
            return Page();
        }
        Input.Bom = Input.Bom.Where(b => b.NguyenLieuId != 0).ToList();
        if (!string.IsNullOrEmpty(Input.AnhMinhHoa) && !Icons.Contains(Input.AnhMinhHoa))
            ModelState.AddModelError("Input.AnhMinhHoa", "Biểu tượng không hợp lệ, chọn trong danh sách.");
        if (!ModelState.IsValid) return Page();

        try
        {
            var tp = await thanhPhan.LuuAsync(new ThanhPhanInput(
                Id,
                Input.Nhom,
                Input.Ten,
                Input.AnhMinhHoa,
                Input.GiaBan,
                Input.SoLuongToiDa,
                Input.Nhom is NhomThanhPhan.COT or NhomThanhPhan.VIEN or NhomThanhPhan.TANG ? Input.SanPhamNenId : null,
                Input.Bom.Select(b => new DinhLuongInput(b.NguyenLieuId, b.DinhMuc ?? 0)).ToList()
            ));
            var gv = await thanhPhan.GiaVonAsync(tp.Id);
            TempData["Message"] = $"Đã lưu thành phần. Giá vốn ước tính {Fmt.Tien(gv.GiaVon)}.";
            return RedirectToPage("Edit", new { id = tp.Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            if (Input.Bom.Count == 0) Input.Bom.Add(new BomRow());
            return Page();
        }
    }

    async Task LoadRefsAsync()
    {
        NguyenLieus = await nguyenLieu.TimAsync(null);
        SanPhams = await sanPham.TimAsync(null, dangKinhDoanh: true);
        if (!string.IsNullOrEmpty(env.WebRootPath))
        {
            var folder = Path.Combine(env.WebRootPath, "img", "thanh-phan");
            if (Directory.Exists(folder))
            {
                Icons = Directory.GetFiles(folder)
                    .Select(Path.GetFileName)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .Select(f => f!)
                    .OrderBy(x => x)
                    .ToList();
                return;
            }
        }
        Icons = ["dau.svg", "cherry.svg", "socola.svg", "macaron.svg", "nen.svg", "in-hinh.svg"];
    }
}
