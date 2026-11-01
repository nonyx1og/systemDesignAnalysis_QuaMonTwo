using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record NguyenLieuInput(int? Id, string TenGoi, string DonViTinh, decimal MucCanhBaoTon);

public record NguyenLieuTon(NguyenLieu NguyenLieu, decimal TongTon)
{
    public bool DuoiMuc => TongTon < NguyenLieu.MucCanhBaoTon;
}

public class NguyenLieuService(FriendshipDb db)
{
    public async Task<NguyenLieu?> LayAsync(int id) => await db.NguyenLieus.FindAsync(id);

    public async Task<List<NguyenLieuTon>> TimAsync(string? tuKhoa)
    {
        var q = db.NguyenLieus.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim().ToLower();
            q = q.Where(x => x.TenGoi.ToLower().Contains(k));
        }
        var ds = await q.OrderBy(x => x.TenGoi).ToListAsync();
        var ids = ds.Select(x => x.Id).ToList();
        var ton = await db.LoNguyenLieus.Where(l => ids.Contains(l.NguyenLieuId))
            .GroupBy(l => l.NguyenLieuId)
            .Select(g => new { g.Key, Tong = g.Sum(l => l.SoLuongCon) })
            .ToDictionaryAsync(x => x.Key, x => x.Tong);
        return ds.Select(n => new NguyenLieuTon(n, ton.GetValueOrDefault(n.Id))).ToList();
    }

    public async Task<NguyenLieu> LuuAsync(NguyenLieuInput i)
    {
        var ten = (i.TenGoi ?? "").Trim();
        var donVi = (i.DonViTinh ?? "").Trim();
        if (ten.Length == 0) throw new NghiepVuException("Tên nguyên liệu không được để trống.");
        if (donVi.Length == 0) throw new NghiepVuException("Đơn vị tính không được để trống.");
        if (i.MucCanhBaoTon < 0) throw new NghiepVuException("Mức cảnh báo tồn không được âm.");
        SoLuongRule.Kiem(i.MucCanhBaoTon, "Mức cảnh báo tồn");

        var tenThuong = ten.ToLower();
        if (await db.NguyenLieus.AnyAsync(x => x.TenGoi.ToLower() == tenThuong && x.Id != (i.Id ?? 0)))
            throw new NghiepVuException($"Nguyên liệu '{ten}' đã tồn tại.");

        NguyenLieu nl;
        if (i.Id is { } id)
        {
            nl = await db.NguyenLieus.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy nguyên liệu.");
            if (nl.DonViTinh != donVi
                && (await db.LoNguyenLieus.AnyAsync(l => l.NguyenLieuId == id)
                    || await db.DinhLuongs.AnyAsync(d => d.NguyenLieuId == id)))
                throw new NghiepVuException("Không đổi được đơn vị tính vì nguyên liệu đã có lô tồn kho hoặc đã nằm trong công thức.");
        }
        else
        {
            nl = new NguyenLieu();
            db.NguyenLieus.Add(nl);
        }
        nl.TenGoi = ten;
        nl.DonViTinh = donVi;
        nl.MucCanhBaoTon = i.MucCanhBaoTon;
        await db.SaveChangesAsync();
        return nl;
    }

    public async Task XoaAsync(int id)
    {
        var nl = await db.NguyenLieus.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy nguyên liệu.");
        if (await db.DinhLuongs.AnyAsync(d => d.NguyenLieuId == id))
            throw new NghiepVuException("Không xóa được: nguyên liệu đang được dùng trong công thức sản phẩm.");
        if (await db.LoNguyenLieus.AnyAsync(l => l.NguyenLieuId == id))
            throw new NghiepVuException("Không xóa được: nguyên liệu đã có lô tồn kho.");
        db.NguyenLieus.Remove(nl);
        await db.SaveChangesAsync();
    }
}
