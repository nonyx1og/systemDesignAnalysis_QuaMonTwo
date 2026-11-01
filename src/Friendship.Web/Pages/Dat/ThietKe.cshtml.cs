using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Friendship.Web.Pages.Dat;

[EnableRateLimiting("khach")]
[ValidateAntiForgeryToken]
public class ThietKeModel(
    ThietKeService thietKeService,
    ThanhPhanService thanhPhanService,
    SanPhamService sanPhamService,
    AnhService anhService,
    FriendshipDb db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Ma { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Mau { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int BuocLoi { get; set; } = 1;

    public List<SanPham> SanPhams { get; set; } = [];
    public List<ThanhPhanTrangTri> ThanhPhans { get; set; } = [];
    public string DanhMucJson { get; set; } = "{}";

    public class InputModel
    {
        public string? MaTraCuu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn thời gian lấy bánh.")]
        public DateTime NgayGioLay { get; set; } = DateTime.Now.AddDays(3);

        [Required(ErrorMessage = "Dữ liệu thiết kế không được để trống.")]
        [MaxLength(ThietKeRule.JsonToiDa, ErrorMessage = "Dữ liệu thiết kế quá lớn.")]
        public string DuLieuJson { get; set; } = "";

        [StringLength(ThietKeRule.ChuToiDa, ErrorMessage = "Chữ trên bánh tối đa 30 ký tự.")]
        public string? ChuTrenBanh { get; set; }

        [StringLength(ThietKeRule.LoiNhanToiDa, ErrorMessage = "Lời nhắn tối đa 200 ký tự.")]
        public string? LoiNhan { get; set; }

        [Range(0, ThietKeRule.SoNenToiDa, ErrorMessage = "Số nến phải từ 0 đến 10.")]
        public int? SoNen { get; set; }

        public IFormFile? AnhThietKe { get; set; }
        public IFormFile? AnhInAn { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? ma)
    {
        Ma = ma;
        await NapDuLieuTrangAsync();

        if (!string.IsNullOrWhiteSpace(ma))
        {
            var tk = await thietKeService.LayTheoMaAsync(ma);
            if (tk is null) return NotFound();

            if (!ThietKeRule.KhachSuaDuoc(tk.TrangThai))
            {
                TempData["Error"] = "Thiết kế này đang ở trạng thái không thể sửa.";
                return Redirect($"/Dat/Theo/{ma}");
            }

            Input.MaTraCuu = tk.MaTraCuu;
            Input.HoTen = tk.KhachHang.TenKhachHang;
            Input.SoDienThoai = tk.KhachHang.SoDienThoai;
            Input.NgayGioLay = tk.NgayGioLay;
            Input.DuLieuJson = tk.DuLieuJson;
            Input.ChuTrenBanh = tk.ChuTrenBanh;
            Input.LoiNhan = tk.LoiNhan;
        }
        else
        {
            var khach = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);
            if (khach is not null)
            {
                Input.HoTen = khach.TenKhachHang;
                Input.SoDienThoai = khach.SoDienThoai;
            }
        }

        return Page();
    }


    public async Task<IActionResult> OnPostLuuAsync()
    {
        // 0. Chuẩn hóa số điện thoại trước khi kiểm tra
        if (!string.IsNullOrWhiteSpace(Input.SoDienThoai))
        {
            try
            {
                Input.SoDienThoai = KhachHangService.ChuanHoaSdt(Input.SoDienThoai) ?? "";
            }
            catch (NghiepVuException ex)
            {
                ModelState.AddModelError("Input.SoDienThoai", ex.Message);
            }
        }

        // 1. Kiểm tra ảnh thiết kế (data URL canvas xuất ra)
        if (Input.AnhThietKe is not null && Input.AnhThietKe.Length > 0)
        {
            if (Input.AnhThietKe.Length > ThietKeRule.AnhDataUrlDungLuongToiDa)
            {
                ModelState.AddModelError("Input.AnhThietKe", "Ảnh thiết kế dung lượng tối đa 2 MB.");
            }
            else if (Input.AnhThietKe.ContentType is not ("image/png" or "image/jpeg"))
            {
                ModelState.AddModelError("Input.AnhThietKe", "Chỉ nhận ảnh thiết kế dạng PNG hoặc JPG.");
            }
        }

        // 2. Kiểm tra ảnh in ăn được (nếu có tải lên)
        if (Input.AnhInAn is not null && Input.AnhInAn.Length > 0)
        {
            if (Input.AnhInAn.Length > ThietKeRule.AnhDataUrlDungLuongToiDa)
            {
                ModelState.AddModelError("Input.AnhInAn", "Ảnh in ăn được dung lượng tối đa 2 MB.");
            }
            else if (Input.AnhInAn.ContentType is not ("image/png" or "image/jpeg"))
            {
                ModelState.AddModelError("Input.AnhInAn", "Chỉ nhận ảnh in dạng PNG hoặc JPG.");
            }
        }

        // 3. Kiểm tra ngày hẹn lấy bánh
        var bayGio = DateTime.Now;
        if (Input.NgayGioLay <= bayGio)
        {
            ModelState.AddModelError("Input.NgayGioLay", "Thời gian lấy bánh không được ở quá khứ.");
        }
        else if (Input.NgayGioLay < bayGio.AddHours(ThietKeRule.GioDatTruocToiThieu))
        {
            ModelState.AddModelError("Input.NgayGioLay", $"Thời gian lấy bánh phải cách hiện tại ít nhất {ThietKeRule.GioDatTruocToiThieu} giờ.");
        }

        if (!ModelState.IsValid)
        {
            BuocLoi = XacDinhBuocLoi();
            await NapDuLieuTrangAsync();
            return Page();
        }

        string tenAnhThietKe = "";
        string? tenAnhIn = null;

        try
        {
            ThietKeBanh? tkCu = null;
            if (!string.IsNullOrWhiteSpace(Input.MaTraCuu))
            {
                tkCu = await thietKeService.LayTheoMaAsync(Input.MaTraCuu);
                if (tkCu is null) return NotFound();
                if (!ThietKeRule.KhachSuaDuoc(tkCu.TrangThai))
                {
                    ModelState.AddModelError("", "Thiết kế này không thể sửa.");
                    BuocLoi = 5;
                    await NapDuLieuTrangAsync();
                    return Page();
                }
                tenAnhThietKe = tkCu.AnhThietKe;
                tenAnhIn = tkCu.AnhInAn;
            }

            if (Input.AnhThietKe is not null && Input.AnhThietKe.Length > 0)
            {
                using var stream = Input.AnhThietKe.OpenReadStream();
                tenAnhThietKe = await anhService.LuuAsync(stream);
            }
            else if (string.IsNullOrEmpty(tenAnhThietKe))
            {
                ModelState.AddModelError("Input.AnhThietKe", "Thiết kế cần có hình ảnh mẫu.");
                BuocLoi = 4;
                await NapDuLieuTrangAsync();
                return Page();
            }

            if (Input.AnhInAn is not null && Input.AnhInAn.Length > 0)
            {
                using var stream = Input.AnhInAn.OpenReadStream();
                tenAnhIn = await anhService.LuuAsync(stream);
            }

            var input = new LuuThietKeInput(
                Input.MaTraCuu,
                Input.HoTen,
                Input.SoDienThoai,
                Input.NgayGioLay,
                Input.DuLieuJson,
                Input.ChuTrenBanh,
                Input.LoiNhan,
                tenAnhThietKe,
                tenAnhIn);

            var tk = await thietKeService.LuuNhapAsync(input);
            var khachHienTai = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);
            if (khachHienTai is not null && tk.KhachHangId != khachHienTai.Id)
            {
                tk.KhachHangId = khachHienTai.Id;
                await db.SaveChangesAsync();
            }

            try
            {
                await thietKeService.GuiAsync(tk.MaTraCuu);
                TempData["Message"] = "Thiết kế của bạn đã được gửi tới bếp bánh! Bếp sẽ kiểm tra và gửi báo giá cho bạn.";
            }
            catch
            {
                // Bỏ qua nếu đã gửi trước đó
            }
            return Redirect($"/Dat/Theo/{tk.MaTraCuu}");
        }
        catch (NghiepVuException ex)
        {
            GhiNhanLoiNghiepVu(ex.Message);
            BuocLoi = XacDinhBuocLoi();
            await NapDuLieuTrangAsync();
            return Page();
        }
    }

    public Task<IActionResult> OnPostAsync() => OnPostLuuAsync();

    void GhiNhanLoiNghiepVu(string msg)
    {
        var lower = msg.ToLowerInvariant();
        if (lower.Contains("quá khứ") || lower.Contains("cách hiện tại") || lower.Contains("thời gian lấy"))
        {
            ModelState.AddModelError("Input.NgayGioLay", msg);
        }
        else if (lower.Contains("họ tên"))
        {
            ModelState.AddModelError("Input.HoTen", msg);
        }
        else if (lower.Contains("số điện thoại"))
        {
            ModelState.AddModelError("Input.SoDienThoai", msg);
        }
        else if (lower.Contains("chữ trên bánh"))
        {
            ModelState.AddModelError("Input.ChuTrenBanh", msg);
        }
        else if (lower.Contains("lời nhắn"))
        {
            ModelState.AddModelError("Input.LoiNhan", msg);
        }
        else if (lower.Contains("ảnh"))
        {
            ModelState.AddModelError("Input.AnhThietKe", msg);
        }
        else if (lower.Contains("nến"))
        {
            ModelState.AddModelError("Input.SoNen", msg);
        }
        else
        {
            ModelState.AddModelError("Input.DuLieuJson", msg);
        }
        ModelState.AddModelError("", msg);
    }

    int XacDinhBuocLoi()
    {
        // Duyệt theo thứ tự bước 1 -> 5 để trả về bước đầu tiên bị lỗi
        // Bước 1: Bánh nền
        if (CoLoiKey("DuLieuJson")) return 1; // Dữ liệu thiết kế trống/quá lớn/không hợp lệ/sai JSON
        if (CoLoi("bánh nền")) return 1;

        // Bước 2: Cốt bánh
        if (CoLoi("cốt")) return 2;

        // Bước 3: Viền, Topping, Nến, Thành phần trang trí
        if (CoLoiKey("SoNen") || CoLoi("nến") || CoLoi("viền") || CoLoi("topping") || CoLoi("thành phần")) return 3;

        // Bước 4: Chữ trên bánh, Ảnh thiết kế, Ảnh in
        if (CoLoiKey("ChuTrenBanh") || CoLoiKey("AnhThietKe") || CoLoiKey("AnhInAn") || CoLoi("chữ trên bánh") || CoLoi("ảnh")) return 4;

        // Bước 5: Thông tin người nhận, Ngày giờ lấy, Lời nhắn
        if (CoLoiKey("NgayGioLay") || CoLoiKey("HoTen") || CoLoiKey("SoDienThoai") || CoLoiKey("LoiNhan")
            || CoLoi("thời gian") || CoLoi("quá khứ") || CoLoi("họ tên") || CoLoi("số điện thoại") || CoLoi("lời nhắn"))
            return 5;

        return 1;
    }

    bool CoLoi(string tuKhoa)
    {
        var lower = tuKhoa.ToLowerInvariant();
        return ModelState.Values.SelectMany(v => v.Errors)
            .Any(e => e.ErrorMessage.ToLowerInvariant().Contains(lower));
    }

    bool CoLoiKey(string keySubstring)
    {
        return ModelState.Keys.Any(k => k.Contains(keySubstring) && ModelState[k]?.Errors.Count > 0);
    }

    async Task NapDuLieuTrangAsync()
    {
        ThanhPhans = await thanhPhanService.TimAsync(null, true);
        // Bánh nền = sản phẩm có ít nhất một cốt gắn với nó; sản phẩm bán lẻ thường không chọn được.
        var nenIds = ThanhPhans.Where(t => t.Nhom == NhomThanhPhan.COT).Select(t => t.SanPhamNenId).ToHashSet();
        SanPhams = (await sanPhamService.TimAsync(null, true)).Where(s => nenIds.Contains(s.Id)).ToList();

        var sanPhamsDto = SanPhams.Select(s => new
        {
            id = s.Id,
            ten = s.TenBanh,
            kichThuoc = s.KichThuoc,
            gia = s.DonGia
        });

        var thanhPhansDto = ThanhPhans.Select(t => new
        {
            id = t.Id,
            nhom = t.Nhom.ToString(),
            ten = t.Ten,
            gia = t.GiaBan,
            toiDa = t.SoLuongToiDa,
            anh = t.AnhMinhHoa,
            sanPhamNenId = t.SanPhamNenId
        });

        DanhMucJson = JsonSerializer.Serialize(new
        {
            sanPhams = sanPhamsDto,
            thanhPhans = thanhPhansDto,
            mau = Mau
        });
    }
}
