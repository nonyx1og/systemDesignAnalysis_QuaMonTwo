using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public class TaiKhoanService(FriendshipDb db, LoginThrottle throttle)
{
    static readonly PasswordHasher<NhanVien> Hasher = new();
    const string LoiChung = "Sai tên đăng nhập hoặc mật khẩu, hoặc tài khoản đã bị khóa.";

    public async Task<NhanVien> DangNhapAsync(string? tenDangNhap, string? matKhau)
    {
        var user = (tenDangNhap ?? "").Trim().ToLowerInvariant();
        if (!TenDangNhapRule.HopLe(user) || string.IsNullOrEmpty(matKhau)) throw new NghiepVuException(LoiChung);
        if (throttle.IsLocked(user))
            throw new NghiepVuException("Đăng nhập sai quá 5 lần. Vui lòng thử lại sau 5 phút.");

        var nv = await db.NhanViens.FirstOrDefaultAsync(x => x.TenDangNhap == user);
        var hopLe = nv is { DangHoatDong: true }
            && Hasher.VerifyHashedPassword(nv, nv.MatKhauHash, matKhau) != PasswordVerificationResult.Failed;
        if (!hopLe)
        {
            throttle.Fail(user);
            throw new NghiepVuException(LoiChung);
        }
        throttle.Reset(user);
        return nv!;
    }

    public async Task<NhanVien> DoiMatKhauAsync(int nhanVienId, string? cu, string? moi)
    {
        var nv = await db.NhanViens.FindAsync(nhanVienId)
            ?? throw new NghiepVuException("Không tìm thấy tài khoản.");
        if (string.IsNullOrEmpty(cu)
            || Hasher.VerifyHashedPassword(nv, nv.MatKhauHash, cu) == PasswordVerificationResult.Failed)
            throw new NghiepVuException("Mật khẩu hiện tại không đúng.");
        MatKhauRule.Kiem(moi);
        nv.MatKhauHash = Hasher.HashPassword(nv, moi!);
        await db.SaveChangesAsync();
        return nv;
    }
}
