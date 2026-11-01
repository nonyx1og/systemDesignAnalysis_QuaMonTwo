using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Bep;

public class NhapDinhLuongDonModel(
    FriendshipDb db,
    CaNhanVienService caService,
    DinhLuongService dinhLuongService,
    BepService bepService) : PageModel
{
    [BindProperty(SupportsGet = true)] public int DonId { get; set; }
    [BindProperty(SupportsGet = true)] public int SanPhamId { get; set; }

    public DonHang? Don { get; set; }
    public SanPham? SanPham { get; set; }
    public ChiTietDonHang? ChiTiet { get; set; }
    public List<NguyenLieu> NguyenLieus { get; set; } = [];

    [BindProperty] public List<DongNhapNguyenLieu> DongNguyenLieus { get; set; } = [];
    [BindProperty] public bool LuuLamBOMMacDinh { get; set; } = true;

    public class DongNhapNguyenLieu
    {
        public int NguyenLieuId { get; set; }
        public decimal SoLuong { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var ok = await NapDuLieuAsync();
        if (!ok) return RedirectToPage("/Bep/Index");

        if (DongNguyenLieus.Count == 0)
        {
            // Mặc định tạo sẵn 3 dòng trống để bếp chọn nguyên liệu
            DongNguyenLieus = [
                new() { NguyenLieuId = NguyenLieus.FirstOrDefault()?.Id ?? 0, SoLuong = 0 },
                new() { NguyenLieuId = NguyenLieus.Skip(1).FirstOrDefault()?.Id ?? 0, SoLuong = 0 },
                new() { NguyenLieuId = NguyenLieus.Skip(2).FirstOrDefault()?.Id ?? 0, SoLuong = 0 }
            ];
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ok = await NapDuLieuAsync();
        if (!ok) return RedirectToPage("/Bep/Index");

        try
        {
            var ca = await caService.CanDangMoAsync(User.NhanVienId(), LoaiCa.BEP_BANH);

            var danhSach = DongNguyenLieus
                .Where(d => d.NguyenLieuId > 0 && d.SoLuong > 0)
                .Select(d => new DongNguyenLieuBoSung(d.NguyenLieuId, d.SoLuong))
                .ToList();

            if (danhSach.Count == 0)
            {
                throw new NghiepVuException("Vui lòng nhập ít nhất một nguyên liệu với số lượng > 0 cho sản phẩm này.");
            }

            // Ghi nhận định lượng bổ sung
            await dinhLuongService.NhapDinhLuongBoSungChoDonAsync(DonId, ca.Id, SanPhamId, danhSach, LuuLamBOMMacDinh);

            // Kiểm tra xem đơn này còn sản phẩm nào khác chưa có định lượng không
            var conThieu = await dinhLuongService.KiemTraSanPhamChuaCoBOMAsync(DonId);
            if (conThieu.Count > 0)
            {
                var tiepTheo = conThieu.First();
                TempData["Message"] = $"Đã lưu định lượng cho '{SanPham!.TenBanh}'. Vui lòng tiếp tục nhập định lượng cho '{tiepTheo.TenBanh}'.";
                return RedirectToPage(new { donId = DonId, sanPhamId = tiepTheo.Id });
            }

            // Nếu tất cả sản phẩm trong đơn đã có định lượng đầy đủ, tự động ghi nhận các sản phẩm còn lại và chuyển Chờ giao!
            await dinhLuongService.GhiNhanTieuHaoDonHangAsync(DonId, ca.Id);
            await bepService.ChoGiaoAsync(DonId);

            TempData["Message"] = $"Đã nhập định lượng thành công và chuyển đơn {Don!.Ma} sang trạng thái Chờ giao.";
            return RedirectToPage("/Bep/Index");
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }

    async Task<bool> NapDuLieuAsync()
    {
        Don = await db.DonHangs
            .Include(d => d.ChiTiets).ThenInclude(c => c.SanPham)
            .FirstOrDefaultAsync(d => d.Id == DonId);

        if (Don == null) return false;

        // Nếu chưa chỉ định SanPhamId, lấy sản phẩm đầu tiên thiếu BOM
        if (SanPhamId == 0)
        {
            var thieu = await dinhLuongService.KiemTraSanPhamChuaCoBOMAsync(DonId);
            if (thieu.Count > 0) SanPhamId = thieu[0].Id;
        }

        SanPham = await db.SanPhams.FindAsync(SanPhamId);
        ChiTiet = Don.ChiTiets.FirstOrDefault(c => c.SanPhamId == SanPhamId);
        NguyenLieus = await db.NguyenLieus.OrderBy(n => n.TenGoi).ToListAsync();

        return SanPham != null && ChiTiet != null;
    }
}
