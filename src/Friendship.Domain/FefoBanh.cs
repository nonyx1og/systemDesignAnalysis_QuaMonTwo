using Friendship.Domain.Entities;

namespace Friendship.Domain;

/// <summary>BR06, BR08: phân bổ số bánh cần bán cho các lô còn hạn theo HSD tăng dần; bỏ lô hết hạn và lô rỗng.</summary>
public static class FefoBanh
{
    public static (List<(LoSanPham Lo, int SoLuong)> Lay, int Thieu) PhanBo(IEnumerable<LoSanPham> los, int can, DateTime bayGio)
    {
        var lay = new List<(LoSanPham Lo, int SoLuong)>();
        var conLai = can;
        foreach (var lo in los.Where(l => l.SoLuongTon > 0 && l.HanSuDung > bayGio).OrderBy(l => l.HanSuDung).ThenBy(l => l.Id))
        {
            if (conLai <= 0) break;
            var n = Math.Min(lo.SoLuongTon, conLai);
            lay.Add((lo, n));
            conLai -= n;
        }
        return (lay, conLai);
    }
}
