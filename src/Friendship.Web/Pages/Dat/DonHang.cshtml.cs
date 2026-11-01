using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

public class DonHangModel(FriendshipDb db) : PageModel
{
    public class DonItem
    {
        public int Id { get; set; }
        public string MaDon { get; set; } = "";
        public string Loai { get; set; } = "BanhSan"; // BanhSan hoặc ThietKe
        public DateTime NgayLap { get; set; }
        public DateTime? NgayGioLay { get; set; } // Thời gian giao hàng dự kiến
        public string TenBanh { get; set; } = "";
        public string? KichThuoc { get; set; }
        public int SoLuong { get; set; } = 1;
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai => Math.Max(0, TongTien - DaThanhToan);

        // Trạng thái hiển thị
        public string TrangThai { get; set; } = "";
        public string TrangThaiClass { get; set; } = "cho"; // cho, tin, xong, loi, trung
        public string TrangThaiChiTiet { get; set; } = "";
        public int TienDoPhanTram { get; set; } = 0; // 0..100
        public string BuocHienTai { get; set; } = "";

        // Thông tin thiết kế tùy chỉnh
        public string? MaTraCuuThietKe { get; set; }
        public string? AnhThietKe { get; set; }
        public string? AnhThanhPham { get; set; }
        public string? ChuTrenBanh { get; set; }
        public string? LoiNhan { get; set; }
        public string? GhiChuBep { get; set; }
        public decimal PhuPhi { get; set; }
        public string? LyDoPhuPhi { get; set; }
        public DateTime? HanBaoGia { get; set; }
        public decimal TienCocToiThieu { get; set; }
        public List<string> ThanhPhans { get; set; } = [];
        public int? DonHangId { get; set; }

        public string? GhiChuGiaoHang { get; set; }

        public string DuKienDisplay => NgayGioLay.HasValue
            ? NgayGioLay.Value.ToString("HH:mm - dd/MM/yyyy")
            : "Chưa xác định";

        public string DemNguoc
        {
            get
            {
                if (!NgayGioLay.HasValue) return "";
                var span = NgayGioLay.Value - DateTime.Now;
                if (span.TotalMinutes < 0) return "Đã qua giờ hẹn";
                if (span.TotalHours < 1) return $"Giao trong {Math.Max(1, (int)span.TotalMinutes)} phút nữa";
                if (span.TotalHours < 24) return $"Giao trong {(int)span.TotalHours} giờ tới";
                return $"Dự kiến {(int)span.TotalDays} ngày nữa";
            }
        }
    }

    public KhachHang? Khach { get; set; }
    public List<DonItem> DanhSachDon { get; set; } = [];
    public string? SdtTraCuu { get; set; }

    public static (string title, string cls, string desc, int percent, string step) TinhTrangThaiThietKe(ThietKeBanh tk)
    {
        if (tk.TrangThai == TrangThaiThietKe.NHAP)
        {
            return ("Bản nháp thiết kế", "trung", "Thiết kế chưa được gửi đến tiệm bánh.", 10, "Bản nháp");
        }
        if (tk.TrangThai == TrangThaiThietKe.CHO_DUYET)
        {
            return ("Chờ tiệm duyệt & Báo giá", "cho", "Tiệm bánh đang xem xét thiết kế và kiểm tra nguyên liệu làm bánh.", 25, "Bếp đang duyệt");
        }
        if (tk.TrangThai == TrangThaiThietKe.CAN_SUA)
        {
            var lyDo = !string.IsNullOrWhiteSpace(tk.GhiChuBep) ? $": {tk.GhiChuBep}" : ". Vui lòng chỉnh sửa lại theo yêu cầu của bếp.";
            return ("Cần sửa lại thiết kế", "loi", $"Bếp bánh yêu cầu chỉnh sửa{lyDo}", 20, "Cần sửa");
        }
        if (tk.TrangThai == TrangThaiThietKe.DA_BAO_GIA)
        {
            var desc = tk.PhuPhi > 0
                ? $"Tiệm đã duyệt mẫu bánh! Phụ phí tạo hình: +{tk.PhuPhi:N0} ₫ ({tk.LyDoPhuPhi}). Vui lòng đặt cọc để tiệm bắt đầu làm bánh."
                : "Tiệm đã duyệt mẫu thiết kế và gửi báo giá! Vui lòng đặt cọc để tiệm bắt đầu làm bánh.";
            return ("Tiệm đã duyệt & Báo giá", "tin", desc, 45, "Chờ bạn cọc");
        }
        if (tk.TrangThai == TrangThaiThietKe.TU_CHOI)
        {
            var lyDo = !string.IsNullOrWhiteSpace(tk.GhiChuBep) ? $": {tk.GhiChuBep}" : ".";
            return ("Tiệm từ chối mẫu", "loi", $"Rất tiếc tiệm không thể thực hiện mẫu bánh này{lyDo}", 0, "Từ chối");
        }
        if (tk.TrangThai == TrangThaiThietKe.HET_HAN)
        {
            return ("Báo giá hết hạn", "trung", "Báo giá của tiệm đã hết hiệu lực do quá thời hạn 24 giờ mà chưa nhận được đặt cọc.", 0, "Hết hạn");
        }
        if (tk.TrangThai == TrangThaiThietKe.KHACH_HUY)
        {
            return ("Đã hủy", "loi", "Mẫu thiết kế đã được bạn chủ động hủy.", 0, "Đã hủy");
        }
        if (tk.TrangThai == TrangThaiThietKe.DA_DAT)
        {
            var dh = tk.DonHang;
            if (dh is null || dh.TrangThai == TrangThaiDon.CHO_COC)
            {
                return ("Đã chốt đơn - Chờ xác nhận cọc", "cho", "Đơn bánh đã ghi nhận, đang chờ xác nhận tiền đặt cọc.", 50, "Chờ cọc");
            }
            if (dh.TrangThai == TrangThaiDon.DA_COC)
            {
                return ("Tiệm đã nhận cọc - Chờ bếp làm", "tin", "Tiệm đã nhận cọc và phân công bếp chuẩn bị nguyên liệu làm bánh.", 65, "Đã nhận cọc");
            }
            if (dh.TrangThai == TrangThaiDon.DANG_LAM)
            {
                return ("Bếp đang làm bánh", "tin", "Thợ làm bánh đang nướng cốt bánh và trang trí tạo hình theo đúng thiết kế của bạn.", 80, "Đang làm bánh");
            }
            if (dh.TrangThai == TrangThaiDon.CHO_GIAO)
            {
                var msg = !string.IsNullOrEmpty(tk.AnhThanhPham)
                    ? "Bánh đã hoàn thiện và bếp đã chụp ảnh bánh thật! Sẵn sàng giao hàng hoặc đón bạn đến lấy."
                    : "Bánh đã làm xong tại tiệm và sẵn sàng để giao hoặc đón bạn đến lấy.";
                return ("Bánh đã xong - Sẵn sàng giao / lấy", "xong", msg, 95, "Bánh đã xong");
            }
            if (dh.TrangThai == TrangThaiDon.DA_GIAO)
            {
                return ("Đang giao hàng", "tin", "Bánh kem thiết kế đang trên đường giao tới địa chỉ của bạn.", 98, "Đang giao");
            }
            if (dh.TrangThai == TrangThaiDon.DA_THANH_TOAN)
            {
                return ("Hoàn thành", "xong", "Bánh đã được giao thành công và hoàn tất đơn hàng. Cảm ơn bạn!", 100, "Hoàn tất");
            }
            if (dh.TrangThai == TrangThaiDon.DA_HUY || dh.TrangThai == TrangThaiDon.BUNG_COC)
            {
                return ("Đơn bánh đã hủy", "loi", $"Đơn bánh đã bị hủy. {dh.LyDoTra}", 0, "Đã hủy");
            }

            return ("Bếp đang làm theo thiết kế", "tin", "Bếp đang xử lý đơn bánh thiết kế của bạn.", 70, "Đang xử lý");
        }

        return (tk.TrangThai.ToString(), "trung", "", 0, "");
    }

    public async Task OnGetAsync(string? sdt)
    {
        Khach = await CustomerAuthHelper.GetCurrentCustomerAsync(HttpContext, db);

        int? khId = Khach?.Id;
        string? phone = Khach?.SoDienThoai ?? sdt?.Trim();
        SdtTraCuu = phone;

        if (khId is null && string.IsNullOrEmpty(phone))
        {
            return;
        }

        // Lấy danh sách ID khách hàng liên quan (cùng ID, SĐT hoặc cùng Tên) để không bao giờ bị sót
        var relatedKhachIds = new HashSet<int>();
        if (khId.HasValue) relatedKhachIds.Add(khId.Value);

        if (Khach is not null)
        {
            var matched = await db.KhachHangs
                .Where(k => k.Id == Khach.Id ||
                            (!string.IsNullOrEmpty(Khach.SoDienThoai) && k.SoDienThoai == Khach.SoDienThoai) ||
                            (!string.IsNullOrEmpty(Khach.TenKhachHang) && k.TenKhachHang == Khach.TenKhachHang))
                .Select(k => k.Id)
                .ToListAsync();
            foreach (var id in matched) relatedKhachIds.Add(id);
        }
        else if (!string.IsNullOrEmpty(phone))
        {
            var matched = await db.KhachHangs
                .Where(k => k.SoDienThoai == phone)
                .Select(k => k.Id)
                .ToListAsync();
            foreach (var id in matched) relatedKhachIds.Add(id);
        }

        // 1. Lấy các đơn thiết kế tùy chỉnh (ThietKeBanh)
        var tkQuery = db.ThietKeBanhs
            .Include(t => t.SanPhamNen)
            .Include(t => t.DonHang)
                .ThenInclude(d => d!.ThanhToans)
            .Include(t => t.ThanhPhans)
                .ThenInclude(tp => tp.ThanhPhan)
            .Where(t => relatedKhachIds.Contains(t.KhachHangId) || 
                        (phone != null && t.KhachHang != null && t.KhachHang.SoDienThoai == phone))
            .AsNoTracking();

        var tks = await tkQuery.OrderByDescending(t => t.NgayGioLay).ToListAsync();
        var donIdsFromThietKe = tks.Where(t => t.DonHangId.HasValue).Select(t => t.DonHangId!.Value).ToHashSet();

        foreach (var t in tks)
        {
            var (ttText, ttCls, ttMoTa, phanTram, buoc) = TinhTrangThaiThietKe(t);
            var tongTien = t.GiaCuoi > 0 ? t.GiaCuoi : t.GiaTamTinh;
            var daThu = t.DonHang?.DaThanhToan ?? 0;
            var tienCocMin = tongTien > 0 ? CocRule.Khoang(tongTien).Min : 0;

            DanhSachDon.Add(new DonItem
            {
                Id = t.DonHangId ?? t.Id,
                MaDon = t.DonHang != null ? $"{t.DonHang.Ma} ({t.Ma})" : t.Ma,
                Loai = "ThietKe",
                NgayLap = t.NgayGui ?? DateTime.Now,
                NgayGioLay = t.NgayGioLay,
                TenBanh = $"Bánh tự thiết kế - {t.SanPhamNen.TenBanh}",
                KichThuoc = t.SanPhamNen.KichThuoc,
                SoLuong = 1,
                TongTien = tongTien,
                DaThanhToan = daThu,
                TrangThai = ttText,
                TrangThaiClass = ttCls,
                TrangThaiChiTiet = ttMoTa,
                TienDoPhanTram = phanTram,
                BuocHienTai = buoc,
                MaTraCuuThietKe = t.MaTraCuu,
                AnhThietKe = t.AnhThietKe,
                AnhThanhPham = t.AnhThanhPham,
                ChuTrenBanh = t.ChuTrenBanh,
                LoiNhan = t.LoiNhan,
                GhiChuBep = t.GhiChuBep,
                PhuPhi = t.PhuPhi,
                LyDoPhuPhi = t.LyDoPhuPhi,
                HanBaoGia = t.HanBaoGia,
                TienCocToiThieu = tienCocMin,
                ThanhPhans = t.ThanhPhans.Select(tp => $"{tp.ThanhPhan.Ten} (x{tp.SoLuong})").ToList(),
                DonHangId = t.DonHangId
            });
        }

        // 2. Lấy đơn hàng bán lẻ / bán sẵn thông thường (không trùng đơn thiết kế đã thêm ở trên)
        var donQuery = db.DonHangs
            .Include(d => d.ChiTiets)
                .ThenInclude(c => c.SanPham)
            .Include(d => d.ThanhToans)
            .Where(d => (d.KhachHangId.HasValue && relatedKhachIds.Contains(d.KhachHangId.Value)) || 
                        (phone != null && d.KhachHang != null && d.KhachHang.SoDienThoai == phone))
            .AsNoTracking();

        var dons = await donQuery.OrderByDescending(d => d.NgayLap).ToListAsync();

        foreach (var d in dons)
        {
            if (donIdsFromThietKe.Contains(d.Id)) continue; // Đã hiển thị dưới dạng thiết kế

            var tenBanh = d.ChiTiets.FirstOrDefault()?.SanPham.TenBanh ?? "Bánh ngọt";
            if (d.ChiTiets.Count > 1) tenBanh += $" (+{d.ChiTiets.Count - 1} món khác)";
            var tongSl = d.ChiTiets.Sum(c => c.SoLuong);

            var (ttText, ttCls, phanTram, moTa) = d.TrangThai switch
            {
                TrangThaiDon.CHO_COC => ("Chờ thanh toán / cọc", "cho", 30, "Đơn hàng đang chờ thanh toán tiền cọc."),
                TrangThaiDon.DA_COC => ("Đã cọc - Chờ bếp làm", "tin", 60, "Tiệm đã nhận cọc và phân công bếp làm bánh."),
                TrangThaiDon.DANG_LAM => ("Bếp đang làm bánh", "tin", 80, "Thợ làm bánh đang tiến hành thực hiện đơn bánh của bạn."),
                TrangThaiDon.CHO_GIAO => ("Bánh đã xong - Chờ giao", "xong", 95, "Bánh đã làm xong, sẵn sàng giao hoặc đón bạn đến nhận."),
                TrangThaiDon.DA_GIAO => ("Đang giao hàng", "tin", 98, "Bánh đang trên đường vận chuyển tới địa chỉ của bạn."),
                TrangThaiDon.DA_THANH_TOAN => ("Hoàn thành", "xong", 100, "Đơn hàng đã giao thành công và hoàn tất thanh toán."),
                TrangThaiDon.DA_HUY => ("Đã hủy", "loi", 0, "Đơn hàng đã hủy."),
                _ => (d.TrangThai.ToString(), "trung", 50, "")
            };

            DanhSachDon.Add(new DonItem
            {
                Id = d.Id,
                MaDon = d.Ma,
                Loai = "BanhSan",
                NgayLap = d.NgayLap,
                NgayGioLay = d.NgayGioLay,
                TenBanh = tenBanh,
                SoLuong = tongSl,
                TongTien = d.TongThanhToan,
                DaThanhToan = d.DaThanhToan,
                TrangThai = ttText,
                TrangThaiClass = ttCls,
                TrangThaiChiTiet = moTa,
                TienDoPhanTram = phanTram,
                GhiChuGiaoHang = d.LyDoTra
            });
        }

        DanhSachDon = DanhSachDon.OrderByDescending(d => d.NgayGioLay ?? d.NgayLap).ToList();
    }
}

