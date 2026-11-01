using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Kho;

public class XuatNguyenLieuModel(KhoService kho, SanPhamService sanPham, BepService bep, DonDatService donDat, ThanhPhanService thanhPhan) : PageModel
{
    [BindProperty] public List<Line> Lines { get; set; } = [];
    [BindProperty] public string? PreviewToken { get; set; }
    public List<SanPham> SanPhams { get; set; } = [];
    public KeHoachXuat? KeHoach { get; set; }
    public List<DonBep> DonCho { get; set; } = [];
    [BindProperty(SupportsGet = true)] public int? DonId { get; set; }

    // Nullable: dòng trống gửi chuỗi rỗng không được gây lỗi bind (xem ledger Task 3).
    public class Line
    {
        public int SanPhamId { get; set; }
        public int? ThanhPhanId { get; set; }
        public string? TenHienThi { get; set; }
        public int? SoLuongBanh { get; set; }
    }

    public async Task OnGetAsync()
    {
        SanPhams = await sanPham.TimAsync(null, true);
        DonCho = await bep.DonCanLamAsync();
        if (DonId is { } id)
        {
            try
            {
                var ycs = await donDat.YeuCauXuatTuDonAsync(id);
                var tpIds = ycs.Where(y => y.ThanhPhanId.HasValue).Select(y => y.ThanhPhanId!.Value).Distinct().ToList();
                var tps = new Dictionary<int, string>();
                foreach (var tid in tpIds)
                {
                    var tp = await thanhPhan.LayAsync(tid);
                    if (tp != null) tps[tid] = tp.Ten;
                }

                Lines = ycs.Select(y => new Line
                {
                    SanPhamId = y.SanPhamId,
                    ThanhPhanId = y.ThanhPhanId,
                    TenHienThi = y.ThanhPhanId.HasValue && tps.TryGetValue(y.ThanhPhanId.Value, out var ten) ? ten : null,
                    SoLuongBanh = y.SoLuongBanh
                }).ToList();
            }
            catch (NghiepVuException e) { ModelState.AddModelError("", e.Message); }
        }
        if (Lines.Count == 0) Lines.Add(new Line());
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        if (!await NapAsync()) return Page();
        try
        {
            KeHoach = await kho.LapKeHoachXuatAsync(YeuCau());
            PreviewToken = DauVan();
        }
        catch (NghiepVuException e) { ModelState.AddModelError("", e.Message); }
        return Page();
    }

    public async Task<IActionResult> OnPostXuatAsync()
    {
        if (!await NapAsync()) return Page();
        if (PreviewToken != DauVan())
        {
            ModelState.AddModelError("", "Danh sách sản phẩm đã thay đổi sau khi xem trước. Hãy bấm \"Xem nguyên liệu cần lấy\" để xem lại trước khi xác nhận.");
            return Page();
        }
        try
        {
            var nvId = User.Identity?.IsAuthenticated == true ? User.NhanVienId() : (int?)null;
            var kq = await kho.XuatAsync(YeuCau(), nvId);   // service tính lại ở thời điểm xác nhận và lưu phiếu xuất kho
            TempData["Message"] = "Đã xuất nguyên liệu. " + string.Join(" ", kq.CanhBaoTon);
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }

    /// <summary>Nạp danh sách sản phẩm; bỏ dòng trống hoàn toàn, nhưng dòng đã nhập số lượng mà chưa chọn sản phẩm là lỗi (không bỏ lặng lẽ).</summary>
    async Task<bool> NapAsync()
    {
        SanPhams = await sanPham.TimAsync(null, true);
        DonCho = await bep.DonCanLamAsync();
        var dongLoi = Lines.Select((l, i) => (l, i))
            .Where(x => x.l.SanPhamId == 0 && !x.l.ThanhPhanId.HasValue && (x.l.SoLuongBanh ?? 0) != 0)
            .Select(x => x.i + 1).ToList();
        if (dongLoi.Count > 0)
        {
            ModelState.AddModelError("", $"Dòng {string.Join(", ", dongLoi)}: chọn sản phẩm.");
            return false;
        }

        var tpIds = Lines.Where(l => l.ThanhPhanId.HasValue && string.IsNullOrEmpty(l.TenHienThi))
            .Select(l => l.ThanhPhanId!.Value).Distinct().ToList();
        foreach (var tid in tpIds)
        {
            var tp = await thanhPhan.LayAsync(tid);
            if (tp != null)
            {
                foreach (var l in Lines.Where(l => l.ThanhPhanId == tid && string.IsNullOrEmpty(l.TenHienThi)))
                    l.TenHienThi = tp.Ten;
            }
        }

        Lines = Lines.Where(l => l.SanPhamId != 0 || l.ThanhPhanId.HasValue).ToList();
        if (Lines.Count == 0) Lines.Add(new Line());
        return true;
    }

    /// <summary>Dấu vân tay của danh sách dòng đã xem trước; Xác nhận xuất chỉ hợp lệ khi các dòng không đổi kể từ lúc xem.</summary>
    string DauVan() => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
        string.Join(';', Lines.Where(l => l.SanPhamId != 0 || l.ThanhPhanId.HasValue)
            .Select(l => $"{l.SanPhamId}:{l.ThanhPhanId}:{l.SoLuongBanh ?? 0}")))))[..16];

    List<YeuCauXuat> YeuCau() => Lines.Where(l => l.SanPhamId != 0 || l.ThanhPhanId.HasValue)
        .Select(l => new YeuCauXuat(l.SanPhamId, l.SoLuongBanh ?? 0, l.ThanhPhanId)).ToList();
}
