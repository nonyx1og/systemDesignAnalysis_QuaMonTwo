using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DinhLuongInput(int NguyenLieuId, decimal DinhMuc);

public record SanPhamInput(int? Id, string TenBanh, string KichThuoc, decimal DonGia, int HanSuDungGio,
    IReadOnlyList<DinhLuongInput> Bom, string? HinhAnh = null);

public record GiaVonUocTinh(decimal GiaVon, List<string> ThieuGiaNhap);

public class SanPhamService(FriendshipDb db)
{
    public Task<SanPham?> LayAsync(int id) => db.SanPhams
        .Include(s => s.DinhLuongs).ThenInclude(d => d.NguyenLieu)
        .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<SanPham>> TimAsync(string? tuKhoa, bool? dangKinhDoanh)
    {
        var q = db.SanPhams.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim().ToLower();
            q = q.Where(x => x.TenBanh.ToLower().Contains(k));
        }
        if (dangKinhDoanh is { } kd) q = q.Where(x => x.DangKinhDoanh == kd);
        return q.OrderBy(x => x.TenBanh).ThenBy(x => x.KichThuoc).ToListAsync();
    }

    public async Task<SanPham> LuuAsync(SanPhamInput i)
    {
        var ten = (i.TenBanh ?? "").Trim();
        var kichThuoc = (i.KichThuoc ?? "").Trim();
        if (ten.Length == 0) throw new NghiepVuException("Tên bánh không được để trống.");
        if (kichThuoc.Length == 0) throw new NghiepVuException("Kích thước không được để trống.");
        if (i.DonGia <= 0) throw new NghiepVuException("Đơn giá phải lớn hơn 0.");
        SoLuongRule.Kiem(i.DonGia, "Đơn giá");
        if (i.HanSuDungGio <= 0) throw new NghiepVuException("Hạn sử dụng (giờ) phải lớn hơn 0.");
        if (i.Bom is null || i.Bom.Count == 0)
            throw new NghiepVuException("Sản phẩm phải có ít nhất một nguyên liệu trong công thức.");
        if (i.Bom.Any(b => b.DinhMuc <= 0)) throw new NghiepVuException("Định mức mỗi nguyên liệu phải lớn hơn 0.");
        foreach (var b in i.Bom) SoLuongRule.Kiem(b.DinhMuc, "Định mức");
        if (i.Bom.GroupBy(b => b.NguyenLieuId).Any(g => g.Count() > 1))
            throw new NghiepVuException("Một nguyên liệu không được khai báo hai lần trong công thức.");
        var nlIds = i.Bom.Select(b => b.NguyenLieuId).ToList();
        if (await db.NguyenLieus.CountAsync(n => nlIds.Contains(n.Id)) != nlIds.Count)
            throw new NghiepVuException("Có nguyên liệu không tồn tại trong danh mục.");

        SanPham sp;
        if (i.Id is { } id)
        {
            sp = await db.SanPhams.Include(s => s.DinhLuongs).FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
            if (sp.DangKinhDoanh) await KiemTrungAsync(ten, kichThuoc, sp.Id);
        }
        else
        {
            await KiemTrungAsync(ten, kichThuoc, 0);
            sp = new SanPham { DangKinhDoanh = true };
            db.SanPhams.Add(sp);
        }
        sp.TenBanh = ten;
        sp.KichThuoc = kichThuoc;
        sp.DonGia = i.DonGia;
        sp.HanSuDungGio = i.HanSuDungGio;
        sp.HinhAnh = string.IsNullOrWhiteSpace(i.HinhAnh) ? null : i.HinhAnh.Trim();

        foreach (var cu in sp.DinhLuongs.Where(d => !nlIds.Contains(d.NguyenLieuId)).ToList())
            db.DinhLuongs.Remove(cu);
        foreach (var b in i.Bom)
        {
            var cu = sp.DinhLuongs.FirstOrDefault(d => d.NguyenLieuId == b.NguyenLieuId);
            if (cu is null) sp.DinhLuongs.Add(new DinhLuong { NguyenLieuId = b.NguyenLieuId, DinhMuc = b.DinhMuc });
            else cu.DinhMuc = b.DinhMuc;
        }
        await db.SaveChangesAsync();
        return sp;
    }

    public async Task DatKinhDoanhAsync(int id, bool kinhDoanh)
    {
        var sp = await db.SanPhams.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
        if (kinhDoanh && !sp.DangKinhDoanh) await KiemTrungAsync(sp.TenBanh, sp.KichThuoc, sp.Id);
        sp.DangKinhDoanh = kinhDoanh;
        await db.SaveChangesAsync();
    }

    /// <summary>UC19 bước 3: định mức × đơn giá nhập của lô gần nhất; nguyên liệu chưa có lô nào được liệt kê riêng.</summary>
    public async Task<GiaVonUocTinh> GiaVonAsync(int sanPhamId)
    {
        var sp = await LayAsync(sanPhamId) ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
        var ids = sp.DinhLuongs.Select(d => d.NguyenLieuId).ToList();
        var lo = await db.LoNguyenLieus.Where(l => ids.Contains(l.NguyenLieuId))
            .OrderByDescending(l => l.NgayNhap).ThenByDescending(l => l.Id).ToListAsync();
        decimal tong = 0;
        var thieu = new List<string>();
        foreach (var d in sp.DinhLuongs)
        {
            var gan = lo.FirstOrDefault(l => l.NguyenLieuId == d.NguyenLieuId);
            if (gan is null) thieu.Add(d.NguyenLieu.TenGoi);
            else tong += d.DinhMuc * gan.DonGiaNhap;
        }
        return new GiaVonUocTinh(tong, thieu);
    }

    async Task KiemTrungAsync(string ten, string kichThuoc, int boQuaId)
    {
        var t = ten.ToLower();
        var k = kichThuoc.ToLower();
        if (await db.SanPhams.AnyAsync(s => s.DangKinhDoanh && s.Id != boQuaId
                && s.TenBanh.ToLower() == t && s.KichThuoc.ToLower() == k))
            throw new NghiepVuException($"Sản phẩm '{ten}' ({kichThuoc}) đã có trong danh sách đang kinh doanh.");
    }
}
