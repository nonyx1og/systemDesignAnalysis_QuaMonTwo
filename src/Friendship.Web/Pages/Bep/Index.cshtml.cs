using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.Bep;

public class IndexModel(
    BepService bep,
    TimeProvider clock,
    ThietKeService thietKe,
    AnhService anh,
    CaNhanVienService caService,
    DinhLuongService dinhLuong) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? Ngay { get; set; }
    [BindProperty(SupportsGet = true)] public string? TrangThai { get; set; }
    public List<DonBep> Dons { get; set; } = [];
    public DateTime BayGio => clock.GetLocalNow().DateTime;
    public CaNhanVien? CaHienTai { get; set; }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            CaHienTai = await caService.DangMoAsync(User.NhanVienId(), LoaiCa.BEP_BANH);
        }
        Dons = TrangThai == "CHO_GIAO" ? await bep.DonChoGiaoAsync(Ngay) : await bep.DonCanLamAsync(Ngay);
    }

    /// <summary>Phần danh sách để trang tự làm mới bằng fetch (UC10 bước 3).</summary>
    public async Task<IActionResult> OnGetDanhSachAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            CaHienTai = await caService.DangMoAsync(User.NhanVienId(), LoaiCa.BEP_BANH);
        }
        Dons = TrangThai == "CHO_GIAO" ? await bep.DonChoGiaoAsync(Ngay) : await bep.DonCanLamAsync(Ngay);
        return Partial("_DanhSach", this);
    }

    public Task<IActionResult> OnPostBatDauAsync(int id) => DoiAsync(() => bep.BatDauLamAsync(id), "Đã bắt đầu làm");

    public async Task<IActionResult> OnPostChoGiaoAsync(int id)
    {
        try
        {
            var nvId = User.NhanVienId();
            var ca = await caService.DangMoAsync(nvId, LoaiCa.BEP_BANH);
            if (ca is null)
            {
                TempData["Error"] = "Bạn chưa mở ca làm việc bếp. Vui lòng vào mục 'Ca & Định lượng bếp' để mở ca trước khi chuyển đơn sang Chờ giao.";
                return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
            }

            // Kiểm tra xem đơn có bánh nào chưa có định lượng BOM không
            var sanPhamChuaCoBOM = await dinhLuong.KiemTraSanPhamChuaCoBOMAsync(id);
            if (sanPhamChuaCoBOM.Count > 0)
            {
                // Yêu cầu bếp nhập định lượng trước
                TempData["Message"] = $"Đơn này có bánh '{sanPhamChuaCoBOM[0].TenBanh}' chưa có định lượng sẵn. Vui lòng nhập định lượng tiêu hao thực tế.";
                return RedirectToPage("/Bep/NhapDinhLuongDon", new { donId = id, sanPhamId = sanPhamChuaCoBOM[0].Id });
            }

            // Nếu đầy đủ BOM, tự động ghi nhận tiêu hao vào ca và chuyển trạng thái
            await dinhLuong.GhiNhanTieuHaoDonHangAsync(id, ca.Id);
            var d = await bep.ChoGiaoAsync(id);
            TempData["Message"] = $"Đã chuyển sang chờ giao và cộng định lượng vào ca {ca.Ma}: {d.Ma}.";
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
        }

        return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
    }

    public async Task<IActionResult> OnPostTaiAnhThanhPhamAsync(int donHangId, IFormFile? anhFile)
    {
        if (anhFile == null || anhFile.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file ảnh thành phẩm.";
            return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
        }

        try
        {
            using var stream = anhFile.OpenReadStream();
            var tenFile = await anh.LuuAsync(stream);
            await thietKe.LuuAnhThanhPhamAsync(donHangId, tenFile);
            TempData["Message"] = "Đã lưu ảnh thành phẩm thành công.";
        }
        catch (NghiepVuException e)
        {
            TempData["Error"] = e.Message;
        }
        return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
    }

    async Task<IActionResult> DoiAsync(Func<Task<DonHang>> lam, string thongBao)
    {
        try
        {
            var d = await lam();
            TempData["Message"] = $"{thongBao}: {d.Ma}.";
        }
        catch (NghiepVuException e) { TempData["Error"] = e.Message; }
        return RedirectToPage(new { ngay = Ngay?.ToString("yyyy-MM-dd"), trangThai = TrangThai });
    }
}
