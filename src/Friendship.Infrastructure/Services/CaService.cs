using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record TongHopCa(CaThuNgan Ca, decimal ThuTienMat, decimal HoanTienMat, decimal TienMatHeThong, decimal ChuyenKhoanHeThong, int SoGiaoDich);

/// <summary>UC09: mở ca (A0) và chốt ca.</summary>
public class CaService(FriendshipDb db, TimeProvider clock)
{
    public Task<CaThuNgan?> DangMoAsync(int nhanVienId) =>
        db.CaThuNgans.FirstOrDefaultAsync(c => c.NhanVienId == nhanVienId && c.TrangThai == TrangThaiCa.DANG_MO);

    /// <summary>Ca đã chốt gần nhất của nhân viên (để mở lại biên bản).</summary>
    public Task<CaThuNgan?> CaDaChotGanNhatAsync(int nhanVienId) =>
        db.CaThuNgans.Where(c => c.NhanVienId == nhanVienId && c.TrangThai == TrangThaiCa.DA_CHOT)
            .OrderByDescending(c => c.GioDongCa).ThenByDescending(c => c.Id).FirstOrDefaultAsync();

    /// <summary>BR18, BR19: mọi khoản thu phải gắn một ca đang mở của người thu.</summary>
    public async Task<CaThuNgan> CanDangMoAsync(int nhanVienId) =>
        await DangMoAsync(nhanVienId) ?? throw new NghiepVuException("Bạn chưa mở ca thu ngân. Hãy mở ca trước khi thu tiền.");

    /// <summary>Như <see cref="CanDangMoAsync"/> nhưng đánh dấu dòng ca là đã đổi: lần lưu khoản thu sẽ ghi kèm RowVersion của ca,
    /// nên nếu ca bị chốt (hoặc có khoản thu khác) giữa lúc nạp và lúc lưu thì lần lưu bị từ chối.</summary>
    public async Task<CaThuNgan> CanDangMoDeThuAsync(int nhanVienId)
    {
        var ca = await CanDangMoAsync(nhanVienId);
        db.Entry(ca).Property(x => x.TienDauCa).IsModified = true;
        return ca;
    }

    public async Task<CaThuNgan> MoCaAsync(int nhanVienId, decimal tienDauCa)
    {
        var nv = await db.NhanViens.FindAsync(nhanVienId) ?? throw new NghiepVuException("Không tìm thấy nhân viên.");
        if (nv.VaiTro != VaiTro.BAN_HANG) throw new NghiepVuException("Chỉ nhân viên bán hàng mới mở ca thu ngân.");
        if (tienDauCa < 0) throw new NghiepVuException("Tiền quỹ đầu ca không được âm.");
        SoLuongRule.Kiem(tienDauCa, "Tiền quỹ đầu ca");
        if (await DangMoAsync(nhanVienId) is not null) throw new NghiepVuException("Bạn đã có ca đang mở.");

        var ca = new CaThuNgan { NhanVienId = nhanVienId, GioMoCa = clock.GetLocalNow().DateTime, TienDauCa = tienDauCa };
        db.CaThuNgans.Add(ca);
        await db.LuuNghiepVuAsync("Bạn đã có ca đang mở.");
        return ca;
    }

    /// <summary>UC09 bước 1: tiền mặt hệ thống = đầu ca + thu − hoàn; chuyển khoản hệ thống = tổng chuyển khoản (hoàn đã là số âm).</summary>
    public async Task<TongHopCa> TongHopAsync(int caId)
    {
        var ca = await db.CaThuNgans.FindAsync(caId) ?? throw new NghiepVuException("Không tìm thấy ca.");
        var ds = await db.ThanhToans.Where(t => t.CaThuNganId == caId).Select(t => new { t.PhuongThuc, t.SoTien }).ToListAsync();
        var thu = ds.Where(t => t.PhuongThuc == PhuongThucTT.TIEN_MAT && t.SoTien > 0).Sum(t => t.SoTien);
        var hoan = -ds.Where(t => t.PhuongThuc == PhuongThucTT.TIEN_MAT && t.SoTien < 0).Sum(t => t.SoTien);
        var ck = ds.Where(t => t.PhuongThuc == PhuongThucTT.CHUYEN_KHOAN).Sum(t => t.SoTien);
        return new TongHopCa(ca, thu, hoan, ca.TienDauCa + thu - hoan, ck, ds.Count);
    }

    /// <summary>UC09 bước 2–4: chỉ người mở ca chốt được ca của mình (BR25); chênh lệch ≠ 0 bắt buộc giải trình (BR20).</summary>
    public async Task<CaThuNgan> ChotCaAsync(int nhanVienId, decimal tienMatThucTe, decimal chuyenKhoanThucTe, string? ghiChu)
    {
        var ca = await CanDangMoAsync(nhanVienId);
        if (tienMatThucTe < 0 || chuyenKhoanThucTe < 0) throw new NghiepVuException("Số tiền thực tế không được âm.");
        SoLuongRule.Kiem(tienMatThucTe, "Tiền mặt thực tế");
        SoLuongRule.Kiem(chuyenKhoanThucTe, "Chuyển khoản thực tế");

        var th = await TongHopAsync(ca.Id);
        var ghi = (ghiChu ?? "").Trim();
        if ((tienMatThucTe != th.TienMatHeThong || chuyenKhoanThucTe != th.ChuyenKhoanHeThong) && ghi.Length == 0)
            throw new NghiepVuException($"Có chênh lệch (tiền mặt {Tien.Dong(tienMatThucTe - th.TienMatHeThong)}, chuyển khoản {Tien.Dong(chuyenKhoanThucTe - th.ChuyenKhoanHeThong)}). Hãy nhập giải trình trước khi chốt ca.");

        ca.TienMatHeThong = th.TienMatHeThong;
        ca.ChuyenKhoanHeThong = th.ChuyenKhoanHeThong;
        ca.TienMatThucTe = tienMatThucTe;
        ca.ChuyenKhoanThucTe = chuyenKhoanThucTe;
        ca.GhiChuChenhLech = ghi.Length == 0 ? null : ghi;
        ca.GioDongCa = clock.GetLocalNow().DateTime;
        ca.TrangThai = TrangThaiCa.DA_CHOT;
        await db.LuuNghiepVuAsync("Ca vừa có giao dịch mới hoặc đã được cập nhật. Hãy xem lại số liệu và chốt lại.");
        return ca;
    }
}
