using Friendship.Domain;
using Friendship.Domain.Entities;

namespace Friendship.Infrastructure.Services;

public class LoBanhService(FriendshipDb db, TimeProvider clock)
{
    public const int SoLuongToiDa = 100_000;

    /// <summary>UC13: tạo lô bánh cho mẻ bán lẻ; HSD = giờ ra lò + HanSuDungGio của sản phẩm.</summary>
    public async Task<LoSanPham> GhiNhanMeAsync(int sanPhamId, int soLuong, DateTime? gioRaLo = null)
    {
        var bay = clock.GetLocalNow().DateTime;
        var sp = await db.SanPhams.FindAsync(sanPhamId) ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
        if (!sp.DangKinhDoanh) throw new NghiepVuException($"Sản phẩm '{sp.TenBanh}' đã ngừng kinh doanh.");
        if (sp.HanSuDungGio <= 0)
            throw new NghiepVuException($"Sản phẩm '{sp.TenBanh}' chưa khai báo hạn sử dụng (giờ). Báo quản lý bổ sung.");
        if (soLuong <= 0) throw new NghiepVuException("Số lượng phải lớn hơn 0.");
        if (soLuong > SoLuongToiDa) throw new NghiepVuException($"Số lượng quá lớn (tối đa {SoLuongToiDa}).");
        var ra = gioRaLo ?? bay;
        if (ra > bay) throw new NghiepVuException("Giờ ra lò không được ở tương lai.");
        var hsd = ra.AddHours(sp.HanSuDungGio);
        if (hsd <= bay) throw new NghiepVuException("Mẻ này đã quá hạn sử dụng ngay khi ghi nhận. Kiểm tra lại giờ ra lò.");

        var lo = new LoSanPham
        {
            SanPhamId = sp.Id, NgaySanXuat = ra, HanSuDung = hsd,
            SoLuongSanXuat = soLuong, SoLuongTon = soLuong, TyLeGiamXaHang = 0
        };
        db.LoSanPhams.Add(lo);
        await db.SaveChangesAsync();
        return lo;
    }
}
