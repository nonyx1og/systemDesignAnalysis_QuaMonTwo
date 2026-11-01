using System.Security.Cryptography;
using System.Text;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang;

public class DonDatModel(DonDatService donDat, SanPhamService sanPham, CaService ca, NonceStore nonces) : PageModel
{
    [BindProperty] public List<Line> Lines { get; set; } = [];
    [BindProperty] public string? SoDienThoai { get; set; }
    [BindProperty] public string? TenKhach { get; set; }
    [BindProperty] public DateTime? NgayGioLay { get; set; }
    [BindProperty] public string? MaVoucher { get; set; }
    [BindProperty] public int? DiemSuDung { get; set; }
    [BindProperty] public decimal? TienCoc { get; set; }
    [BindProperty] public PhuongThucTT PhuongThuc { get; set; }
    [BindProperty] public decimal? TienKhachDua { get; set; }
    [BindProperty] public string? PreviewToken { get; set; }
    [BindProperty] public string? Nonce { get; set; }
    public List<SanPham> SanPhams { get; set; } = [];
    public DonHang? Xem { get; set; }
    public bool CoCa { get; set; }

    public class Line
    {
        public int SanPhamId { get; set; }
        public int? SoLuong { get; set; }
        public string? LoiChuc { get; set; }
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

    public async Task<IActionResult> OnPostTaoAsync()
    {
        if (!await NapAsync()) return Page();
        if (PreviewToken != DauVan())
        {
            ModelState.AddModelError("", "Đơn đã thay đổi sau khi xem trước. Hãy bấm \"Xem trước\" lại trước khi xác nhận.");
            return Page();
        }
        if (!nonces.Dung(Nonce, User.NhanVienId()))
        {
            ModelState.AddModelError("", "Phiên xác nhận đã được dùng hoặc đã hết hạn. Hãy xem trước lại (kiểm tra danh sách đơn nếu nghi đã tạo).");
            return Page();
        }
        try
        {
            var d = await donDat.TaoAsync(Yeu());
            TempData["Message"] = d.TrangThai == TrangThaiDon.DA_COC
                ? $"Đã tạo {d.Ma} và nhận cọc {Fmt.Tien(d.DaThanhToan)}. Đơn đã xuống bếp."
                : $"Đã tạo {d.Ma}, đang chờ cọc (chưa xuống bếp).";
            return RedirectToPage("/BanHang/Don/Details", new { id = d.Id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            await XemTruocAsync(giuLoi: true);
            return Page();
        }
    }

    async Task<bool> NapAsync()
    {
        SanPhams = await sanPham.TimAsync(null, true);
        CoCa = await ca.DangMoAsync(User.NhanVienId()) is not null;
        var dongLoi = Lines.Select((l, i) => (l, i)).Where(x => x.l.SanPhamId == 0 && ((x.l.SoLuong ?? 0) != 0 || !string.IsNullOrWhiteSpace(x.l.LoiChuc)))
            .Select(x => x.i + 1).ToList();
        if (dongLoi.Count > 0)
        {
            ModelState.AddModelError("", $"Dòng {string.Join(", ", dongLoi)}: chọn bánh.");
            return false;
        }
        Lines = Lines.Where(l => l.SanPhamId != 0).ToList();
        if (Lines.Count == 0) Lines.Add(new Line { SoLuong = 1 });
        return true;
    }

    async Task XemTruocAsync(bool giuLoi = false)
    {
        ModelState.Remove(nameof(Nonce));
        ModelState.Remove(nameof(PreviewToken));
        try
        {
            if (NgayGioLay is null) throw new NghiepVuException("Nhập giờ hẹn lấy.");
            Xem = await donDat.XemTruocAsync(Yeu());
            PreviewToken = DauVan();
            Nonce = nonces.Cap(User.NhanVienId());
        }
        catch (NghiepVuException e) when (!giuLoi) { ModelState.AddModelError("", e.Message); }
        catch (NghiepVuException) { /* lỗi đã được hiện ở trên */ }
    }

    YeuCauDat Yeu() => new(User.NhanVienId(), SoDienThoai, TenKhach,
        Lines.Where(l => l.SanPhamId != 0).Select(l => new DongDat(l.SanPhamId, l.SoLuong ?? 0, l.LoiChuc)).ToList(),
        NgayGioLay ?? default, MaVoucher, DiemSuDung ?? 0, TienCoc, PhuongThuc, TienKhachDua);

    /// <summary>Dấu vân tay các trường quyết định giá và hẹn; tiền cọc, phương thức, tiền khách đưa được sửa sau khi xem.</summary>
    string DauVan() => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(';',
        Lines.Where(l => l.SanPhamId != 0).Select(l => $"{l.SanPhamId}:{l.SoLuong ?? 0}:{l.LoiChuc?.Trim()}")
            .Concat([$"sdt={SoDienThoai?.Trim()}", $"ten={TenKhach?.Trim()}", $"hen={NgayGioLay:yyyyMMddHHmm}",
                     $"vc={MaVoucher?.Trim().ToUpperInvariant()}", $"d={DiemSuDung ?? 0}"])))))[..16];
}
