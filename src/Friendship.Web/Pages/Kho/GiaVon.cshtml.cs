using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Kho;

public class GiaVonModel(FriendshipDb db, TimeProvider clock) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? BanhId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SoLuong { get; set; } = 20;

    [BindProperty(SupportsGet = true)]
    public string? TuKhoa { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? LocDanhGia { get; set; }

    public DateOnly HomNay { get; set; }

    // Chỉ số KPI
    public int TongBanhKinhDoanh { get; set; }
    public int SoBanhCoDinhLuong { get; set; }
    public decimal TyLeFoodCostBinhQuan { get; set; }
    public string BanhLaiCaoNhat { get; set; } = "—";
    public decimal LaiCaoNhatPhanTram { get; set; }

    // Dữ liệu danh sách
    public List<BanhGiaVonDto> DanhSachBanh { get; set; } = [];
    public DuTruMeDto? DuTruHienTai { get; set; }

    public record ChiTietDinhLuongDto(
        int NguyenLieuId,
        string TenNguyenLieu,
        string DonViTinh,
        decimal DinhMuc,
        decimal DonGiaBq,
        decimal ThanhTien,
        decimal PhanTramChiPhi,
        decimal TonKhoHienCo
    );

    public record BanhGiaVonDto(
        int Id,
        string TenBanh,
        string KichThuoc,
        string HinhAnh,
        decimal DonGiaBan,
        decimal GiaVonNguyenLieu,
        decimal LoiNhuanGop,
        decimal TyLeLoiNhuan,
        decimal TyLeFoodCost,
        int SoLuongNguyenLieu,
        string DanhGia,
        string CssClassDanhGia,
        List<ChiTietDinhLuongDto> ChiTiets
    );

    public record NguyenLieuDuTruDto(
        int NguyenLieuId,
        string TenNguyenLieu,
        string DonViTinh,
        decimal DinhMucMotBanh,
        decimal CanDungChoMe,
        decimal TonKhoHienCo,
        decimal ChenhLech,
        bool DuHang,
        decimal ChiPhiTrongMe,
        decimal TyLeChiPhi
    );

    public record DuTruMeDto(
        int SanPhamId,
        string TenBanh,
        string KichThuoc,
        string HinhAnh,
        int SoLuongBanh,
        decimal GiaBanMotBanh,
        decimal GiaVonMotBanh,
        decimal TongDoanhThuDuKien,
        decimal TongChiPhiNguyenLieu,
        decimal TongLoiNhuanGop,
        decimal TyLeLoiNhuan,
        bool DuNguyenLieuToanMe,
        List<NguyenLieuDuTruDto> ChiTietNguyenLieu
    );

    public async Task OnGetAsync()
    {
        HomNay = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

        // 1. Tải toàn bộ nguyên liệu và tính đơn giá bình quân còn hạn
        var allNl = await db.NguyenLieus.Include(n => n.Los).AsNoTracking().ToListAsync();
        var donGiaBqDict = new Dictionary<int, decimal>();
        var tonKhaDungDict = new Dictionary<int, decimal>();

        foreach (var nl in allNl)
        {
            var conHan = nl.Los.Where(l => l.SoLuongCon > 0 && l.HanSuDung >= HomNay).ToList();
            var tongTon = conHan.Sum(l => l.SoLuongCon);
            var tongGiaTri = conHan.Sum(l => l.SoLuongCon * l.DonGiaNhap);
            var giaBq = tongTon > 0
                ? tongGiaTri / tongTon
                : (nl.Los.OrderByDescending(l => l.NgayNhap).FirstOrDefault()?.DonGiaNhap ?? 0);

            donGiaBqDict[nl.Id] = giaBq;
            tonKhaDungDict[nl.Id] = tongTon;
        }

        // 2. Tải toàn bộ sản phẩm đang kinh doanh kèm định lượng
        var qSp = db.SanPhams
            .Where(s => s.DangKinhDoanh)
            .Include(s => s.DinhLuongs)
                .ThenInclude(d => d.NguyenLieu)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(TuKhoa))
        {
            var tk = TuKhoa.Trim().ToLower();
            qSp = qSp.Where(s => s.TenBanh.ToLower().Contains(tk));
        }

        var sanPhams = await qSp.OrderBy(s => s.TenBanh).ToListAsync();
        TongBanhKinhDoanh = sanPhams.Count;

        decimal tongFoodCostPct = 0;
        int countBanhCoDinhLuong = 0;
        decimal maxMargin = -1;
        string bestCake = "—";

        foreach (var sp in sanPhams)
        {
            var chiTiets = new List<ChiTietDinhLuongDto>();
            decimal tongGiaVon = 0;

            foreach (var dl in sp.DinhLuongs.OrderBy(d => d.NguyenLieu.TenGoi))
            {
                var donGiaBq = donGiaBqDict.GetValueOrDefault(dl.NguyenLieuId, 0);
                var thanhTien = dl.DinhMuc * donGiaBq;
                tongGiaVon += thanhTien;

                chiTiets.Add(new ChiTietDinhLuongDto(
                    dl.NguyenLieuId,
                    dl.NguyenLieu.TenGoi,
                    dl.NguyenLieu.DonViTinh,
                    dl.DinhMuc,
                    donGiaBq,
                    thanhTien,
                    0, // sẽ tính % sau khi có tongGiaVon
                    tonKhaDungDict.GetValueOrDefault(dl.NguyenLieuId, 0)
                ));
            }

            // Tính % chi phí từng nguyên liệu
            if (tongGiaVon > 0)
            {
                chiTiets = chiTiets.Select(c => c with
                {
                    PhanTramChiPhi = decimal.Round((c.ThanhTien / tongGiaVon) * 100m, 1)
                }).OrderByDescending(c => c.ThanhTien).ToList();
            }

            var giaBan = sp.DonGia;
            var laiGop = giaBan - tongGiaVon;
            var tyLeLai = giaBan > 0 ? decimal.Round((laiGop / giaBan) * 100m, 1) : 0;
            var foodCostPct = giaBan > 0 ? decimal.Round((tongGiaVon / giaBan) * 100m, 1) : 0;

            string danhGia;
            string cssDanhGia;
            if (sp.DinhLuongs.Count == 0)
            {
                danhGia = "Chưa có công thức";
                cssDanhGia = "badge bg-secondary";
            }
            else if (foodCostPct <= 30)
            {
                danhGia = "Biên lợi nhuận cao";
                cssDanhGia = "badge bg-success";
            }
            else if (foodCostPct <= 40)
            {
                danhGia = "Tiêu chuẩn ngành bánh";
                cssDanhGia = "badge bg-primary";
            }
            else
            {
                danhGia = "Chi phí nguyên liệu cao";
                cssDanhGia = "badge bg-warning text-dark";
            }

            if (sp.DinhLuongs.Count > 0)
            {
                countBanhCoDinhLuong++;
                tongFoodCostPct += foodCostPct;
                if (tyLeLai > maxMargin)
                {
                    maxMargin = tyLeLai;
                    bestCake = sp.TenBanh;
                }
            }

            // Lọc theo đánh giá nếu có
            if (string.IsNullOrEmpty(LocDanhGia) ||
                (LocDanhGia == "cao" && foodCostPct <= 30 && sp.DinhLuongs.Count > 0) ||
                (LocDanhGia == "tieuchuan" && foodCostPct > 30 && foodCostPct <= 40 && sp.DinhLuongs.Count > 0) ||
                (LocDanhGia == "canhbao" && foodCostPct > 40 && sp.DinhLuongs.Count > 0) ||
                (LocDanhGia == "chuaco" && sp.DinhLuongs.Count == 0))
            {
                DanhSachBanh.Add(new BanhGiaVonDto(
                    sp.Id,
                    sp.TenBanh,
                    sp.KichThuoc,
                    sp.AnhHienThi,
                    giaBan,
                    tongGiaVon,
                    laiGop,
                    tyLeLai,
                    foodCostPct,
                    sp.DinhLuongs.Count,
                    danhGia,
                    cssDanhGia,
                    chiTiets
                ));
            }
        }

        SoBanhCoDinhLuong = countBanhCoDinhLuong;
        TyLeFoodCostBinhQuan = countBanhCoDinhLuong > 0
            ? decimal.Round(tongFoodCostPct / countBanhCoDinhLuong, 1)
            : 0;
        BanhLaiCaoNhat = bestCake;
        LaiCaoNhatPhanTram = maxMargin > 0 ? maxMargin : 0;

        // 3. Tính toán Giả lập mẻ sản xuất (Batch Production Simulator)
        var banhDuTru = (BanhId.HasValue ? sanPhams.FirstOrDefault(s => s.Id == BanhId.Value) : null)
                       ?? sanPhams.FirstOrDefault(s => s.DinhLuongs.Count > 0);

        if (banhDuTru != null)
        {
            BanhId = banhDuTru.Id;
            var slBanh = Math.Max(1, SoLuong ?? 20);
            SoLuong = slBanh;

            var duTruChiTiets = new List<NguyenLieuDuTruDto>();
            decimal tongChiPhiMe = 0;

            foreach (var dl in banhDuTru.DinhLuongs.OrderBy(d => d.NguyenLieu.TenGoi))
            {
                var donGiaBq = donGiaBqDict.GetValueOrDefault(dl.NguyenLieuId, 0);
                var canDung = dl.DinhMuc * slBanh;
                var tonHienCo = tonKhaDungDict.GetValueOrDefault(dl.NguyenLieuId, 0);
                var chenhLech = tonHienCo - canDung;
                var duHang = chenhLech >= 0;
                var chiPhiNvl = canDung * donGiaBq;
                tongChiPhiMe += chiPhiNvl;

                duTruChiTiets.Add(new NguyenLieuDuTruDto(
                    dl.NguyenLieuId,
                    dl.NguyenLieu.TenGoi,
                    dl.NguyenLieu.DonViTinh,
                    dl.DinhMuc,
                    canDung,
                    tonHienCo,
                    chenhLech,
                    duHang,
                    chiPhiNvl,
                    0
                ));
            }

            if (tongChiPhiMe > 0)
            {
                duTruChiTiets = duTruChiTiets.Select(c => c with
                {
                    TyLeChiPhi = decimal.Round((c.ChiPhiTrongMe / tongChiPhiMe) * 100m, 1)
                }).OrderByDescending(c => c.ChiPhiTrongMe).ToList();
            }

            var giaBanMot = banhDuTru.DonGia;
            var giaVonMot = slBanh > 0 ? tongChiPhiMe / slBanh : 0;
            var tongDoanhThu = giaBanMot * slBanh;
            var tongLaiGop = tongDoanhThu - tongChiPhiMe;
            var tyLeLaiMe = tongDoanhThu > 0 ? decimal.Round((tongLaiGop / tongDoanhThu) * 100m, 1) : 0;
            var duNguyenLieuToanMe = duTruChiTiets.Count > 0 && duTruChiTiets.All(c => c.DuHang);

            DuTruHienTai = new DuTruMeDto(
                banhDuTru.Id,
                banhDuTru.TenBanh,
                banhDuTru.KichThuoc,
                banhDuTru.AnhHienThi,
                slBanh,
                giaBanMot,
                giaVonMot,
                tongDoanhThu,
                tongChiPhiMe,
                tongLaiGop,
                tyLeLaiMe,
                duNguyenLieuToanMe,
                duTruChiTiets
            );
        }
    }
}
