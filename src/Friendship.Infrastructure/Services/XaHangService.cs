using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record LoXaHang(LoSanPham Lo, TimeSpan ConLai, decimal GiaGoc, decimal GiaSauGiam);

/// <summary>UC08: quản lý đặt mức giảm cho lô bánh cận date (BR17, BR24). Quầy bán theo giá này ở BanLeService.</summary>
public class XaHangService(FriendshipDb db, TimeProvider clock)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;

    public async Task<List<LoXaHang>> DanhSachAsync()
    {
        var bay = BayGio;
        var han = bay.AddHours(HanSuDungRule.NguongGioBanh);
        var ds = await db.LoSanPhams.Include(l => l.SanPham)
            .Where(l => l.SoLuongTon > 0 && l.HanSuDung > bay && l.HanSuDung <= han).ToListAsync();
        return ds.OrderBy(l => l.HanSuDung).ThenBy(l => l.Id)
            .Select(l => new LoXaHang(l, l.HanSuDung - bay, l.SanPham.DonGia,
                GiamGiaRule.GiaXaHang(l.SanPham.DonGia, l.TyLeGiamXaHang)))
            .ToList();
    }

    /// <summary>phanTram 0 = bỏ xả hàng (luôn cho phép); &gt; 0 chỉ áp cho lô còn tồn và đang cận date. Có lô sai thì không đổi lô nào.</summary>
    public async Task<int> ThietLapAsync(IReadOnlyCollection<int> loIds, decimal phanTram)
    {
        if (loIds.Count == 0) throw new NghiepVuException("Chưa chọn lô bánh nào.");
        if (phanTram < 0 || phanTram > 100) throw new NghiepVuException("Mức giảm phải từ 0 đến 100%.");
        if (decimal.Round(phanTram, 1) != phanTram) throw new NghiepVuException("Mức giảm chỉ được có tối đa 1 chữ số thập phân.");

        var ids = loIds.Distinct().ToList();
        var los = await db.LoSanPhams.Where(l => ids.Contains(l.Id)).ToListAsync();
        if (los.Count != ids.Count) throw new NghiepVuException("Không tìm thấy lô bánh.");

        var bay = BayGio;
        if (phanTram > 0)
            foreach (var lo in los)
            {
                if (lo.SoLuongTon <= 0) throw new NghiepVuException($"Lô #{lo.Id} đã bán hết.");
                var tt = lo.TrangThaiDate(bay);
                if (tt == TrangThaiDate.HET_HAN)
                    throw new NghiepVuException($"Lô #{lo.Id} đã hết hạn, không thể xả hàng. Hãy lập phiếu hủy.");
                if (tt == TrangThaiDate.CON_HAN)
                    throw new NghiepVuException($"Lô #{lo.Id} chưa cận date (còn hơn {HanSuDungRule.NguongGioBanh} giờ), chưa được xả hàng.");
            }

        foreach (var lo in los) lo.TyLeGiamXaHang = phanTram / 100m;
        await db.LuuNghiepVuAsync("Lô bánh vừa được người khác cập nhật. Hãy tải lại danh sách xả hàng.");
        return los.Count;
    }
}
