using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Bep.BaoLoi;

public class IndexModel(
    FriendshipDb db,
    CaNhanVienService caService,
    DinhLuongService dinhLuongService) : PageModel
{
    public CaNhanVien? CaHienTai { get; set; }
    public List<SanPham> SanPhams { get; set; } = [];
    public List<NguyenLieu> NguyenLieus { get; set; } = [];
    public List<PhieuBaoLoiMeBanh> DanhSachPhieu { get; set; } = [];

    [BindProperty] public int SanPhamId { get; set; }
    [BindProperty] public int SoLuongHong { get; set; } = 1;
    [BindProperty] public string LyDoLoi { get; set; } = "";
    [BindProperty] public List<DongNguyenLieuHong> ChiTiets { get; set; } = [];

    public class DongNguyenLieuHong
    {
        public int NguyenLieuId { get; set; }
        public decimal SoLuong { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? spId = null, int? slHong = null)
    {
        await NapDuLieuAsync();

        if (spId.HasValue && slHong.HasValue && slHong.Value > 0)
        {
            SanPhamId = spId.Value;
            SoLuongHong = slHong.Value;
            var goiY = await dinhLuongService.GoiYDinhLuongMeLoiAsync(SanPhamId, SoLuongHong);
            ChiTiets = goiY.Select(g => new DongNguyenLieuHong { NguyenLieuId = g.NguyenLieuId, SoLuong = g.SoLuong }).ToList();
        }

        if (ChiTiets.Count == 0)
        {
            ChiTiets = [
                new() { NguyenLieuId = NguyenLieus.FirstOrDefault()?.Id ?? 0, SoLuong = 0 },
                new() { NguyenLieuId = NguyenLieus.Skip(1).FirstOrDefault()?.Id ?? 0, SoLuong = 0 },
                new() { NguyenLieuId = NguyenLieus.Skip(2).FirstOrDefault()?.Id ?? 0, SoLuong = 0 }
            ];
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapDuLieuAsync();

        try
        {
            var nvId = User.NhanVienId();
            var ca = await caService.CanDangMoAsync(nvId, LoaiCa.BEP_BANH);

            var hopLe = ChiTiets
                .Where(c => c.NguyenLieuId > 0 && c.SoLuong > 0)
                .Select(c => new DongNguyenLieuBoSung(c.NguyenLieuId, c.SoLuong))
                .ToList();

            if (hopLe.Count == 0)
            {
                throw new NghiepVuException("Vui lòng nhập định lượng nguyên liệu bị hao phí (số lượng > 0).");
            }

            var phieu = await dinhLuongService.TaoPhieuBaoLoiAsync(
                ca.Id,
                nvId,
                SanPhamId > 0 ? SanPhamId : (int?)null,
                SoLuongHong,
                LyDoLoi,
                hopLe
            );

            TempData["Message"] = $"Đã lập phiếu báo lỗi mẻ bánh {phieu.Ma} gửi quản lý duyệt thành công. Sau khi quản lý duyệt, định lượng hao phí sẽ được tự động cộng vào ca này.";
            return RedirectToPage();
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }

    async Task NapDuLieuAsync()
    {
        var nvId = User.NhanVienId();
        CaHienTai = await caService.DangMoAsync(nvId, LoaiCa.BEP_BANH);
        SanPhams = await db.SanPhams.Where(s => s.DangKinhDoanh).OrderBy(s => s.TenBanh).ToListAsync();
        NguyenLieus = await db.NguyenLieus.OrderBy(n => n.TenGoi).ToListAsync();
        DanhSachPhieu = await dinhLuongService.LayDanhSachPhieuLoiAsync(caId: CaHienTai?.Id);
    }
}
