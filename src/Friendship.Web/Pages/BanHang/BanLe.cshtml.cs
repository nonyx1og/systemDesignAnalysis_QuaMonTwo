using System.Security.Cryptography;
using System.Text;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang;

public class BanLeModel(BanLeService banLe, SanPhamService sanPham, CaService ca, NonceStore nonces) : PageModel
{
    [BindProperty] public List<Line> Lines { get; set; } = [];
    [BindProperty] public string? SoDienThoai { get; set; }
    [BindProperty] public string? TenKhach { get; set; }
    [BindProperty] public string? MaVoucher { get; set; }
    [BindProperty] public int? DiemSuDung { get; set; }
    [BindProperty] public PhuongThucTT PhuongThuc { get; set; }
    [BindProperty] public decimal? TienKhachDua { get; set; }
    [BindProperty] public List<ThanhToanLine> ThanhToanLines { get; set; } = [];
    [BindProperty] public string? PreviewToken { get; set; }
    [BindProperty] public string? Nonce { get; set; }
    public List<SanPham> SanPhams { get; set; } = [];
    public DonHang? Xem { get; set; }
    public bool CoCa { get; set; }

    // Nullable: dòng trống gửi chuỗi rỗng không được làm hỏng bind.
    public class Line
    {
        public int SanPhamId { get; set; }
        public int? SoLuong { get; set; }
    }

    public class ThanhToanLine
    {
        public PhuongThucTT PhuongThuc { get; set; } = PhuongThucTT.TIEN_MAT;
        public decimal? SoTien { get; set; }
        public decimal? TienKhachDua { get; set; }
    }

    public async Task OnGetAsync()
    {
        await NapAsync();      // NapAsync đã thêm sẵn một dòng trống
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        if (!await NapAsync()) return Page();
        await XemTruocAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostBanAsync()
    {
        if (!await NapAsync()) return Page();
        if (PreviewToken != DauVan())
        {
            ModelState.AddModelError("", "Đơn đã thay đổi sau khi xem trước. Hãy bấm \"Xem trước\" lại trước khi xác nhận.");
            return Page();
        }
        if (!nonces.Dung(Nonce, User.NhanVienId()))
        {
            ModelState.AddModelError("", "Phiên xác nhận đã được dùng hoặc đã hết hạn. Hãy xem trước lại (kiểm tra danh sách đơn nếu nghi đã bán).");
            return Page();
        }
        try
        {
            var kq = await banLe.BanAsync(Yeu());
            TempData["Message"] = $"Đã bán {Fmt.Tien(kq.Don.TongThanhToan)}." + (kq.TienThoi > 0 ? $" Tiền thối {Fmt.Tien(kq.TienThoi)}." : "");
            return RedirectToPage("/BanHang/Don/Details", new { id = kq.Don.Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            await XemTruocAsync(giuLoi: true);      // giữ bước xác nhận (mã mới) để sửa tiền khách đưa rồi bấm lại
            return Page();
        }
    }

    /// <summary>Nạp bánh đang kinh doanh và trạng thái ca; bỏ dòng trống hoàn toàn, nhưng dòng đã nhập số lượng mà chưa chọn bánh là lỗi.</summary>
    async Task<bool> NapAsync()
    {
        SanPhams = await sanPham.TimAsync(null, true);
        CoCa = await ca.DangMoAsync(User.NhanVienId()) is not null;
        var dongLoi = Lines.Select((l, i) => (l, i)).Where(x => x.l.SanPhamId == 0 && (x.l.SoLuong ?? 0) != 0)
            .Select(x => x.i + 1).ToList();
        if (dongLoi.Count > 0)
        {
            ModelState.AddModelError("", $"Dòng {string.Join(", ", dongLoi)}: chọn bánh.");
            return false;
        }
        Lines = Lines.Where(l => l.SanPhamId != 0).ToList();
        if (Lines.Count == 0) Lines.Add(new Line());
        return true;
    }

    async Task XemTruocAsync(bool giuLoi = false)
    {
        ModelState.Remove(nameof(Nonce));            // giá trị bind cũ không được che mã mới
        ModelState.Remove(nameof(PreviewToken));
        try
        {
            Xem = await banLe.XemTruocAsync(Yeu());
            PreviewToken = DauVan();
            Nonce = nonces.Cap(User.NhanVienId());
            if (Xem is not null && (ThanhToanLines.Count == 0 || ThanhToanLines.All(t => (t.SoTien ?? 0) <= 0)))
            {
                ThanhToanLines =
                [
                    new ThanhToanLine
                    {
                        PhuongThuc = PhuongThuc,
                        SoTien = Xem.TongThanhToan,
                        TienKhachDua = TienKhachDua ?? (PhuongThuc == PhuongThucTT.TIEN_MAT ? Xem.TongThanhToan : null)
                    }
                ];
            }
        }
        catch (NghiepVuException e) when (!giuLoi) { ModelState.AddModelError("", e.Message); }
        catch (NghiepVuException) { /* lỗi đã được hiện ở trên */ }
    }

    YeuCauBanLe Yeu()
    {
        var dongs = Lines.Where(l => l.SanPhamId != 0).Select(l => new DongBanLe(l.SanPhamId, l.SoLuong ?? 0)).ToList();
        var validTts = ThanhToanLines.Where(t => (t.SoTien ?? 0) > 0 || (t.TienKhachDua ?? 0) > 0).ToList();
        if (validTts.Count > 0)
        {
            var ttList = validTts.Select(t => new DongThanhToan(t.PhuongThuc, t.SoTien ?? 0, t.TienKhachDua)).ToList();
            return new YeuCauBanLe(User.NhanVienId(), SoDienThoai, TenKhach, dongs,
                MaVoucher, DiemSuDung ?? 0, validTts[0].PhuongThuc, validTts[0].TienKhachDua, ttList);
        }
        return new YeuCauBanLe(User.NhanVienId(), SoDienThoai, TenKhach, dongs,
            MaVoucher, DiemSuDung ?? 0, PhuongThuc, TienKhachDua);
    }


    /// <summary>Dấu vân tay các trường quyết định giá; tiền khách đưa và phương thức được sửa tự do sau khi xem.</summary>
    string DauVan() => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(';',
        Lines.Where(l => l.SanPhamId != 0).Select(l => $"{l.SanPhamId}:{l.SoLuong ?? 0}")
            .Concat([$"sdt={SoDienThoai?.Trim()}", $"ten={TenKhach?.Trim()}", $"vc={MaVoucher?.Trim().ToUpperInvariant()}", $"d={DiemSuDung ?? 0}"])))))[..16];
}
