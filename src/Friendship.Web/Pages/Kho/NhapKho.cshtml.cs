using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho;

public class NhapKhoModel(KhoService kho, NguyenLieuService nguyenLieu) : PageModel
{
    [BindProperty] public List<Row> Rows { get; set; } = [];
    public List<NguyenLieuTon> NguyenLieus { get; set; } = [];

    // Nullable: dòng trống gửi chuỗi rỗng không được gây lỗi bind (xem ledger Task 3).
    public class Row
    {
        public int NguyenLieuId { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public DateOnly? HanSuDung { get; set; }
    }

    public async Task OnGetAsync()
    {
        NguyenLieus = await nguyenLieu.TimAsync(null);
        Rows.Add(new Row());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        NguyenLieus = await nguyenLieu.TimAsync(null);
        // Bỏ dòng trống hoàn toàn; dòng đã nhập số liệu mà chưa chọn nguyên liệu là lỗi, không bỏ lặng lẽ.
        var dongLoi = Rows.Select((r, i) => (r, i))
            .Where(x => x.r.NguyenLieuId == 0 && ((x.r.SoLuong ?? 0) != 0 || (x.r.DonGia ?? 0) != 0 || x.r.HanSuDung is not null))
            .Select(x => x.i + 1).ToList();
        if (dongLoi.Count > 0)
        {
            ModelState.AddModelError("", $"Dòng {string.Join(", ", dongLoi)}: chọn nguyên liệu.");
            return Page();
        }
        Rows = Rows.Where(r => r.NguyenLieuId != 0).ToList();
        try
        {
            var los = await kho.NhapKhoAsync(Rows
                .Select(r => new DongNhapKho(r.NguyenLieuId, r.SoLuong ?? 0, r.DonGia ?? 0, r.HanSuDung)).ToList());
            TempData["Message"] = $"Đã nhập kho {los.Count} lô nguyên liệu.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            if (Rows.Count == 0) Rows.Add(new Row());
            return Page();
        }
    }
}
