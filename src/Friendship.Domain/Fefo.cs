using Friendship.Domain.Entities;

namespace Friendship.Domain;

/// <summary>BR06, BR08: phân bổ lượng cần cho các lô theo HSD tăng dần; bỏ lô hết hạn và lô rỗng.</summary>
public static class Fefo
{
    public static (List<(LoNguyenLieu Lo, decimal SoLuong)> Lay, decimal Thieu) PhanBo(
        IEnumerable<LoNguyenLieu> los, decimal can, DateOnly homNay)
    {
        var lay = new List<(LoNguyenLieu Lo, decimal SoLuong)>();
        var conLai = can;
        foreach (var lo in los.Where(l => l.SoLuongCon > 0 && l.HanSuDung >= homNay)
                              .OrderBy(l => l.HanSuDung).ThenBy(l => l.Id))
        {
            if (conLai <= 0) break;
            var lay1 = Math.Min(lo.SoLuongCon, conLai);
            lay.Add((lo, lay1));
            conLai -= lay1;
        }
        return (lay, conLai);
    }
}
