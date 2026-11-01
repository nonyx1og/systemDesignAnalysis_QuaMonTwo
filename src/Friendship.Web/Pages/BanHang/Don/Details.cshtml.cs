using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.BanHang.Don;

public class DetailsModel(DonDatService donDat, BanLeService banLe, CaService ca, TimeProvider clock) : PageModel
{
    public DonHang Don { get; set; } = null!;
    public bool CoCa { get; set; }
    [BindProperty] public decimal? TienCoc { get; set; }
    [BindProperty] public PhuongThucTT PhuongThuc { get; set; }
    [BindProperty] public decimal? TienKhachDua { get; set; }
    [BindProperty] public KieuHuy Kieu { get; set; }
    [BindProperty] public bool XacNhanDangLam { get; set; }
    [BindProperty] public string? LyDoTra { get; set; }

    DateTime BayGio => clock.GetLocalNow().DateTime;
    public bool DatTruoc => Don.LoaiDon == LoaiDon.DAT_TRUOC;
    public bool ChoPhepTraHang => Don.LoaiDon == LoaiDon.BAN_LE && Don.TrangThai == TrangThaiDon.DA_THANH_TOAN;
    public bool ChoPhepHuy => DatTruoc
        && (Don.TrangThai is TrangThaiDon.CHO_COC or TrangThaiDon.DA_COC or TrangThaiDon.DANG_LAM or TrangThaiDon.CHO_GIAO);
    public bool ChoPhepBungCoc => Don.TrangThai == TrangThaiDon.CHO_GIAO && HuyDonRule.BungCoc(BayGio, Don.NgayGioLay!.Value);

    /// <summary>Mô tả trước hậu quả tiền cọc khi hủy (BR23), khớp luật service dùng.</summary>
    public string ChinhSachHuy
    {
        get
        {
            var coc = Don.ThanhToans.Where(t => t.Loai == LoaiThanhToan.COC).Sum(t => t.SoTien);
            if (coc <= 0) return "Đơn chưa có tiền cọc nên không có khoản hoàn.";
            return HuyDonRule.HoanCoc(BayGio, Don.NgayGioLay!.Value)
                ? $"Hủy trước giờ hẹn từ 24 giờ: hoàn 100% tiền cọc ({Fmt.Tien(coc)})."
                : $"Hủy muộn hơn 24 giờ trước giờ hẹn: không hoàn cọc ({Fmt.Tien(coc)}).";
        }
    }

    public async Task<IActionResult> OnGetAsync(int id) => await NapAsync(id) ? Page() : NotFound();

    public Task<IActionResult> OnPostCocAsync(int id) => ThucHienAsync(id, async () =>
    {
        await donDat.GhiNhanCocAsync(id, User.NhanVienId(), TienCoc ?? 0, PhuongThuc, TienKhachDua);
        return "Đã ghi nhận cọc. Đơn đã xuống bếp.";
    });

    public Task<IActionResult> OnPostGiaoAsync(int id) => ThucHienAsync(id, async () =>
    {
        var thoi = await donDat.GiaoAsync(id, User.NhanVienId(), PhuongThuc, TienKhachDua);
        return "Đã giao bánh và tất toán." + (thoi > 0 ? $" Tiền thối {Fmt.Tien(thoi)}." : "");
    });

    public Task<IActionResult> OnPostTraHangAsync(int id) => ThucHienAsync(id, async () =>
    {
        await banLe.TraHangAsync(id, User.NhanVienId(), LyDoTra ?? "");
        return "Đã trả hàng và hoàn tiền thành công.";
    });

    public Task<IActionResult> OnPostHuyAsync(int id) => ThucHienAsync(id, async () =>
    {
        var d = await donDat.HuyAsync(id, User.NhanVienId(), Kieu, XacNhanDangLam);
        return d.TrangThai == TrangThaiDon.BUNG_COC
            ? "Đã chuyển đơn sang bùng cọc, phiếu hủy nháp đã được tạo để gửi duyệt."
            : "Đã hủy đơn.";
    });

    async Task<IActionResult> ThucHienAsync(int id, Func<Task<string>> lam)
    {
        try
        {
            TempData["Message"] = await lam();
            return RedirectToPage(new { id });
        }
        catch (NghiepVuException e)
        {
            ModelState.AddModelError("", e.Message);
            return await NapAsync(id) ? Page() : NotFound();
        }
    }

    async Task<bool> NapAsync(int id)
    {
        var d = await donDat.LayAsync(id);
        if (d is null) return false;
        Don = d;
        CoCa = await ca.DangMoAsync(User.NhanVienId()) is not null;
        return true;
    }
}
