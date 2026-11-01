using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Kho;

public class TonKhoModel(FriendshipDb db, TimeProvider clock) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? TuKhoa { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; } = "nguyenlieu";

    public DateOnly HomNay { get; set; }
    public DateTime BayGio { get; set; }

    // KPI stats
    public int TongNguyenLieu { get; set; }
    public int SoNguyenLieuCanhBao { get; set; }
    public int TongLoBanh { get; set; }
    public int SoLoBanhCanDate { get; set; }

    // Data lists
    public List<NguyenLieuItemDto> DanhSachNguyenLieu { get; set; } = [];
    public List<LoSanPhamItemDto> DanhSachLoBanh { get; set; } = [];

    public record NguyenLieuItemDto(
        int Id,
        string TenGoi,
        string DonViTinh,
        decimal MucCanhBaoTon,
        decimal TonConHan,
        decimal TonHetHan,
        decimal DonGiaBinhQuan,
        decimal TongGiaTri,
        bool DuoiMucCanhBao,
        List<LoNguyenLieu> ChiTietLos
    );

    public record LoSanPhamItemDto(
        int Id,
        string TenBanh,
        string KichThuoc,
        decimal DonGia,
        int SoLuongSanXuat,
        int SoLuongTon,
        DateTime NgaySanXuat,
        DateTime HanSuDung,
        decimal TyLeGiamXaHang,
        string TinhTrang,
        string CssBadge,
        string HinhAnh
    );

    public async Task OnGetAsync()
    {
        HomNay = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        BayGio = clock.GetLocalNow().DateTime;

        var qNl = db.NguyenLieus.Include(n => n.Los).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(TuKhoa))
        {
            var tk = TuKhoa.Trim().ToLower();
            qNl = qNl.Where(n => n.TenGoi.ToLower().Contains(tk));
        }

        var allNls = await qNl.OrderBy(n => n.TenGoi).ToListAsync();
        TongNguyenLieu = allNls.Count;

        foreach (var nl in allNls)
        {
            var conHan = nl.Los.Where(l => l.SoLuongCon > 0 && l.HanSuDung >= HomNay).ToList();
            var hetHan = nl.Los.Where(l => l.SoLuongCon > 0 && l.HanSuDung < HomNay).ToList();

            var tongConHan = conHan.Sum(l => l.SoLuongCon);
            var tongHetHan = hetHan.Sum(l => l.SoLuongCon);

            var giaTri = conHan.Sum(l => l.SoLuongCon * l.DonGiaNhap);
            var donGiaBq = tongConHan > 0 ? giaTri / tongConHan : (nl.Los.OrderByDescending(l => l.NgayNhap).FirstOrDefault()?.DonGiaNhap ?? 0);

            var duoiMuc = tongConHan < nl.MucCanhBaoTon;
            if (duoiMuc) SoNguyenLieuCanhBao++;

            DanhSachNguyenLieu.Add(new NguyenLieuItemDto(
                nl.Id,
                nl.TenGoi,
                nl.DonViTinh,
                nl.MucCanhBaoTon,
                tongConHan,
                tongHetHan,
                donGiaBq,
                giaTri,
                duoiMuc,
                nl.Los.Where(l => l.SoLuongCon > 0).OrderBy(l => l.HanSuDung).ToList()
            ));
        }

        // Lấy danh sách lô bánh thành phẩm
        var qLo = db.LoSanPhams.Include(l => l.SanPham).AsNoTracking().Where(l => l.SoLuongTon > 0).AsQueryable();
        if (!string.IsNullOrWhiteSpace(TuKhoa))
        {
            var tk = TuKhoa.Trim().ToLower();
            qLo = qLo.Where(l => l.SanPham.TenBanh.ToLower().Contains(tk));
        }

        var allLos = await qLo.OrderBy(l => l.HanSuDung).ToListAsync();
        TongLoBanh = allLos.Count;

        foreach (var lo in allLos)
        {
            string tinhTrang;
            string css;

            if (lo.HanSuDung < BayGio)
            {
                tinhTrang = "Hết hạn";
                css = "bg-danger text-white";
                SoLoBanhCanDate++;
            }
            else if (lo.HanSuDung <= BayGio.AddHours(6))
            {
                tinhTrang = "Cận date (≤6h)";
                css = "bg-warning text-dark";
                SoLoBanhCanDate++;
            }
            else
            {
                tinhTrang = "Còn hạn";
                css = "bg-success text-white";
            }

            DanhSachLoBanh.Add(new LoSanPhamItemDto(
                lo.Id,
                lo.SanPham.TenBanh,
                lo.SanPham.KichThuoc,
                lo.SanPham.DonGia,
                lo.SoLuongSanXuat,
                lo.SoLuongTon,
                lo.NgaySanXuat,
                lo.HanSuDung,
                lo.TyLeGiamXaHang,
                tinhTrang,
                css,
                lo.SanPham.AnhHienThi
            ));
        }
    }
}
