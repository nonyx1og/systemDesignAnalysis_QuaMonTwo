using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Huy;

public class TaoModel(HuyHangService huy) : PageModel
{
    [BindProperty] public List<Line> Lines { get; set; } = [];
    // UC17 A1: lập từ cảnh báo (nl, banh) hoặc từ đơn bùng cọc (don) thì điền sẵn dòng đầu.
    [BindProperty(SupportsGet = true)] public int? Nl { get; set; }
    [BindProperty(SupportsGet = true)] public int? Banh { get; set; }
    [BindProperty(SupportsGet = true)] public int? Don { get; set; }
    public LuaChonHuy Chon { get; set; } = null!;

    public class Line
    {
        public string? Target { get; set; }
        public decimal? SoLuong { get; set; }
        public LyDoHuy? LyDo { get; set; }          // null khi giá trị gửi lên không phải lý do hợp lệ
    }

    public async Task OnGetAsync()
    {
        Chon = await huy.LuaChonAsync();
        if (Nl is { } nl) Lines.Add(new Line { Target = "NL:" + nl, LyDo = LyDoHuy.QUA_HAN });
        else if (Banh is { } b) Lines.Add(new Line { Target = "BANH:" + b, LyDo = LyDoHuy.QUA_HAN });
        else if (Don is { } d) Lines.Add(new Line { Target = "DON:" + d, LyDo = LyDoHuy.KHACH_BUNG_COC });
        else Lines.Add(new Line { LyDo = LyDoHuy.QUA_HAN });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Chon = await huy.LuaChonAsync();
        try
        {
            var dong = new List<DongHuyInput>();
            for (var i = 0; i < Lines.Count; i++)
            {
                var l = Lines[i];
                if (string.IsNullOrEmpty(l.Target) && l.SoLuong is null) continue;      // dòng trống hoàn toàn
                var t = ParseTarget(l.Target) ?? throw new NghiepVuException($"Dòng {i + 1}: chọn lô hoặc đơn cần hủy.");
                // Đơn bùng cọc chỉ có một lý do hợp lệ: không bắt người dùng chọn lại khi đổi dòng sang đơn.
                var lyDo = t.Loai == LoaiDoiTuongHuy.DON_BUNG_COC ? LyDoHuy.KHACH_BUNG_COC
                    : l.LyDo ?? throw new NghiepVuException($"Dòng {i + 1}: chọn lý do hủy.");
                dong.Add(new DongHuyInput(t.Loai, t.Id, l.SoLuong ?? 0, lyDo));
            }
            var p = await huy.LapAsync(User.NhanVienId(), dong);
            TempData["Message"] = $"Đã lập {p.Ma} (tổn thất {Fmt.Tien(HuyHangService.TongTonThat(p))}), chờ quản lý duyệt.";
            return RedirectToPage("Details", new { id = p.Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            if (Lines.Count == 0) Lines.Add(new Line { LyDo = LyDoHuy.QUA_HAN });
            return Page();
        }
    }

    /// <summary>"NL:5", "BANH:3", "DON:7" → (loại, id); chuỗi khác → null.</summary>
    public static (LoaiDoiTuongHuy Loai, int Id)? ParseTarget(string? s)
    {
        var p = (s ?? "").Split(':');
        if (p.Length != 2 || !int.TryParse(p[1], out var id)) return null;
        return p[0] switch
        {
            "NL" => (LoaiDoiTuongHuy.LO_NGUYEN_LIEU, id),
            "BANH" => (LoaiDoiTuongHuy.LO_BANH, id),
            "DON" => (LoaiDoiTuongHuy.DON_BUNG_COC, id),
            _ => null
        };
    }
}
