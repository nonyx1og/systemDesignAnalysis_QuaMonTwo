using System.Text.RegularExpressions;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public partial class KhachHangAuthService(FriendshipDb db)
{
    static readonly PasswordHasher<KhachHang> Hasher = new();

    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex SdtRegex();

    public async Task<KhachHang> DangKyAsync(string hoTen, string soDienThoai, string matKhau, string? email = null, string? diaChi = null)
    {
        var ten = (hoTen ?? "").Trim();
        var sdt = (soDienThoai ?? "").Trim();
        var mk = (matKhau ?? "").Trim();
        var em = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        var dc = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim();

        if (string.IsNullOrWhiteSpace(ten))
            throw new NghiepVuException("Vui lòng nhập họ và tên.");
        if (!SdtRegex().IsMatch(sdt))
            throw new NghiepVuException("Số điện thoại không hợp lệ (gồm 10 chữ số, bắt đầu bằng số 0).");
        if (mk.Length < 6)
            throw new NghiepVuException("Mật khẩu phải có ít nhất 6 ký tự.");

        var tonTai = await db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt);
        if (tonTai is not null)
        {
            if (!string.IsNullOrEmpty(tonTai.MatKhauHash))
                throw new NghiepVuException("Số điện thoại này đã được đăng ký tài khoản. Vui lòng đăng nhập.");

            // Khách đã có thông tin tại quầy (mua bánh trước đây) -> kích hoạt tài khoản online
            tonTai.TenKhachHang = ten;
            tonTai.Email = em ?? tonTai.Email;
            tonTai.DiaChi = dc ?? tonTai.DiaChi;
            tonTai.MatKhauHash = Hasher.HashPassword(tonTai, mk);
            await db.SaveChangesAsync();
            return tonTai;
        }

        var kh = new KhachHang
        {
            TenKhachHang = ten,
            SoDienThoai = sdt,
            Email = em,
            DiaChi = dc,
            DiemTichLuy = 0,
            NgayTao = DateTime.Now
        };
        kh.MatKhauHash = Hasher.HashPassword(kh, mk);
        db.KhachHangs.Add(kh);
        await db.SaveChangesAsync();
        return kh;
    }

    public async Task<KhachHang> DangNhapAsync(string soDienThoai, string matKhau)
    {
        var sdt = (soDienThoai ?? "").Trim();
        var mk = (matKhau ?? "").Trim();

        if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(mk))
            throw new NghiepVuException("Vui lòng nhập đầy đủ số điện thoại và mật khẩu.");

        var kh = await db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt);
        if (kh is null || string.IsNullOrEmpty(kh.MatKhauHash))
            throw new NghiepVuException("Số điện thoại chưa được đăng ký tài khoản.");

        var kq = Hasher.VerifyHashedPassword(kh, kh.MatKhauHash, mk);
        if (kq == PasswordVerificationResult.Failed)
            throw new NghiepVuException("Mật khẩu không chính xác.");

        return kh;
    }

    public async Task<KhachHang?> LayTheoIdAsync(int id) =>
        await db.KhachHangs.FindAsync(id);

    public async Task<KhachHang?> LayTheoSoDienThoaiAsync(string sdt) =>
        await db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt);
}
