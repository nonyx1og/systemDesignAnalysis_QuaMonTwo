using System.Text.RegularExpressions;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record KhuyenMaiInput(int? Id, string Ma, string Ten, LoaiGiam Loai, decimal GiaTri, decimal DonToiThieu,
    DateOnly NgayBatDau, DateOnly NgayKetThuc);

public partial class KhuyenMaiService(FriendshipDb db)
{
    [GeneratedRegex(@"^[A-Z0-9_-]{3,20}$")] private static partial Regex MaRegex();

    public Task<KhuyenMai?> LayAsync(int id) => db.KhuyenMais.FirstOrDefaultAsync(x => x.Id == id);

    /// <summary>UC06 bước 1: mã rỗng → không dùng voucher (null); mã không tồn tại → lỗi.</summary>
    public async Task<KhuyenMai?> TimTheoMaAsync(string? ma)
    {
        var m = (ma ?? "").Trim().ToUpperInvariant();
        if (m.Length == 0) return null;
        return await db.KhuyenMais.FirstOrDefaultAsync(x => x.MaKhuyenMai == m)
            ?? throw new NghiepVuException($"Mã voucher '{m}' không tồn tại.");
    }

    public Task<List<KhuyenMai>> TimAsync(string? tuKhoa)
    {
        var q = db.KhuyenMais.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim().ToLower();
            q = q.Where(x => x.MaKhuyenMai.ToLower().Contains(k) || x.TenChuongTrinh.ToLower().Contains(k));
        }
        return q.OrderByDescending(x => x.NgayKetThuc).ThenBy(x => x.MaKhuyenMai).ToListAsync();
    }

    public async Task<KhuyenMai> LuuAsync(KhuyenMaiInput i)
    {
        var ma = (i.Ma ?? "").Trim().ToUpperInvariant();
        var ten = (i.Ten ?? "").Trim();
        if (!MaRegex().IsMatch(ma))
            throw new NghiepVuException("Mã voucher gồm 3–20 ký tự: chữ không dấu, số, gạch ngang hoặc gạch dưới.");
        if (ten.Length == 0) throw new NghiepVuException("Tên chương trình không được để trống.");
        if (i.NgayKetThuc < i.NgayBatDau) throw new NghiepVuException("Ngày kết thúc không được trước ngày bắt đầu.");
        if (i.Loai == LoaiGiam.PHAN_TRAM && (i.GiaTri <= 0 || i.GiaTri > 100))
            throw new NghiepVuException("Giảm theo phần trăm phải lớn hơn 0 và không quá 100.");
        if (i.Loai == LoaiGiam.SO_TIEN && i.GiaTri <= 0)
            throw new NghiepVuException("Số tiền giảm phải lớn hơn 0.");
        if (i.DonToiThieu < 0) throw new NghiepVuException("Giá trị đơn tối thiểu không được âm.");
        SoLuongRule.Kiem(i.GiaTri, "Giá trị giảm");
        SoLuongRule.Kiem(i.DonToiThieu, "Giá trị đơn tối thiểu");
        if (await db.KhuyenMais.AnyAsync(x => x.MaKhuyenMai == ma && x.Id != (i.Id ?? 0)))
            throw new NghiepVuException($"Mã voucher '{ma}' đã tồn tại.");

        KhuyenMai km;
        if (i.Id is { } id)
            km = await db.KhuyenMais.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy voucher.");
        else
        {
            km = new KhuyenMai { DangApDung = true };
            db.KhuyenMais.Add(km);
        }
        km.MaKhuyenMai = ma;
        km.TenChuongTrinh = ten;
        km.LoaiGiam = i.Loai;
        km.GiaTriGiam = i.GiaTri;
        km.GiaTriDonToiThieu = i.DonToiThieu;
        km.NgayBatDau = i.NgayBatDau;
        km.NgayKetThuc = i.NgayKetThuc;
        await db.SaveChangesAsync();
        return km;
    }

    public async Task DatApDungAsync(int id, bool apDung)
    {
        var km = await db.KhuyenMais.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy voucher.");
        km.DangApDung = apDung;
        await db.SaveChangesAsync();
    }
}
