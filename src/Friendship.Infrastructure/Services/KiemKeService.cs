using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongKiemKeInput(int ChiTietId, decimal SoLuongThucTe, string? LyDo);

/// <summary>UC16, BR26: kiểm kê theo lô; chỉ cập nhật tồn sau khi quản lý duyệt, bằng cách cộng chênh lệch.</summary>
public class KiemKeService(FriendshipDb db, TimeProvider clock)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;

    IQueryable<PhieuKiemKe> ChiTietDayDu => db.PhieuKiemKes
        .Include(p => p.NguoiThucHien).Include(p => p.NguoiDuyet)
        .Include(p => p.ChiTiets).ThenInclude(c => c.LoNguyenLieu).ThenInclude(l => l.NguyenLieu);

    public Task<PhieuKiemKe?> LayAsync(int id) => ChiTietDayDu.FirstOrDefaultAsync(p => p.Id == id);

    public Task<PhieuKiemKe?> PhieuDangMoAsync() => ChiTietDayDu.FirstOrDefaultAsync(p =>
        p.TrangThai == TrangThaiPhieu.NHAP || p.TrangThai == TrangThaiPhieu.CHO_DUYET);

    public Task<List<PhieuKiemKe>> TimAsync(TrangThaiPhieu? trangThai)
    {
        var q = db.PhieuKiemKes.Include(p => p.NguoiThucHien).Include(p => p.NguoiDuyet).AsQueryable();
        if (trangThai is { } tt) q = q.Where(p => p.TrangThai == tt);
        return q.OrderByDescending(p => p.NgayKiemKe).ThenByDescending(p => p.Id).ToListAsync();
    }

    public async Task<PhieuKiemKe> TaoPhieuAsync(int nguoiThucHienId)
    {
        if (await db.PhieuKiemKes.AnyAsync(p => p.TrangThai == TrangThaiPhieu.NHAP || p.TrangThai == TrangThaiPhieu.CHO_DUYET))
            throw new NghiepVuException("Đã có phiếu kiểm kê đang nháp hoặc chờ duyệt. Hãy hoàn tất hoặc hủy phiếu đó trước.");
        var los = await db.LoNguyenLieus.Where(l => l.SoLuongCon > 0)
            .OrderBy(l => l.NguyenLieuId).ThenBy(l => l.HanSuDung).ThenBy(l => l.Id).ToListAsync();
        if (los.Count == 0) throw new NghiepVuException("Không có lô nguyên liệu nào còn tồn để kiểm kê.");

        var p = new PhieuKiemKe
        {
            NguoiThucHienId = nguoiThucHienId, NgayKiemKe = BayGio, TrangThai = TrangThaiPhieu.NHAP,
            ChiTiets = los.Select(l => new ChiTietKiemKe
            {
                LoNguyenLieuId = l.Id, SoLuongHeThong = l.SoLuongCon, SoLuongThucTe = l.SoLuongCon
            }).ToList()
        };
        db.PhieuKiemKes.Add(p);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            throw new NghiepVuException("Vừa có người khác tạo phiếu kiểm kê. Hãy mở phiếu đang có thay vì tạo mới.");
        }
        return p;
    }

    public async Task LuuSoLieuAsync(int phieuId, IReadOnlyList<DongKiemKeInput> dong)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.NHAP, "Chỉ sửa được số liệu của phiếu đang nháp.");
        var cap = dong.Select(d =>
        {
            var ct = p.ChiTiets.FirstOrDefault(c => c.Id == d.ChiTietId)
                ?? throw new NghiepVuException("Dòng kiểm kê không thuộc phiếu này.");
            if (d.SoLuongThucTe < 0) throw new NghiepVuException("Số lượng thực tế không được âm.");
            SoLuongRule.Kiem(d.SoLuongThucTe, "Số lượng thực tế");
            return (ct, d);
        }).ToList();
        // CSDL (CK_ChiTietKiemKe_LyDo) không cho lưu dòng lệch thiếu lý do (BR26): chặn trước khi chạm DB.
        var thieu = cap.Where(x => x.d.SoLuongThucTe != x.ct.SoLuongHeThong && string.IsNullOrWhiteSpace(x.d.LyDo))
            .Select(x => $"{x.ct.LoNguyenLieu.NguyenLieu.TenGoi} (lô #{x.ct.LoNguyenLieuId})").ToList();
        if (thieu.Count > 0)
            throw new NghiepVuException("Các dòng chênh lệch cần nhập lý do: " + string.Join(", ", thieu) + ".");
        foreach (var (ct, d) in cap)
        {
            ct.SoLuongThucTe = d.SoLuongThucTe;
            ct.LyDo = string.IsNullOrWhiteSpace(d.LyDo) ? null : d.LyDo.Trim();
        }
        await LuuAsync();
    }

    public async Task GuiDuyetAsync(int phieuId)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.NHAP, "Chỉ gửi duyệt được phiếu đang nháp.");
        var thieu = p.ChiTiets.Where(c => c.SoLuongThucTe != c.SoLuongHeThong && string.IsNullOrWhiteSpace(c.LyDo))
            .Select(c => $"{c.LoNguyenLieu.NguyenLieu.TenGoi} (lô #{c.LoNguyenLieuId})").ToList();
        if (thieu.Count > 0)
            throw new NghiepVuException("Các dòng chênh lệch cần nhập lý do: " + string.Join(", ", thieu) + ".");
        p.TrangThai = TrangThaiPhieu.CHO_DUYET;
        await LuuAsync();
    }

    public async Task DuyetAsync(int phieuId, int nguoiDuyetId)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.CHO_DUYET, "Chỉ duyệt được phiếu đang chờ duyệt.");
        foreach (var c in p.ChiTiets)
        {
            var moi = c.LoNguyenLieu.SoLuongCon + (c.SoLuongThucTe - c.SoLuongHeThong);
            if (moi < 0)
                throw new NghiepVuException($"Lô #{c.LoNguyenLieuId} của {c.LoNguyenLieu.NguyenLieu.TenGoi} sẽ âm tồn ({moi:0.###}) vì đã xuất thêm trong lúc chờ duyệt. Yêu cầu kho kiểm lại lô này.");
        }
        foreach (var c in p.ChiTiets)
            c.LoNguyenLieu.SoLuongCon += c.SoLuongThucTe - c.SoLuongHeThong;
        p.TrangThai = TrangThaiPhieu.DA_DUYET;
        p.NguoiDuyetId = nguoiDuyetId;
        p.NgayDuyet = BayGio;
        await LuuAsync();
    }

    public async Task TuChoiAsync(int phieuId, int nguoiDuyetId, string? lyDo)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.CHO_DUYET, "Chỉ từ chối được phiếu đang chờ duyệt.");
        if (string.IsNullOrWhiteSpace(lyDo)) throw new NghiepVuException("Phải nhập lý do từ chối.");
        p.TrangThai = TrangThaiPhieu.TU_CHOI;
        p.NguoiDuyetId = nguoiDuyetId;
        p.NgayDuyet = BayGio;
        p.LyDoTuChoi = lyDo.Trim();
        await LuuAsync();
    }

    /// <summary>Hủy phiếu nháp bị bỏ quên để không chặn phiếu mới (UC16 cấm hai phiếu mở cùng lúc).</summary>
    public async Task HuyNhapAsync(int phieuId)
    {
        var p = await LayCanTrangThai(phieuId, TrangThaiPhieu.NHAP, "Chỉ hủy được phiếu đang nháp.");
        db.PhieuKiemKes.Remove(p);
        await LuuAsync();
    }

    async Task<PhieuKiemKe> LayCanTrangThai(int id, TrangThaiPhieu can, string thongBaoSai)
    {
        var p = await ChiTietDayDu.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NghiepVuException("Không tìm thấy phiếu kiểm kê.");
        if (p.TrangThai != can) throw new NghiepVuException(thongBaoSai);
        return p;
    }

    async Task LuuAsync()
    {
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            throw new NghiepVuException("Phiếu hoặc tồn kho vừa được người khác thay đổi. Vui lòng mở lại phiếu.");
        }
    }
}
