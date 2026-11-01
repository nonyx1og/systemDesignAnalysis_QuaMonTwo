using Friendship.Domain;

namespace Friendship.Infrastructure.Services;

/// <summary>Cột decimal ở DB là (18,3): số có hơn 3 chữ số thập phân sẽ bị làm tròn âm thầm (0,0004 → 0; 0,0005 → 0,001).</summary>
public static class SoLuongRule
{
    const decimal GioiHan = 1_000_000_000_000m;

    public static bool HopLe(decimal v) => decimal.Round(v, 3) == v && Math.Abs(v) < GioiHan;

    public static void Kiem(decimal v, string ten)
    {
        if (decimal.Round(v, 3) != v) throw new NghiepVuException($"{ten} chỉ được có tối đa 3 chữ số thập phân.");
        if (Math.Abs(v) >= GioiHan) throw new NghiepVuException($"{ten} quá lớn.");
    }
}
