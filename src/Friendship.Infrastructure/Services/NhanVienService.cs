using System.Text.RegularExpressions;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record NhanVienInput(string HoTen, string? GioiTinh, DateOnly? NgaySinh, string SoDienThoai,
    string? Email, string TenDangNhap, VaiTro VaiTro);

public partial class NhanVienService(FriendshipDb db)
{
    static readonly PasswordHasher<NhanVien> Hasher = new();

    [GeneratedRegex(@"^0\d{9}$")] private static partial Regex SdtRegex();

    public async Task<NhanVien?> LayAsync(int id) => await db.NhanViens.FindAsync(id);

    public Task<List<NhanVien>> TimAsync(string? tuKhoa, VaiTro? vaiTro)
    {
        var q = db.NhanViens.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim().ToLower();
            q = q.Where(x => x.HoTen.ToLower().Contains(k) || x.TenDangNhap.Contains(k) || x.SoDienThoai.Contains(k));
        }
        if (vaiTro is { } v) q = q.Where(x => x.VaiTro == v);
        return q.OrderBy(x => x.HoTen).ToListAsync();
    }

    public async Task<NhanVien> TaoAsync(NhanVienInput input, string matKhauTam)
    {
        var i = await ChuanHoaAsync(input, boQuaId: 0);
        MatKhauRule.Kiem(matKhauTam);
        var nv = new NhanVien { DangHoatDong = true };
        Gan(nv, i);
        nv.MatKhauHash = Hasher.HashPassword(nv, matKhauTam);
        db.NhanViens.Add(nv);
        await db.SaveChangesAsync();
        return nv;
    }

    public async Task CapNhatAsync(int id, NhanVienInput input, int nguoiThaoTacId)
    {
        var nv = await db.NhanViens.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy nhân viên.");
        var i = await ChuanHoaAsync(input, boQuaId: id);
        if (id == nguoiThaoTacId && i.VaiTro != nv.VaiTro)
            throw new NghiepVuException("Không thể đổi vai trò của chính mình.");
        Gan(nv, i);
        await db.SaveChangesAsync();
    }

    public async Task DatTrangThaiAsync(int id, bool hoatDong, int nguoiThaoTacId)
    {
        if (id == nguoiThaoTacId && !hoatDong)
            throw new NghiepVuException("Không thể khóa chính mình.");
        var nv = await db.NhanViens.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy nhân viên.");
        nv.DangHoatDong = hoatDong;
        await db.SaveChangesAsync();
    }

    public async Task DatLaiMatKhauAsync(int id, string matKhauMoi)
    {
        MatKhauRule.Kiem(matKhauMoi);
        var nv = await db.NhanViens.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy nhân viên.");
        nv.MatKhauHash = Hasher.HashPassword(nv, matKhauMoi);
        await db.SaveChangesAsync();
    }

    static void Gan(NhanVien nv, NhanVienInput i)
    {
        nv.HoTen = i.HoTen; nv.GioiTinh = i.GioiTinh; nv.NgaySinh = i.NgaySinh;
        nv.SoDienThoai = i.SoDienThoai; nv.Email = i.Email; nv.TenDangNhap = i.TenDangNhap; nv.VaiTro = i.VaiTro;
    }

    async Task<NhanVienInput> ChuanHoaAsync(NhanVienInput i, int boQuaId)
    {
        var hoTen = (i.HoTen ?? "").Trim();
        var user = (i.TenDangNhap ?? "").Trim().ToLowerInvariant();
        var sdt = (i.SoDienThoai ?? "").Trim();
        var email = string.IsNullOrWhiteSpace(i.Email) ? null : i.Email.Trim();

        if (hoTen.Length == 0) throw new NghiepVuException("Họ tên không được để trống.");
        if (!SdtRegex().IsMatch(sdt)) throw new NghiepVuException("Số điện thoại gồm 10 số, bắt đầu bằng 0.");
        if (!TenDangNhapRule.HopLe(user))
            throw new NghiepVuException("Tên đăng nhập gồm 3–30 ký tự: chữ thường không dấu, số, dấu chấm hoặc gạch dưới.");
        if (email is not null && !email.Contains('@')) throw new NghiepVuException("Email không hợp lệ.");
        if (await db.NhanViens.AnyAsync(x => x.TenDangNhap == user && x.Id != boQuaId))
            throw new NghiepVuException($"Tên đăng nhập '{user}' đã tồn tại.");

        return i with { HoTen = hoTen, TenDangNhap = user, SoDienThoai = sdt, Email = email };
    }
}
