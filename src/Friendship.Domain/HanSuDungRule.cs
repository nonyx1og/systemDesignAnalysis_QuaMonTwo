namespace Friendship.Domain;

/// <summary>BR07: ngưỡng cận date. Lô nguyên liệu có HSD hôm nay vẫn dùng hết ngày; lô bánh hết hạn khi HSD ≤ bây giờ.</summary>
public static class HanSuDungRule
{
    public const int NguongNgayNguyenLieu = 3;
    public const int NguongGioBanh = 6;

    public static TrangThaiDate NguyenLieu(DateOnly hsd, DateOnly homNay)
    {
        if (hsd < homNay) return TrangThaiDate.HET_HAN;
        return hsd.DayNumber - homNay.DayNumber <= NguongNgayNguyenLieu ? TrangThaiDate.CAN_DATE : TrangThaiDate.CON_HAN;
    }

    public static TrangThaiDate Banh(DateTime hsd, DateTime bayGio)
    {
        if (hsd <= bayGio) return TrangThaiDate.HET_HAN;
        return hsd - bayGio <= TimeSpan.FromHours(NguongGioBanh) ? TrangThaiDate.CAN_DATE : TrangThaiDate.CON_HAN;
    }
}
