using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongNhapKho(int NguyenLieuId, decimal SoLuong, decimal DonGia, DateOnly? HanSuDung);
public record YeuCauXuat(int SanPhamId, int SoLuongBanh, int? ThanhPhanId = null);
public record LoLay(LoNguyenLieu Lo, decimal SoLuong);
public record DongKeHoach(NguyenLieu NguyenLieu, decimal Can, List<LoLay> Lay, decimal Thieu);
public record KeHoachXuat(List<DongKeHoach> Dong) { public bool DuHang => Dong.All(d => d.Thieu == 0); }
public record KetQuaXuat(KeHoachXuat KeHoach, List<string> CanhBaoTon);
public record DongDeXuatNhap(
    int NguyenLieuId,
    string TenGoi,
    string DonViTinh,
    decimal TonHienTai,
    decimal MucCanhBaoTon,
    decimal SoLuongDeXuat,
    decimal? DonGiaNhapGanNhat,
    decimal? ThanhTienUocTinh
)
{
    public string TenNguyenLieu => TenGoi;
}
public record BangDeXuatNhap(decimal TongTienUocTinh, List<DongDeXuatNhap> Dong);

public class KhoService(FriendshipDb db, TimeProvider clock)
{
    DateOnly HomNay => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    /// <summary>UC14: mỗi dòng tạo một lô; có dòng sai thì không lưu dòng nào.</summary>
    public async Task<List<LoNguyenLieu>> NhapKhoAsync(IReadOnlyList<DongNhapKho> dong)
    {
        if (dong.Count == 0) throw new NghiepVuException("Phiếu nhập kho phải có ít nhất một dòng.");
        var ids = dong.Select(d => d.NguyenLieuId).Distinct().ToList();
        var coTrongDanhMuc = await db.NguyenLieus.Where(n => ids.Contains(n.Id)).Select(n => n.Id).ToListAsync();

        var loi = new List<string>();
        for (var i = 0; i < dong.Count; i++)
        {
            var d = dong[i];
            var p = $"Dòng {i + 1}: ";
            if (!coTrongDanhMuc.Contains(d.NguyenLieuId)) loi.Add(p + "nguyên liệu không có trong danh mục (nhờ quản lý thêm).");
            if (d.SoLuong <= 0) loi.Add(p + "số lượng phải lớn hơn 0.");
            else if (!SoLuongRule.HopLe(d.SoLuong)) loi.Add(p + "số lượng chỉ được có tối đa 3 chữ số thập phân.");
            if (d.DonGia <= 0) loi.Add(p + "đơn giá nhập phải lớn hơn 0.");
            else if (!SoLuongRule.HopLe(d.DonGia)) loi.Add(p + "đơn giá chỉ được có tối đa 3 chữ số thập phân.");
            if (d.HanSuDung is null) loi.Add(p + "hạn sử dụng là bắt buộc.");
            else if (d.HanSuDung <= HomNay) loi.Add(p + "hạn sử dụng phải sau ngày hôm nay.");
        }
        if (loi.Count > 0) throw new NghiepVuException(string.Join(" ", loi));

        var los = dong.Select(d => new LoNguyenLieu
        {
            NguyenLieuId = d.NguyenLieuId, NgayNhap = HomNay, HanSuDung = d.HanSuDung!.Value,
            SoLuongNhap = d.SoLuong, SoLuongCon = d.SoLuong, DonGiaNhap = d.DonGia
        }).ToList();
        db.LoNguyenLieus.AddRange(los);
        await db.SaveChangesAsync();
        return los;
    }

    /// <summary>UC12 bước 1–2: nhu cầu = định mức × số lượng, gộp theo nguyên liệu rồi mới phân bổ FEFO (BR05, BR06, BR08). Không đổi dữ liệu.</summary>
    public async Task<KeHoachXuat> LapKeHoachXuatAsync(IReadOnlyList<YeuCauXuat> yeuCau)
    {
        if (yeuCau.Count == 0 || yeuCau.Any(y => y.SoLuongBanh <= 0))
            throw new NghiepVuException("Số lượng bánh phải lớn hơn 0.");

        var spIds = yeuCau.Where(y => !y.ThanhPhanId.HasValue).Select(y => y.SanPhamId).Distinct().ToList();
        var tpIds = yeuCau.Where(y => y.ThanhPhanId.HasValue).Select(y => y.ThanhPhanId!.Value).Distinct().ToList();

        var sps = await db.SanPhams.Include(s => s.DinhLuongs).Where(s => spIds.Contains(s.Id)).ToListAsync();
        var tps = await db.ThanhPhanTrangTris.Include(t => t.DinhLuongs).Where(t => tpIds.Contains(t.Id)).ToListAsync();

        var can = new Dictionary<int, decimal>();
        foreach (var y in yeuCau)
        {
            if (y.ThanhPhanId.HasValue)
            {
                var tp = tps.FirstOrDefault(t => t.Id == y.ThanhPhanId.Value) ?? throw new NghiepVuException("Không tìm thấy thành phần.");
                if (tp.DinhLuongs.Count == 0)
                    throw new NghiepVuException($"Thành phần '{tp.Ten}' chưa có công thức định lượng. Báo quản lý bổ sung.");
                foreach (var dl in tp.DinhLuongs)
                    can[dl.NguyenLieuId] = can.GetValueOrDefault(dl.NguyenLieuId) + dl.DinhMuc * y.SoLuongBanh;
            }
            else
            {
                var sp = sps.FirstOrDefault(s => s.Id == y.SanPhamId) ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");
                if (sp.DinhLuongs.Count == 0)
                    throw new NghiepVuException($"Sản phẩm '{sp.TenBanh}' chưa có công thức BOM. Báo quản lý bổ sung.");
                foreach (var dl in sp.DinhLuongs)
                    can[dl.NguyenLieuId] = can.GetValueOrDefault(dl.NguyenLieuId) + dl.DinhMuc * y.SoLuongBanh;
            }
        }

        var nlIds = can.Keys.ToList();
        var nls = await db.NguyenLieus.Where(n => nlIds.Contains(n.Id)).ToDictionaryAsync(n => n.Id);
        var los = await db.LoNguyenLieus.Where(l => nlIds.Contains(l.NguyenLieuId)).ToListAsync();
        var dong = can.Select(kv =>
        {
            var (lay, thieu) = Fefo.PhanBo(los.Where(l => l.NguyenLieuId == kv.Key), kv.Value, HomNay);
            return new DongKeHoach(nls[kv.Key], kv.Value, lay.Select(x => new LoLay(x.Lo, x.SoLuong)).ToList(), thieu);
        }).OrderBy(d => d.NguyenLieu.TenGoi).ToList();
        return new KeHoachXuat(dong);
    }

    /// <summary>UC12 bước 3: tính lại kế hoạch ở thời điểm xác nhận (không tin số liệu cũ trên màn hình) rồi trừ tồn từng lô.</summary>
    public async Task<KetQuaXuat> XuatAsync(IReadOnlyList<YeuCauXuat> yeuCau)
    {
        var kh = await LapKeHoachXuatAsync(yeuCau);
        if (!kh.DuHang)
            throw new NghiepVuException("Không đủ nguyên liệu còn hạn: "
                + string.Join("; ", kh.Dong.Where(d => d.Thieu > 0)
                    .Select(d => $"{d.NguyenLieu.TenGoi} thiếu {d.Thieu:0.###} {d.NguyenLieu.DonViTinh}")) + ".");

        foreach (var d in kh.Dong)
            foreach (var l in d.Lay)
                l.Lo.SoLuongCon -= l.SoLuong;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            throw new NghiepVuException("Tồn kho vừa được người khác thay đổi. Vui lòng lập lại phiếu xuất.");
        }

        var ids = kh.Dong.Select(d => d.NguyenLieu.Id).ToList();
        var ton = await db.LoNguyenLieus.Where(l => ids.Contains(l.NguyenLieuId))
            .GroupBy(l => l.NguyenLieuId).Select(g => new { g.Key, Tong = g.Sum(l => l.SoLuongCon) })
            .ToDictionaryAsync(x => x.Key, x => x.Tong);
        var canhBao = kh.Dong
            .Where(d => ton.GetValueOrDefault(d.NguyenLieu.Id) < d.NguyenLieu.MucCanhBaoTon)
            .Select(d => $"{d.NguyenLieu.TenGoi} còn {ton.GetValueOrDefault(d.NguyenLieu.Id):0.###} {d.NguyenLieu.DonViTinh}, dưới mức cảnh báo {d.NguyenLieu.MucCanhBaoTon:0.###}.")
            .ToList();
        return new KetQuaXuat(kh, canhBao);
    }

    /// <summary>Đề xuất nhập hàng: liệt kê nguyên liệu có tổng tồn (các lô còn hạn) < mức cảnh báo tồn.</summary>
    public async Task<BangDeXuatNhap> LayDeXuatNhapAsync()
    {
        var homNay = HomNay;
        var nls = await db.NguyenLieus.Include(n => n.Los).AsNoTracking().ToListAsync();
        var danhSach = new List<DongDeXuatNhap>();

        foreach (var nl in nls)
        {
            // Tổng tồn các lô CÒN HẠN (không tính lô hết hạn)
            var tongTon = nl.Los
                .Where(l => l.SoLuongCon > 0 && l.HanSuDung >= homNay)
                .Sum(l => l.SoLuongCon);

            if (tongTon >= nl.MucCanhBaoTon) continue;

            // ponytail: công thức đơn giản, đổi thành định mức riêng nếu cần
            var deXuat = Math.Ceiling(Math.Max(0m, 2 * nl.MucCanhBaoTon - tongTon));

            // Đơn giá nhập gần nhất (nếu có lô)
            var loGanNhat = nl.Los
                .OrderByDescending(l => l.NgayNhap)
                .ThenByDescending(l => l.Id)
                .FirstOrDefault();
            var donGiaGanNhat = loGanNhat?.DonGiaNhap;
            var thanhTien = donGiaGanNhat.HasValue ? deXuat * donGiaGanNhat.Value : (decimal?)null;

            danhSach.Add(new DongDeXuatNhap(
                nl.Id,
                nl.TenGoi,
                nl.DonViTinh,
                tongTon,
                nl.MucCanhBaoTon,
                deXuat,
                donGiaGanNhat,
                thanhTien
            ));
        }

        // Sắp xếp theo mức thiếu nghiêm trọng nhất trước:
        // Tỷ lệ tồn / mức cảnh báo càng thấp thì càng thiếu nghiêm trọng (0% tồn xếp trước);
        // nếu bằng nhau thì thiếu số lượng tuyệt đối nhiều hơn xếp trước.
        var ketQua = danhSach
            .OrderBy(d => d.MucCanhBaoTon > 0 ? d.TonHienTai / d.MucCanhBaoTon : 1m)
            .ThenByDescending(d => d.MucCanhBaoTon - d.TonHienTai)
            .ThenBy(d => d.TenGoi)
            .ToList();

        var tongTien = ketQua.Sum(d => d.ThanhTienUocTinh ?? 0m);
        return new BangDeXuatNhap(tongTien, ketQua);
    }
}
