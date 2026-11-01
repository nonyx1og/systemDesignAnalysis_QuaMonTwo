using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record ThanhPhanInput(
    int? Id,
    NhomThanhPhan Nhom,
    string Ten,
    string? AnhMinhHoa,
    decimal GiaBan,
    int SoLuongToiDa,
    int? SanPhamNenId,
    List<DinhLuongInput>? DinhLuong = null);

public class ThanhPhanService(FriendshipDb db)
{
    public Task<ThanhPhanTrangTri?> LayAsync(int id) => db.ThanhPhanTrangTris
        .Include(t => t.SanPhamNen)
        .Include(t => t.DinhLuongs).ThenInclude(d => d.NguyenLieu)
        .FirstOrDefaultAsync(t => t.Id == id);

    public Task<List<ThanhPhanTrangTri>> TimAsync(NhomThanhPhan? nhom = null, bool? dangKinhDoanh = null)
    {
        var q = db.ThanhPhanTrangTris.AsQueryable();
        if (nhom is { } n) q = q.Where(x => x.Nhom == n);
        if (dangKinhDoanh is { } kd) q = q.Where(x => x.DangKinhDoanh == kd);
        return q.Include(x => x.SanPhamNen)
            .OrderBy(x => x.Nhom).ThenBy(x => x.Ten)
            .ToListAsync();
    }

    public async Task<ThanhPhanTrangTri> LuuAsync(ThanhPhanInput i)
    {
        var ten = (i.Ten ?? "").Trim();
        if (ten.Length == 0) throw new NghiepVuException("Tên thành phần không được để trống.");
        if (i.GiaBan < 0) throw new NghiepVuException("Giá bán không được âm.");
        SoLuongRule.Kiem(i.GiaBan, "Giá bán");
        if (i.SoLuongToiDa <= 0) throw new NghiepVuException("Số lượng tối đa phải lớn hơn 0.");

        var bom = i.DinhLuong ?? [];
        if (bom.Any(b => b.DinhMuc <= 0)) throw new NghiepVuException("Định mức mỗi nguyên liệu phải lớn hơn 0.");
        foreach (var b in bom) SoLuongRule.Kiem(b.DinhMuc, "Định mức");
        if (bom.GroupBy(b => b.NguyenLieuId).Any(g => g.Count() > 1))
            throw new NghiepVuException("Một nguyên liệu không được khai báo hai lần trong định lượng.");

        var nlIds = bom.Select(b => b.NguyenLieuId).ToList();
        if (nlIds.Count > 0 && await db.NguyenLieus.CountAsync(n => nlIds.Contains(n.Id)) != nlIds.Count)
            throw new NghiepVuException("Có nguyên liệu không tồn tại trong danh mục.");

        if (i.Nhom is NhomThanhPhan.COT or NhomThanhPhan.VIEN or NhomThanhPhan.TANG && i.SanPhamNenId is null)
            throw new NghiepVuException("Cốt, viền và tầng phụ phải gắn với một bánh nền (kích thước).");
        if (i.SanPhamNenId is { } nenId && !await db.SanPhams.AnyAsync(s => s.Id == nenId))
            throw new NghiepVuException("Sản phẩm nền không tồn tại.");

        ThanhPhanTrangTri tp;
        if (i.Id is { } id)
        {
            tp = await db.ThanhPhanTrangTris.Include(t => t.DinhLuongs).FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NghiepVuException("Không tìm thấy thành phần.");
            if (tp.DangKinhDoanh) await KiemTrungAsync(i.Nhom, ten, tp.Id);
        }
        else
        {
            await KiemTrungAsync(i.Nhom, ten, 0);
            tp = new ThanhPhanTrangTri { DangKinhDoanh = true };
            db.ThanhPhanTrangTris.Add(tp);
        }

        tp.Nhom = i.Nhom;
        tp.Ten = ten;
        tp.AnhMinhHoa = string.IsNullOrWhiteSpace(i.AnhMinhHoa) ? null : i.AnhMinhHoa.Trim();
        tp.GiaBan = i.GiaBan;
        tp.SoLuongToiDa = i.SoLuongToiDa;
        tp.SanPhamNenId = (i.Nhom is NhomThanhPhan.COT or NhomThanhPhan.VIEN or NhomThanhPhan.TANG) ? i.SanPhamNenId : null;

        foreach (var cu in tp.DinhLuongs.Where(d => !nlIds.Contains(d.NguyenLieuId)).ToList())
            db.DinhLuongThanhPhans.Remove(cu);

        foreach (var b in bom)
        {
            var cu = tp.DinhLuongs.FirstOrDefault(d => d.NguyenLieuId == b.NguyenLieuId);
            if (cu is null)
                tp.DinhLuongs.Add(new DinhLuongThanhPhan { NguyenLieuId = b.NguyenLieuId, DinhMuc = b.DinhMuc });
            else
                cu.DinhMuc = b.DinhMuc;
        }

        await db.SaveChangesAsync();
        return tp;
    }

    public async Task DatKinhDoanhAsync(int id, bool kinhDoanh)
    {
        var tp = await db.ThanhPhanTrangTris.FindAsync(id) ?? throw new NghiepVuException("Không tìm thấy thành phần.");
        if (kinhDoanh && !tp.DangKinhDoanh) await KiemTrungAsync(tp.Nhom, tp.Ten, tp.Id);
        tp.DangKinhDoanh = kinhDoanh;
        await db.SaveChangesAsync();
    }

    public async Task<GiaVonUocTinh> GiaVonAsync(int thanhPhanId)
    {
        var tp = await LayAsync(thanhPhanId) ?? throw new NghiepVuException("Không tìm thấy thành phần.");
        var ids = tp.DinhLuongs.Select(d => d.NguyenLieuId).ToList();
        var lo = await db.LoNguyenLieus.Where(l => ids.Contains(l.NguyenLieuId))
            .OrderByDescending(l => l.NgayNhap).ThenByDescending(l => l.Id).ToListAsync();
        decimal tong = 0;
        var thieu = new List<string>();
        foreach (var d in tp.DinhLuongs)
        {
            var gan = lo.FirstOrDefault(l => l.NguyenLieuId == d.NguyenLieuId);
            if (gan is null) thieu.Add(d.NguyenLieu?.TenGoi ?? $"NL #{d.NguyenLieuId}");
            else tong += d.DinhMuc * gan.DonGiaNhap;
        }
        return new GiaVonUocTinh(tong, thieu);
    }

    async Task KiemTrungAsync(NhomThanhPhan nhom, string ten, int boQuaId)
    {
        var t = ten.ToLower();
        if (await db.ThanhPhanTrangTris.AnyAsync(s => s.DangKinhDoanh && s.Id != boQuaId
                && s.Nhom == nhom && s.Ten.ToLower() == t))
            throw new NghiepVuException($"Thành phần '{ten}' đã có trong nhóm {nhom}.");
    }
}
