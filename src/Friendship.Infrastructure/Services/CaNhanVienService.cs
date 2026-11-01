using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record TongHopDinhLuongCa(
    CaNhanVien Ca,
    List<DongTieuHaoTongHop> ChiTiets,
    int SoPhieuLoi,
    int SoDonDaLam
);

public record DongTieuHaoTongHop(
    int NguyenLieuId,
    string TenNguyenLieu,
    string DonViTinh,
    decimal TongSoLuong
);

public class CaNhanVienService(FriendshipDb db, TimeProvider clock)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;

    /// <summary>
    /// Lấy ca đang mở của nhân viên theo loại ca (mặc định BEP_BANH).
    /// </summary>
    public Task<CaNhanVien?> DangMoAsync(int nhanVienId, LoaiCa loaiCa = LoaiCa.BEP_BANH) =>
        db.CaNhanViens
            .Include(c => c.NhanVien)
            .FirstOrDefaultAsync(c => c.NhanVienId == nhanVienId && c.LoaiCa == loaiCa && c.TrangThai == TrangThaiCa.DANG_MO);

    /// <summary>
    /// Bắt buộc nhân viên phải có ca đang mở, nếu chưa mở sẽ ném exception.
    /// </summary>
    public async Task<CaNhanVien> CanDangMoAsync(int nhanVienId, LoaiCa loaiCa = LoaiCa.BEP_BANH) =>
        await DangMoAsync(nhanVienId, loaiCa)
        ?? throw new NghiepVuException($"Bạn chưa mở ca {loaiCa.ToString().ToLower()}. Vui lòng mở ca làm việc trước khi thực hiện.");

    /// <summary>
    /// Mở ca mới cho nhân viên (chọn ca sáng hoặc ca chiều).
    /// </summary>
    public async Task<CaNhanVien> MoCaAsync(int nhanVienId, LoaiCa loaiCa = LoaiCa.BEP_BANH, BuoiCa buoiCa = BuoiCa.CA_SANG, string? ghiChu = null)
    {
        var nv = await db.NhanViens.FindAsync(nhanVienId) ?? throw new NghiepVuException("Không tìm thấy nhân viên.");
        if (loaiCa == LoaiCa.BEP_BANH && nv.VaiTro != VaiTro.SAN_XUAT && nv.VaiTro != VaiTro.QUAN_LY && nv.VaiTro != VaiTro.QUAN_TRI)
            throw new NghiepVuException("Chỉ nhân viên sản xuất (bếp) mới mở ca bếp.");

        var daMo = await DangMoAsync(nhanVienId, loaiCa);
        if (daMo is not null)
            throw new NghiepVuException($"Bạn đã có ca {loaiCa} đang mở ({daMo.Ma} - {daMo.BuoiCa.TenNgan()} lúc {daMo.GioMoCa:HH:mm dd/MM}).");

        var ca = new CaNhanVien
        {
            NhanVienId = nhanVienId,
            LoaiCa = loaiCa,
            BuoiCa = buoiCa,
            GioMoCa = BayGio,
            TrangThai = TrangThaiCa.DANG_MO,
            GhiChu = ghiChu
        };
        db.CaNhanViens.Add(ca);
        await db.LuuNghiepVuAsync("Bạn đã có ca đang mở.");
        return ca;
    }

    /// <summary>
    /// Tổng hợp định lượng tiêu hao trong ca hiện tại.
    /// </summary>
    public async Task<TongHopDinhLuongCa> TongHopCaAsync(int caId)
    {
        var ca = await db.CaNhanViens
            .Include(c => c.NhanVien)
            .FirstOrDefaultAsync(c => c.Id == caId)
            ?? throw new NghiepVuException("Không tìm thấy ca làm việc.");

        var tieuHaos = await db.TieuHaoDinhLuongCas
            .Include(t => t.NguyenLieu)
            .Where(t => t.CaNhanVienId == caId)
            .ToListAsync();

        var chiTiets = tieuHaos
            .GroupBy(t => new { t.NguyenLieuId, t.NguyenLieu.TenGoi, t.NguyenLieu.DonViTinh })
            .Select(g => new DongTieuHaoTongHop(
                g.Key.NguyenLieuId,
                g.Key.TenGoi,
                g.Key.DonViTinh,
                g.Sum(x => x.SoLuong)
            ))
            .OrderBy(x => x.TenNguyenLieu)
            .ToList();

        var soPhieuLoi = await db.PhieuBaoLoiMeBanhs.CountAsync(p => p.CaNhanVienId == caId && p.TrangThai == TrangThaiPhieu.DA_DUYET);
        var soDon = await db.TieuHaoDinhLuongCas
            .Where(t => t.CaNhanVienId == caId && t.DonHangId.HasValue)
            .Select(t => t.DonHangId!.Value)
            .Distinct()
            .CountAsync();

        return new TongHopDinhLuongCa(ca, chiTiets, soPhieuLoi, soDon);
    }

    /// <summary>
    /// Chốt ca và xuất phiếu định lượng gửi sang kho để đối chiếu.
    /// </summary>
    public async Task<PhieuDinhLuongCa> ChotCaVaXuatPhieuDinhLuongAsync(int caId, int nhanVienId, string? ghiChu = null)
    {
        var ca = await db.CaNhanViens.FindAsync(caId) ?? throw new NghiepVuException("Không tìm thấy ca.");
        if (ca.NhanVienId != nhanVienId)
            throw new NghiepVuException("Bạn chỉ có thể chốt ca của chính mình.");
        if (ca.TrangThai != TrangThaiCa.DANG_MO)
            throw new NghiepVuException("Ca này đã được chốt trước đó.");

        var tongHop = await TongHopCaAsync(caId);

        var phieu = new PhieuDinhLuongCa
        {
            CaNhanVienId = caId,
            NguoiLapId = nhanVienId,
            ThoiGianXuat = BayGio,
            TrangThai = TrangThaiPhieuDinhLuong.CHO_DOI_CHIEU,
            GhiChu = ghiChu,
            ChiTiets = tongHop.ChiTiets.Select(c => new ChiTietDinhLuongCa
            {
                NguyenLieuId = c.NguyenLieuId,
                SoLuongDinhLuong = c.TongSoLuong
            }).ToList()
        };

        ca.GioDongCa = BayGio;
        ca.TrangThai = TrangThaiCa.DA_CHOT;
        if (!string.IsNullOrWhiteSpace(ghiChu))
            ca.GhiChu = string.IsNullOrWhiteSpace(ca.GhiChu) ? ghiChu : $"{ca.GhiChu} | {ghiChu}";

        db.PhieuDinhLuongCas.Add(phieu);
        await db.LuuNghiepVuAsync("Ca làm việc đã được cập nhật ở nơi khác.");
        return phieu;
    }

    /// <summary>
    /// Lấy danh sách các phiếu định lượng đang chờ kho đối chiếu.
    /// </summary>
    public Task<List<PhieuDinhLuongCa>> LayPhieuChoDoiChieuAsync() =>
        db.PhieuDinhLuongCas
            .Include(p => p.CaNhanVien).ThenInclude(c => c.NhanVien)
            .Include(p => p.NguoiLap)
            .Include(p => p.ChiTiets).ThenInclude(c => c.NguyenLieu)
            .Where(p => p.TrangThai == TrangThaiPhieuDinhLuong.CHO_DOI_CHIEU)
            .OrderByDescending(p => p.ThoiGianXuat)
            .ToListAsync();

    /// <summary>
    /// Lấy lịch sử ca làm việc đã chốt.
    /// </summary>
    public Task<List<CaNhanVien>> LayLichSuCaAsync(int? nhanVienId = null, LoaiCa? loaiCa = null)
    {
        var q = db.CaNhanViens
            .Include(c => c.NhanVien)
            .Include(c => c.PhieuDinhLuongs)
            .AsQueryable();
        if (nhanVienId.HasValue) q = q.Where(c => c.NhanVienId == nhanVienId.Value);
        if (loaiCa.HasValue) q = q.Where(c => c.LoaiCa == loaiCa.Value);
        return q.OrderByDescending(c => c.GioMoCa).Take(50).ToListAsync();
    }
}
