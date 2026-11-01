using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public enum LocHuy { TatCa, Banh, NguyenLieu }
public record DongBanChay(int SanPhamId, string TenBanh, int SoLuong, decimal DoanhSo);
public record DongTopHuy(string DoiTuong, LoaiDoiTuongHuy Loai, decimal SoLuong, decimal TonThat);
public record BaoCaoHuyBanChay(DateOnly Tu, DateOnly Den, List<DongBanChay> BanChay, List<DongTopHuy> TopHuy,
    Dictionary<LyDoHuy, decimal> TonThatTheoLyDo, decimal TongTonThat, decimal DoanhThu, decimal? TyLeTonThat);

record HuyDong(LoaiDoiTuongHuy Loai, string Khoa, string Ten, ChiTietXuatHuy C);

public partial class BaoCaoService
{
    /// <summary>UC22: bán chạy theo số lượng; hủy chỉ tính phiếu đã duyệt trong kỳ theo ngày duyệt; tỷ lệ tổn thất = hủy / doanh thu (UC21), null khi doanh thu bằng 0.</summary>
    public async Task<BaoCaoHuyBanChay> HuyVaBanChayAsync(DateOnly tu, DateOnly den, int top = 5, LocHuy loc = LocHuy.TatCa)
    {
        var (tuGio, denGio) = Khoang(tu, den);
        if (top < 1 || top > 50) throw new NghiepVuException("Số mục hiển thị từ 1 đến 50.");

        var ban = await LayDongBanAsync(tuGio, denGio);
        var ten = await db.SanPhams.ToDictionaryAsync(s => s.Id, s => s.TenBanh);
        var banChay = ban.GroupBy(b => b.SanPhamId)
            .Select(g => new DongBanChay(g.Key, ten.GetValueOrDefault(g.Key, "?"), g.Sum(x => x.SoLuong), g.Sum(x => x.ThanhTien)))
            .OrderByDescending(x => x.SoLuong).ThenByDescending(x => x.DoanhSo).ThenBy(x => x.TenBanh)
            .Take(top).ToList();

        var dong = (await db.ChiTietXuatHuys
            .Include(c => c.LoNguyenLieu).ThenInclude(l => l!.NguyenLieu)
            .Include(c => c.LoSanPham).ThenInclude(l => l!.SanPham)
            .Include(c => c.DonHang)
            .Where(c => c.PhieuXuatHuy.TrangThai == TrangThaiPhieu.DA_DUYET
                     && c.PhieuXuatHuy.NgayDuyet >= tuGio && c.PhieuXuatHuy.NgayDuyet < denGio)
            .ToListAsync())
            .Select(c => c.LoNguyenLieu is not null ? new HuyDong(LoaiDoiTuongHuy.LO_NGUYEN_LIEU, "nl" + c.LoNguyenLieu.NguyenLieuId, c.LoNguyenLieu.NguyenLieu.TenGoi, c)
                : c.LoSanPham is not null ? new HuyDong(LoaiDoiTuongHuy.LO_BANH, "sp" + c.LoSanPham.SanPhamId, c.LoSanPham.SanPham.TenBanh, c)
                : new HuyDong(LoaiDoiTuongHuy.DON_BUNG_COC, "don" + c.DonHangId, "Đơn " + c.DonHang!.Ma, c))
            .Where(x => loc switch
            {
                LocHuy.NguyenLieu => x.Loai == LoaiDoiTuongHuy.LO_NGUYEN_LIEU,
                LocHuy.Banh => x.Loai != LoaiDoiTuongHuy.LO_NGUYEN_LIEU,
                _ => true
            }).ToList();

        var topHuy = dong.GroupBy(x => x.Khoa)
            .Select(g => new DongTopHuy(g.First().Ten, g.First().Loai, g.Sum(x => x.C.SoLuongHuy), g.Sum(x => x.C.SoLuongHuy * x.C.DonGiaVon)))
            .OrderByDescending(x => x.TonThat).ThenBy(x => x.DoiTuong).Take(top).ToList();
        var theoLyDo = dong.GroupBy(x => x.C.LyDo).ToDictionary(g => g.Key, g => g.Sum(x => x.C.SoLuongHuy * x.C.DonGiaVon));
        var tong = dong.Sum(x => x.C.SoLuongHuy * x.C.DonGiaVon);

        var doanhThu = (await DoanhThuAsync(tu, den)).DoanhThu;
        return new BaoCaoHuyBanChay(tu, den, banChay, topHuy, theoLyDo, tong, doanhThu, doanhThu > 0 ? tong / doanhThu : null);
    }
}
