using System.ComponentModel.DataAnnotations;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

public class DatNhanhModel(FriendshipDb db) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Vui lòng chọn loại bánh")]
    public int SanPhamId { get; set; }

    [BindProperty, Range(1, 20, ErrorMessage = "Số lượng từ 1 đến 20 chiếc")]
    public int SoLuong { get; set; } = 1;

    [BindProperty, MaxLength(200, ErrorMessage = "Lời chúc tối đa 200 ký tự")]
    public string? LoiChuc { get; set; }

    [BindProperty, Required(ErrorMessage = "Vui lòng chọn thời gian giao hoặc nhận bánh")]
    public DateTime NgayGioLay { get; set; } = DateTime.Now.AddHours(4);

    [BindProperty]
    public string HinhThucNhan { get; set; } = "GiaoHang"; // GiaoHang hoặc TaiTiem

    [BindProperty]
    public string? DiaChiGiao { get; set; }

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
    public string HoTen { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng số 0")]
    public string SoDienThoai { get; set; } = "";

    [BindProperty]
    public string? GhiChu { get; set; }

    public record BanhOptionDto(int Id, string Ten, string KichThuoc, decimal DonGia, bool LaBanhKem);
    public List<BanhOptionDto> DanhSachBanh { get; set; } = [];
    public List<SelectListItem> BanhOptions { get; set; } = [];
    public SanPham? SanPhamDaChon { get; set; }

    public async Task OnGetAsync(int? sanPhamId)
    {
        await LoadBanhOptionsAsync();

        if (sanPhamId.HasValue)
        {
            SanPhamId = sanPhamId.Value;
            SanPhamDaChon = await db.SanPhams.FindAsync(sanPhamId.Value);
        }

        var khach = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);
        if (khach is not null)
        {
            HoTen = khach.TenKhachHang;
            SoDienThoai = khach.SoDienThoai;
            DiaChiGiao = khach.DiaChi ?? "";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadBanhOptionsAsync();

        var sp = await db.SanPhams.FindAsync(SanPhamId);
        if (sp is null || !sp.DangKinhDoanh)
        {
            ModelState.AddModelError(nameof(SanPhamId), "Bánh bạn chọn hiện không kinh doanh.");
            return Page();
        }
        SanPhamDaChon = sp;

        if (!string.IsNullOrWhiteSpace(LoiChuc) && !sp.LaBanhKem)
        {
            ModelState.AddModelError(nameof(LoiChuc), $"Chỉ bánh kem mới được ghi lời chúc viết lên bánh (sản phẩm '{sp.TenBanh}' không hỗ trợ).");
            return Page();
        }
        if (!sp.LaBanhKem)
        {
            LoiChuc = null;
        }

        if (NgayGioLay <= DateTime.Now.AddHours(1))
        {
            ModelState.AddModelError(nameof(NgayGioLay), "Thời gian giao/nhận bánh phải cách thời điểm hiện tại ít nhất 1 giờ để bếp chuẩn bị.");
            return Page();
        }

        if (HinhThucNhan == "GiaoHang" && string.IsNullOrWhiteSpace(DiaChiGiao))
        {
            ModelState.AddModelError(nameof(DiaChiGiao), "Vui lòng nhập địa chỉ nhận bánh khi chọn giao hàng tận nơi.");
            return Page();
        }

        if (!ModelState.IsValid) return Page();

        // Tìm hoặc tạo khách hàng
        var sdt = SoDienThoai.Trim();
        var kh = await db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt);
        if (kh is null)
        {
            kh = new KhachHang
            {
                TenKhachHang = HoTen.Trim(),
                SoDienThoai = sdt,
                DiaChi = DiaChiGiao?.Trim(),
                DiemTichLuy = 0,
                NgayTao = DateTime.Now
            };
            db.KhachHangs.Add(kh);
            await db.SaveChangesAsync();
        }
        else if (string.IsNullOrEmpty(kh.DiaChi) && !string.IsNullOrEmpty(DiaChiGiao))
        {
            kh.DiaChi = DiaChiGiao.Trim();
        }

        var thanhTien = sp.DonGia * SoLuong;
        var adminId = await db.NhanViens.Where(n => n.DangHoatDong).Select(n => n.Id).FirstOrDefaultAsync();
        if (adminId == 0) adminId = 1;

        var fullGhiChu = HinhThucNhan == "GiaoHang"
            ? $"[Giao hàng tận nơi: {DiaChiGiao}] {GhiChu}".Trim()
            : $"[Khách lấy tại tiệm] {GhiChu}".Trim();

        var don = new DonHang
        {
            LoaiDon = LoaiDon.DAT_TRUOC,
            NgayLap = DateTime.Now,
            NgayGioLay = NgayGioLay,
            TongTienHang = thanhTien,
            TienGiamGia = 0,
            DiemSuDung = 0,
            TongThanhToan = thanhTien,
            TrangThai = TrangThaiDon.CHO_COC,
            NhanVienId = adminId,
            KhachHangId = kh.Id,
            LyDoTra = fullGhiChu,
            ChiTiets =
            [
                new ChiTietDonHang
                {
                    SanPhamId = sp.Id,
                    SoLuong = SoLuong,
                    DonGia = sp.DonGia,
                    TienGiam = 0,
                    ThanhTien = thanhTien,
                    LoiChuc = string.IsNullOrWhiteSpace(LoiChuc) ? null : LoiChuc.Trim()
                }
            ]
        };

        db.DonHangs.Add(don);
        await db.SaveChangesAsync();

        TempData["Message"] = $"Đặt bánh thành công! Mã đơn của bạn là {don.Ma}.";
        return RedirectToPage("/Dat/HoaDon", new { id = don.Id });
    }

    private async Task LoadBanhOptionsAsync()
    {
        var sps = await db.SanPhams
            .Where(s => s.DangKinhDoanh)
            .OrderBy(s => s.TenBanh)
            .ToListAsync();

        DanhSachBanh = sps.Select(s => new BanhOptionDto(s.Id, s.TenBanh, s.KichThuoc, s.DonGia, s.LaBanhKem)).ToList();

        BanhOptions = sps.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.TenBanh} ({s.KichThuoc}) – {s.DonGia:N0} ₫"
        }).ToList();
    }
}
