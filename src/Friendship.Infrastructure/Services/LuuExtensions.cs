using Friendship.Domain;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public static class LuuExtensions
{
    /// <summary>Lưu một lần. Xung đột đồng thời (RowVersion) hoặc vi phạm ràng buộc (trùng SĐT, tồn âm…) → thông báo nghiệp vụ.</summary>
    public static async Task LuuNghiepVuAsync(this FriendshipDb db, string khiXungDot)
    {
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            // Bỏ các thay đổi dở dang để trang hiện lại đọc đúng trạng thái trong CSDL, không phải bản sửa chưa lưu được.
            db.ChangeTracker.Clear();
            throw new NghiepVuException(khiXungDot);
        }
    }
}
