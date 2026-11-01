using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Bep.ThietKe;

[Authorize(Roles = $"{nameof(VaiTro.SAN_XUAT)},{nameof(VaiTro.QUAN_LY)},{nameof(VaiTro.QUAN_TRI)}")]
public class ChiTietModel(
    FriendshipDb db,
    ThietKeService thietKeService,
    SanPhamService sanPhamService,
    ThanhPhanService thanhPhanService) : PageModel
{
    public ThietKeBanh ThietKe { get; set; } = null!;
    public List<string> CanhBaos { get; set; } = [];
    public decimal GiaVonDuKien { get; set; }

    public string MauKem { get; set; } = "#FFFDF5";
    public string TenMauKem { get; set; } = "Trắng sữa";
    public string FontChu { get; set; } = "-";
    public string MauChu { get; set; } = "-";
    public int SoNen { get; set; }
    public string TenCot { get; set; } = "Cốt tiêu chuẩn";
    public string TenVien { get; set; } = "Không viền";
    public bool CoVien { get; set; }
    public string CotFlavor { get; set; } = "vani";
    public int SoTang { get; set; } = 1;
    public string? TenTangPhu { get; set; }
    public List<(string Ten, int SoLuong)> Toppings { get; set; } = [];

    [BindProperty]
    public decimal PhuPhi { get; set; }

    [BindProperty]
    public string? LyDoPhuPhi { get; set; }

    [BindProperty]
    public string GhiChu { get; set; } = "";

    [BindProperty]
    public string LyDo { get; set; } = "";

    public static string LayTenMauKem(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return "Trắng sữa";
        return hex.ToUpperInvariant() switch
        {
            "#FFFDF5" => "Trắng sữa",
            "#FFF8DC" => "Kem vani",
            "#FFB6C1" => "Hồng phấn",
            "#B0E0E6" => "Xanh pastel",
            "#FFFACD" => "Vàng nhạt",
            "#5D4037" => "Socola",
            "#8FBC8F" => "Matcha",
            "#E6E6FA" => "Tím lavender",
            _ => hex
        };
    }

    async Task<bool> NapAsync(int id)
    {
        var tk = await db.ThietKeBanhs
            .Include(x => x.KhachHang)
            .Include(x => x.SanPhamNen)
            .Include(x => x.ThanhPhans).ThenInclude(t => t.ThanhPhan)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tk is null) return false;
        ThietKe = tk;
        CanhBaos = await thietKeService.CanhBaoNguyenLieuAsync(id);

        Toppings.Clear();
        foreach (var tp in tk.ThanhPhans)
        {
            if (tp.ThanhPhan == null) continue;
            if (tp.ThanhPhan.Nhom == NhomThanhPhan.COT)
            {
                TenCot = tp.ThanhPhan.Ten;
                var t = tp.ThanhPhan.Ten.ToLowerInvariant();
                if (t.Contains("socola")) CotFlavor = "socola";
                else if (t.Contains("matcha")) CotFlavor = "matcha";
                else if (t.Contains("bắp") || t.Contains("bap")) CotFlavor = "bap";
                else if (t.Contains("dâu") || t.Contains("dau")) CotFlavor = "dau";
                else CotFlavor = "vani";
            }
            else if (tp.ThanhPhan.Nhom == NhomThanhPhan.VIEN)
            {
                TenVien = tp.ThanhPhan.Ten;
                CoVien = true;
            }
            else if (tp.ThanhPhan.Nhom == NhomThanhPhan.TOPPING)
            {
                Toppings.Add((tp.ThanhPhan.Ten, tp.SoLuong));
            }
            else if (tp.ThanhPhan.Nhom == NhomThanhPhan.TANG)
            {
                SoTang = 2;
                TenTangPhu = tp.ThanhPhan.Ten;
            }
        }

        if (!string.IsNullOrWhiteSpace(tk.DuLieuJson))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(tk.DuLieuJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("mauKem", out var mk) && mk.GetString() is { } mks && !string.IsNullOrWhiteSpace(mks))
                {
                    MauKem = mks;
                    TenMauKem = LayTenMauKem(mks);
                }
                if (root.TryGetProperty("fontChu", out var fc) && fc.GetString() is { } fcs && !string.IsNullOrWhiteSpace(fcs))
                {
                    FontChu = fcs.Replace("'", "").Replace(", cursive", "").Replace(", sans-serif", "").Trim();
                }
                if (root.TryGetProperty("mauChu", out var mc) && mc.GetString() is { } mcs && !string.IsNullOrWhiteSpace(mcs))
                {
                    MauChu = mcs;
                }
                if (root.TryGetProperty("soNen", out var sn) && sn.TryGetInt32(out var sni))
                {
                    SoNen = sni;
                }
                if (root.TryGetProperty("soTang", out var st) && st.TryGetInt32(out var sti) && sti >= 2)
                {
                    SoTang = 2;
                }
            }
            catch
            {
            }
        }

        if (tk.GiaVonDuKien > 0)
        {
            GiaVonDuKien = tk.GiaVonDuKien;
        }
        else
        {
            var gvNen = await sanPhamService.GiaVonAsync(tk.SanPhamNenId);
            decimal gv = gvNen.GiaVon;
            foreach (var tp in tk.ThanhPhans)
            {
                var gvTp = await thanhPhanService.GiaVonAsync(tp.ThanhPhanId);
                gv += gvTp.GiaVon * tp.SoLuong;
            }
            GiaVonDuKien = gv;
        }

        return true;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await NapAsync(id)) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostDuyetAsync(int id)
    {
        if (!await NapAsync(id)) return NotFound();

        try
        {
            await thietKeService.DuyetAsync(id, User.NhanVienId(), PhuPhi, LyDoPhuPhi);
            TempData["Message"] = "Đã duyệt thiết kế và gửi báo giá tới khách hàng.";
            return RedirectToPage("/Bep/ThietKe/Index");
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostYeuCauSuaAsync(int id)
    {
        if (!await NapAsync(id)) return NotFound();

        if (string.IsNullOrWhiteSpace(GhiChu))
        {
            ModelState.AddModelError("GhiChu", "Ghi chú yêu cầu sửa không được để trống.");
            return Page();
        }

        try
        {
            await thietKeService.YeuCauSuaAsync(id, User.NhanVienId(), GhiChu);
            TempData["Message"] = "Đã gửi yêu cầu sửa thiết kế tới khách hàng.";
            return RedirectToPage("/Bep/ThietKe/Index");
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostTuChoiAsync(int id)
    {
        if (!await NapAsync(id)) return NotFound();

        if (string.IsNullOrWhiteSpace(LyDo))
        {
            ModelState.AddModelError("LyDo", "Lý do từ chối không được để trống.");
            return Page();
        }

        try
        {
            await thietKeService.TuChoiAsync(id, User.NhanVienId(), LyDo);
            TempData["Message"] = "Đã từ chối thiết kế.";
            return RedirectToPage("/Bep/ThietKe/Index");
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }
}
