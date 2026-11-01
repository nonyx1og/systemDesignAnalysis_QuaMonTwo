using System.Collections.Concurrent;

namespace Friendship.Infrastructure.Services;

/// <summary>Khóa tạm tên đăng nhập sau 5 lần sai. Lưu trong bộ nhớ (singleton).</summary>
// ponytail: trạng thái trong RAM, mất khi khởi động lại và không chia sẻ giữa nhiều máy chủ; chuyển sang bảng DB nếu chạy nhiều instance.
public class LoginThrottle(Func<DateTime>? now = null)
{
    const int ToiDa = 5;
    static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(5);
    readonly Func<DateTime> _now = now ?? (() => DateTime.UtcNow);
    readonly ConcurrentDictionary<string, (int Dem, DateTime KhoaDen)> _tt = new();

    public bool IsLocked(string user) => _tt.TryGetValue(user, out var v) && v.KhoaDen > _now();

    public void Fail(string user) => _tt.AddOrUpdate(user, (1, default), (_, v) =>
        v.Dem + 1 >= ToiDa ? (0, _now() + ThoiGianKhoa) : (v.Dem + 1, v.KhoaDen));

    public void Reset(string user) => _tt.TryRemove(user, out _);
}
