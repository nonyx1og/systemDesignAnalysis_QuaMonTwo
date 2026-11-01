using System.Text.RegularExpressions;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public partial class KhachHangService(FriendshipDb db)
{
    [GeneratedRegex(@"^0[0-9]{9}$")] private static partial Regex SdtRegex();
    [GeneratedRegex(@"[ .\-]")] private static partial Regex PhanCachRegex();

    public static string? ChuanHoaSdt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var d = PhanCachRegex().Replace(s.Trim(), "");
        if (d.StartsWith("+84")) d = "0" + d[3..];
        if (!SdtRegex().IsMatch(d)) throw new NghiepVuException("Số điện thoại gồm 10 chữ số và bắt đầu bằng 0.");
        return d;
    }

    public async Task<KhachHang?> TimAsync(string? sdt)
    {
        var d = ChuanHoaSdt(sdt);
        return d is null ? null : await db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == d);
    }

    /// <summary>Tìm theo SĐT; chưa có thì dựng khách mới (điểm 0) nhưng KHÔNG Add vào context — người gọi Add khi thật sự lưu đơn.</summary>
    public async Task<(KhachHang? KhachHang, bool Moi)> TimHoacTaoAsync(string? sdt, string? ten)
    {
        var d = ChuanHoaSdt(sdt);
        if (d is null) return (null, false);
        var kh = await db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == d);
        if (kh is not null) return (kh, false);
        var t = (ten ?? "").Trim();
        if (t.Length == 0) throw new NghiepVuException("Khách mới: nhập tên khách.");
        return (new KhachHang { TenKhachHang = t, SoDienThoai = d }, true);
    }
}
