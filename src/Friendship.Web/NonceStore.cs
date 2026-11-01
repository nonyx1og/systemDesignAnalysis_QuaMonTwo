using System.Collections.Concurrent;

namespace Friendship.Web;

/// <summary>Mã xác nhận dùng một lần cho thao tác ghi tiền. Gỡ mã là nguyên tử nên bấm hai lần hoặc gửi lại form chỉ một lần thành công.</summary>
public class NonceStore(TimeProvider clock)
{
    static readonly TimeSpan Han = TimeSpan.FromMinutes(30);
    readonly ConcurrentDictionary<string, (int UserId, DateTime Het)> _ds = new();

    DateTime Bay => clock.GetUtcNow().UtcDateTime;

    public string Cap(int userId)
    {
        var bay = Bay;
        foreach (var k in _ds.Where(x => x.Value.Het < bay).Select(x => x.Key)) _ds.TryRemove(k, out _);
        var n = Guid.NewGuid().ToString("N");
        _ds[n] = (userId, bay + Han);
        return n;
    }

    /// <summary>Dùng mã: đúng người, còn hạn, và mã bị gỡ ngay dù thành công hay không.</summary>
    public bool Dung(string? nonce, int userId)
    {
        if (string.IsNullOrEmpty(nonce) || !_ds.TryRemove(nonce, out var v)) return false;
        return v.UserId == userId && v.Het >= Bay;
    }
}
