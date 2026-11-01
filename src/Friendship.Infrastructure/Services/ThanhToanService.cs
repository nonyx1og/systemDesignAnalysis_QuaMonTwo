using Friendship.Domain;
using Friendship.Domain.Entities;

namespace Friendship.Infrastructure.Services;

public record PhieuThu(ThanhToan ThanhToan, decimal TienThoi);

/// <summary>UC05 (được include bởi UC02, UC03, UC04). Chỉ dựng khoản thu và thêm vào đơn; người gọi lưu cùng đơn để nguyên tử.</summary>
public class ThanhToanService(CaService ca, TimeProvider clock)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;

    public async Task<PhieuThu> GhiNhanAsync(DonHang don, LoaiThanhToan loai, PhuongThucTT phuongThuc,
        decimal soTien, decimal? tienKhachDua, int nhanVienId)
    {
        if (loai == LoaiThanhToan.HOAN_COC) throw new InvalidOperationException("Hoàn cọc dùng GhiHoanCocAsync.");
        if (!Enum.IsDefined(phuongThuc)) throw new NghiepVuException("Phương thức thanh toán không hợp lệ.");
        if (soTien <= 0) throw new NghiepVuException("Số tiền thu phải lớn hơn 0.");
        SoLuongRule.Kiem(soTien, "Số tiền thu");
        var thoi = 0m;
        if (phuongThuc == PhuongThucTT.TIEN_MAT)
        {
            if (tienKhachDua is null || tienKhachDua < soTien)
                throw new NghiepVuException($"Tiền khách đưa ({Tien.Dong(tienKhachDua ?? 0)}) chưa đủ {Tien.Dong(soTien)}.");
            thoi = tienKhachDua.Value - soTien;
        }
        var caHienTai = await ca.CanDangMoDeThuAsync(nhanVienId);
        var tt = new ThanhToan { CaThuNgan = caHienTai, Loai = loai, PhuongThuc = phuongThuc, SoTien = soTien, ThoiGian = BayGio };
        don.ThanhToans.Add(tt);
        return new PhieuThu(tt, thoi);
    }


    /// <summary>Ghi nhận nhiều dòng thanh toán cho đơn bán lẻ. Tổng các dòng phải bằng TongThanhToan của đơn.</summary>
    public async Task<decimal> GhiNhanNhieuAsync(DonHang don, LoaiThanhToan loai,
        IReadOnlyList<DongThanhToan> dongThanhToans, int nhanVienId)
    {
        if (loai == LoaiThanhToan.HOAN_COC) throw new InvalidOperationException("Hoàn cọc dùng GhiHoanCocAsync.");
        if (dongThanhToans.Count == 0) throw new NghiepVuException("Chưa có phương thức thanh toán.");

        foreach (var d in dongThanhToans)
        {
            if (!Enum.IsDefined(d.PhuongThuc)) throw new NghiepVuException("Phương thức thanh toán không hợp lệ.");
            if (d.SoTien <= 0) throw new NghiepVuException("Số tiền thanh toán phải lớn hơn 0.");
            SoLuongRule.Kiem(d.SoTien, "Số tiền thanh toán");
        }

        var tong = dongThanhToans.Sum(d => d.SoTien);
        if (tong < don.TongThanhToan)
            throw new NghiepVuException($"Tổng tiền thanh toán ({Tien.Dong(tong)}) chưa đủ {Tien.Dong(don.TongThanhToan)}.");
        if (tong > don.TongThanhToan)
            throw new NghiepVuException($"Tổng tiền thanh toán ({Tien.Dong(tong)}) vượt quá tổng thanh toán ({Tien.Dong(don.TongThanhToan)}).");

        var thoi = 0m;
        foreach (var d in dongThanhToans)
        {
            if (d.PhuongThuc == PhuongThucTT.TIEN_MAT)
            {
                if (d.TienKhachDua is null || d.TienKhachDua < d.SoTien)
                    throw new NghiepVuException($"Tiền khách đưa ({Tien.Dong(d.TienKhachDua ?? 0)}) chưa đủ {Tien.Dong(d.SoTien)}.");
                thoi += d.TienKhachDua.Value - d.SoTien;
            }
        }

        var caHienTai = await ca.CanDangMoDeThuAsync(nhanVienId);
        foreach (var d in dongThanhToans)
        {
            var tt = new ThanhToan
            {
                CaThuNgan = caHienTai,
                Loai = loai,
                PhuongThuc = d.PhuongThuc,
                SoTien = d.SoTien,
                ThoiGian = BayGio
            };
            don.ThanhToans.Add(tt);
        }
        return thoi;
    }

    /// <summary>Hoàn 100% tiền đơn bán lẻ đã thanh toán theo từng phương thức đã thu, lưu số âm, gắn ca đang mở của người trả.</summary>
    public async Task<List<ThanhToan>> GhiHoanTienBanLeAsync(DonHang don, int nhanVienId)
    {
        var daThu = don.ThanhToans.Where(t => t.SoTien > 0).ToList();
        var tong = daThu.Sum(t => t.SoTien);
        if (tong <= 0) return [];
        var caHienTai = await ca.CanDangMoDeThuAsync(nhanVienId);
        var danhSach = new List<ThanhToan>();
        foreach (var g in daThu.GroupBy(t => t.PhuongThuc))
        {
            var tt = new ThanhToan
            {
                CaThuNgan = caHienTai,
                Loai = LoaiThanhToan.HOAN_TIEN,
                PhuongThuc = g.Key,
                SoTien = -g.Sum(t => t.SoTien),
                ThoiGian = BayGio
            };
            don.ThanhToans.Add(tt);
            danhSach.Add(tt);
        }
        return danhSach;
    }


    /// <summary>BR23: hoàn 100% tiền cọc, lưu số âm, gắn ca đang mở của người hủy.</summary>
    public async Task<ThanhToan> GhiHoanCocAsync(DonHang don, int nhanVienId)
    {
        var cocs = don.ThanhToans.Where(t => t.Loai == LoaiThanhToan.COC).ToList();
        var tong = cocs.Sum(t => t.SoTien);
        if (tong <= 0) throw new NghiepVuException("Đơn chưa có tiền cọc để hoàn.");
        var caHienTai = await ca.CanDangMoDeThuAsync(nhanVienId);
        var tt = new ThanhToan
        {
            CaThuNgan = caHienTai, Loai = LoaiThanhToan.HOAN_COC, PhuongThuc = cocs[0].PhuongThuc,
            SoTien = -tong, ThoiGian = BayGio
        };
        don.ThanhToans.Add(tt);
        return tt;
    }
}
