using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongTonNguyenLieu(
    int NguyenLieuId,
    string TenNguyenLieu,
    string DonViTinh,
    decimal TongTonConHan,
    decimal TongTonHetHan,
    decimal DonGiaBinhQuan,
    decimal GiaTriTonConHan,
    decimal GiaTriTonHetHan
)
{
    public decimal TongTon => TongTonConHan + TongTonHetHan;
    public decimal TongGiaTri => GiaTriTonConHan + GiaTriTonHetHan;
}

public record DongTonThanhPham(
    int SanPhamId,
    string TenSanPham,
    string KichThuoc,
    int SoLuongTonConHan,
    int SoLuongTonHetHan,
    decimal GiaVonUocTinh,
    decimal GiaTriTonConHan,
    decimal GiaTriTonHetHan
)
{
    public int TongTon => SoLuongTonConHan + SoLuongTonHetHan;
    public decimal TongGiaTri => GiaTriTonConHan + GiaTriTonHetHan;
}

public record BaoCaoGiaTriTonKho(
    DateTime ThoiDiem,
    List<DongTonNguyenLieu> NguyenLieu,
    decimal TongGiaTriNguyenLieuConHan,
    decimal TongGiaTriNguyenLieuHetHan,
    List<DongTonThanhPham> ThanhPham,
    decimal TongGiaTriThanhPhamConHan,
    decimal TongGiaTriThanhPhamHetHan
)
{
    public decimal TongGiaTriNguyenLieu => TongGiaTriNguyenLieuConHan + TongGiaTriNguyenLieuHetHan;
    public decimal TongGiaTriThanhPham => TongGiaTriThanhPhamConHan + TongGiaTriThanhPhamHetHan;
    public decimal TongGiaTriTonKho => TongGiaTriNguyenLieuConHan + TongGiaTriThanhPhamConHan;
    public decimal TongTonThatTiemAn => TongGiaTriNguyenLieuHetHan + TongGiaTriThanhPhamHetHan;
    public decimal TongGiaTriToanBo => TongGiaTriTonKho + TongTonThatTiemAn;
}

public partial class BaoCaoService
{
    DateTime BayGio => (clock ?? TimeProvider.System).GetLocalNow().DateTime;
    DateOnly HomNay => DateOnly.FromDateTime(BayGio);

    public async Task<BaoCaoGiaTriTonKho> GiaTriTonKhoAsync(string? tuKhoa = null)
    {
        var bayGio = BayGio;
        var homNay = HomNay;
        var tuKhoaChuan = tuKhoa?.Trim().ToLower();

        // 1. Tồn nguyên liệu
        var qNl = db.NguyenLieus.Include(n => n.Los).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoaChuan))
            qNl = qNl.Where(n => n.TenGoi.ToLower().Contains(tuKhoaChuan));

        var nls = await qNl.OrderBy(n => n.TenGoi).ToListAsync();
        var dsNl = new List<DongTonNguyenLieu>();

        foreach (var nl in nls)
        {
            var loConTon = nl.Los.Where(l => l.SoLuongCon > 0).ToList();
            var loConHan = loConTon.Where(l => l.HanSuDung >= homNay).ToList();
            var loHetHan = loConTon.Where(l => l.HanSuDung < homNay).ToList();

            var tonConHan = loConHan.Sum(l => l.SoLuongCon);
            var tonHetHan = loHetHan.Sum(l => l.SoLuongCon);
            var tongTon = tonConHan + tonHetHan;

            var giaTriConHan = loConHan.Sum(l => l.SoLuongCon * l.DonGiaNhap);
            var giaTriHetHan = loHetHan.Sum(l => l.SoLuongCon * l.DonGiaNhap);
            var tongGiaTri = giaTriConHan + giaTriHetHan;

            var donGiaBinhQuan = tongTon > 0 ? tongGiaTri / tongTon : 0m;

            dsNl.Add(new DongTonNguyenLieu(
                nl.Id,
                nl.TenGoi,
                nl.DonViTinh,
                tonConHan,
                tonHetHan,
                donGiaBinhQuan,
                giaTriConHan,
                giaTriHetHan
            ));
        }

        // 2. Tồn thành phẩm (LoSanPham theo sản phẩm)
        var qSp = db.SanPhams.Include(s => s.Los).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoaChuan))
            qSp = qSp.Where(s => s.TenBanh.ToLower().Contains(tuKhoaChuan));

        var sps = await qSp.OrderBy(s => s.TenBanh).ThenBy(s => s.KichThuoc).ToListAsync();
        var dsTp = new List<DongTonThanhPham>();

        foreach (var sp in sps)
        {
            var loConTon = sp.Los.Where(l => l.SoLuongTon > 0).ToList();
            if (loConTon.Count == 0) continue;

            var loConHan = loConTon.Where(l => l.HanSuDung > bayGio).ToList();
            var loHetHan = loConTon.Where(l => l.HanSuDung <= bayGio).ToList();

            var tonConHan = loConHan.Sum(l => l.SoLuongTon);
            var tonHetHan = loHetHan.Sum(l => l.SoLuongTon);

            var giaVon = (await sanPham.GiaVonAsync(sp.Id)).GiaVon;
            var giaTriConHan = tonConHan * giaVon;
            var giaTriHetHan = tonHetHan * giaVon;

            dsTp.Add(new DongTonThanhPham(
                sp.Id,
                sp.TenBanh,
                sp.KichThuoc,
                tonConHan,
                tonHetHan,
                giaVon,
                giaTriConHan,
                giaTriHetHan
            ));
        }

        var tongGiaTriNlConHan = dsNl.Sum(x => x.GiaTriTonConHan);
        var tongGiaTriNlHetHan = dsNl.Sum(x => x.GiaTriTonHetHan);
        var tongGiaTriTpConHan = dsTp.Sum(x => x.GiaTriTonConHan);
        var tongGiaTriTpHetHan = dsTp.Sum(x => x.GiaTriTonHetHan);

        return new BaoCaoGiaTriTonKho(
            bayGio,
            dsNl,
            tongGiaTriNlConHan,
            tongGiaTriNlHetHan,
            dsTp,
            tongGiaTriTpConHan,
            tongGiaTriTpHetHan
        );
    }
}
