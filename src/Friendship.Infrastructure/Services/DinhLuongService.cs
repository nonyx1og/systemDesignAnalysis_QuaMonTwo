using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongNguyenLieuBoSung(int NguyenLieuId, decimal SoLuong);

public class DinhLuongService(FriendshipDb db, TimeProvider clock)
{
    DateTime BayGio => clock.GetLocalNow().DateTime;

    /// <summary>
    /// Kiểm tra các sản phẩm trong đơn hàng xem có sản phẩm nào chưa có công thức BOM định lượng không.
    /// Trả về danh sách các sản phẩm thiếu định lượng.
    /// </summary>
    public async Task<List<SanPham>> KiemTraSanPhamChuaCoBOMAsync(int donHangId)
    {
        var don = await db.DonHangs
            .Include(d => d.ChiTiets).ThenInclude(c => c.SanPham).ThenInclude(s => s!.DinhLuongs)
            .Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh).ThenInclude(tk => tk!.ThanhPhans).ThenInclude(tp => tp.ThanhPhan).ThenInclude(t => t.DinhLuongs)
            .FirstOrDefaultAsync(d => d.Id == donHangId)
            ?? throw new NghiepVuException("Không tìm thấy đơn hàng.");

        var dsThieu = new List<SanPham>();
        foreach (var c in don.ChiTiets)
        {
            if (c.SanPham != null)
            {
                if (c.SanPham.DinhLuongs.Count == 0 && !dsThieu.Any(x => x.Id == c.SanPham.Id))
                {
                    dsThieu.Add(c.SanPham);
                }
            }
        }
        return dsThieu;
    }

    /// <summary>
    /// Tự động ghi nhận tiêu hao nguyên liệu theo định mức BOM có sẵn khi chuyển trạng thái hoặc làm đơn.
    /// </summary>
    public async Task GhiNhanTieuHaoDonHangAsync(int donHangId, int caNhanVienId)
    {
        var ca = await db.CaNhanViens.FindAsync(caNhanVienId)
            ?? throw new NghiepVuException("Không tìm thấy ca làm việc.");
        if (ca.TrangThai != TrangThaiCa.DANG_MO)
            throw new NghiepVuException("Ca làm việc đã chốt, không thể ghi nhận định lượng.");

        // Kiểm tra xem đơn này đã được tính định lượng trong ca chưa (tránh cộng trùng)
        var daGhi = await db.TieuHaoDinhLuongCas.AnyAsync(t => t.CaNhanVienId == caNhanVienId && t.DonHangId == donHangId && t.Nguon == NguonTieuHao.SAN_PHAM_CO_BOM);
        if (daGhi) return;

        var don = await db.DonHangs
            .Include(d => d.ChiTiets).ThenInclude(c => c.SanPham).ThenInclude(s => s!.DinhLuongs)
            .Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh).ThenInclude(tk => tk!.ThanhPhans).ThenInclude(tp => tp.ThanhPhan).ThenInclude(t => t.DinhLuongs)
            .FirstOrDefaultAsync(d => d.Id == donHangId)
            ?? throw new NghiepVuException("Không tìm thấy đơn hàng.");

        var danhSachTieuHao = new List<TieuHaoDinhLuongCa>();

        foreach (var c in don.ChiTiets)
        {
            if (c.SanPham is { DinhLuongs.Count: > 0 } sp)
            {
                foreach (var dl in sp.DinhLuongs)
                {
                    danhSachTieuHao.Add(new TieuHaoDinhLuongCa
                    {
                        CaNhanVienId = caNhanVienId,
                        NguyenLieuId = dl.NguyenLieuId,
                        SoLuong = dl.DinhMuc * c.SoLuong,
                        Nguon = NguonTieuHao.SAN_PHAM_CO_BOM,
                        DonHangId = donHangId,
                        SanPhamId = sp.Id,
                        ThoiGian = BayGio,
                        GhiChu = $"Đơn {don.Ma} - Bánh: {sp.TenBanh} (x{c.SoLuong})"
                    });
                }
            }
            else if (c.ThietKeBanh is { ThanhPhans.Count: > 0 } tk)
            {
                foreach (var tp in tk.ThanhPhans)
                {
                    if (tp.ThanhPhan?.DinhLuongs != null)
                    {
                        foreach (var dltp in tp.ThanhPhan.DinhLuongs)
                        {
                            danhSachTieuHao.Add(new TieuHaoDinhLuongCa
                            {
                                CaNhanVienId = caNhanVienId,
                                NguyenLieuId = dltp.NguyenLieuId,
                                SoLuong = dltp.DinhMuc * c.SoLuong,
                                Nguon = NguonTieuHao.SAN_PHAM_CO_BOM,
                                DonHangId = donHangId,
                                ThoiGian = BayGio,
                                GhiChu = $"Đơn {don.Ma} - Bánh thiết kế ({tk.MaTraCuu}) - Thành phần: {tp.ThanhPhan.Ten}"
                            });
                        }
                    }
                }
            }
        }

        if (danhSachTieuHao.Count > 0)
        {
            db.TieuHaoDinhLuongCas.AddRange(danhSachTieuHao);
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Nhập phiếu định lượng bổ sung cho sản phẩm chưa có định lượng sẵn trong đơn.
    /// Nếu luuLamBOMMacDinh = true, hệ thống sẽ lưu công thức này vào danh mục SanPham để dùng lại cho các đơn sau.
    /// </summary>
    public async Task NhapDinhLuongBoSungChoDonAsync(
        int donHangId,
        int caNhanVienId,
        int sanPhamId,
        IReadOnlyList<DongNguyenLieuBoSung> danhSachNguyenLieu,
        bool luuLamBOMMacDinh = true)
    {
        var ca = await db.CaNhanViens.FindAsync(caNhanVienId)
            ?? throw new NghiepVuException("Không tìm thấy ca làm việc.");
        if (ca.TrangThai != TrangThaiCa.DANG_MO)
            throw new NghiepVuException("Ca làm việc đã chốt.");

        var don = await db.DonHangs
            .Include(d => d.ChiTiets)
            .FirstOrDefaultAsync(d => d.Id == donHangId)
            ?? throw new NghiepVuException("Không tìm thấy đơn hàng.");

        var sp = await db.SanPhams
            .Include(s => s.DinhLuongs)
            .FirstOrDefaultAsync(s => s.Id == sanPhamId)
            ?? throw new NghiepVuException("Không tìm thấy sản phẩm.");

        var ct = don.ChiTiets.FirstOrDefault(c => c.SanPhamId == sanPhamId)
            ?? throw new NghiepVuException("Sản phẩm không có trong đơn hàng.");

        if (danhSachNguyenLieu.Count == 0 || danhSachNguyenLieu.All(d => d.SoLuong <= 0))
            throw new NghiepVuException("Phiếu định lượng bổ sung phải có ít nhất 1 nguyên liệu với số lượng > 0.");

        var hopLe = danhSachNguyenLieu.Where(d => d.SoLuong > 0).ToList();

        // 1. Thêm vào tiêu hao ca
        foreach (var nl in hopLe)
        {
            db.TieuHaoDinhLuongCas.Add(new TieuHaoDinhLuongCa
            {
                CaNhanVienId = caNhanVienId,
                NguyenLieuId = nl.NguyenLieuId,
                SoLuong = nl.SoLuong,
                Nguon = NguonTieuHao.PHIEU_BO_SUNG,
                DonHangId = donHangId,
                SanPhamId = sanPhamId,
                ThoiGian = BayGio,
                GhiChu = $"Bổ sung định lượng - Bánh: {sp.TenBanh} (x{ct.SoLuong})"
            });
        }

        // 2. Nếu chọn lưu làm BOM mặc định và sản phẩm chưa có BOM
        if (luuLamBOMMacDinh)
        {
            var soLuongBanh = ct.SoLuong > 0 ? (decimal)ct.SoLuong : 1m;
            foreach (var nl in hopLe)
            {
                var dinhMucDonVi = Math.Round(nl.SoLuong / soLuongBanh, 3);
                var dlCu = sp.DinhLuongs.FirstOrDefault(d => d.NguyenLieuId == nl.NguyenLieuId);
                if (dlCu != null)
                {
                    dlCu.DinhMuc = dinhMucDonVi;
                }
                else
                {
                    sp.DinhLuongs.Add(new DinhLuong
                    {
                        SanPhamId = sp.Id,
                        NguyenLieuId = nl.NguyenLieuId,
                        DinhMuc = dinhMucDonVi
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Ghi nhận tiêu hao định lượng khi bếp ra lò một mẻ bánh bán lẻ.
    /// </summary>
    public async Task GhiNhanTieuHaoMeBanhAsync(int caNhanVienId, int sanPhamId, int soLuong)
    {
        var ca = await db.CaNhanViens.FindAsync(caNhanVienId);
        if (ca == null || ca.TrangThai != TrangThaiCa.DANG_MO) return;

        var sp = await db.SanPhams.Include(s => s.DinhLuongs).FirstOrDefaultAsync(s => s.Id == sanPhamId);
        if (sp == null || sp.DinhLuongs.Count == 0) return;

        foreach (var dl in sp.DinhLuongs)
        {
            db.TieuHaoDinhLuongCas.Add(new TieuHaoDinhLuongCa
            {
                CaNhanVienId = caNhanVienId,
                NguyenLieuId = dl.NguyenLieuId,
                SoLuong = dl.DinhMuc * soLuong,
                Nguon = NguonTieuHao.SAN_PHAM_CO_BOM,
                SanPhamId = sanPhamId,
                ThoiGian = BayGio,
                GhiChu = $"Mẻ bánh bán lẻ: {sp.TenBanh} (x{soLuong})"
            });
        }
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Bếp tạo phiếu báo lỗi mẻ bánh gửi quản lý duyệt.
    /// </summary>
    public async Task<PhieuBaoLoiMeBanh> TaoPhieuBaoLoiAsync(
        int caNhanVienId,
        int nhanVienLapId,
        int? sanPhamId,
        int soLuongHong,
        string lyDo,
        IReadOnlyList<DongNguyenLieuBoSung> chiTiets)
    {
        var ca = await db.CaNhanViens.FindAsync(caNhanVienId)
            ?? throw new NghiepVuException("Không tìm thấy ca làm việc.");
        if (ca.TrangThai != TrangThaiCa.DANG_MO)
            throw new NghiepVuException("Ca làm việc này đã chốt.");

        if (soLuongHong <= 0) throw new NghiepVuException("Số lượng bánh lỗi/hỏng phải > 0.");
        if (string.IsNullOrWhiteSpace(lyDo)) throw new NghiepVuException("Vui lòng ghi rõ lý do mẻ bánh bị lỗi.");

        var phieu = new PhieuBaoLoiMeBanh
        {
            CaNhanVienId = caNhanVienId,
            NhanVienLapId = nhanVienLapId,
            SanPhamId = sanPhamId,
            SoLuongHong = soLuongHong,
            LyDoLoi = lyDo.Trim(),
            TrangThai = TrangThaiPhieu.CHO_DUYET,
            NgayLap = BayGio,
            ChiTiets = chiTiets.Where(c => c.SoLuong > 0).Select(c => new ChiTietBaoLoi
            {
                NguyenLieuId = c.NguyenLieuId,
                SoLuong = c.SoLuong
            }).ToList()
        };

        if (phieu.ChiTiets.Count == 0)
            throw new NghiepVuException("Cần khai báo ít nhất một nguyên liệu tiêu hao bị hao phí do mẻ lỗi.");

        db.PhieuBaoLoiMeBanhs.Add(phieu);
        await db.SaveChangesAsync();
        return phieu;
    }

    /// <summary>
    /// Gợi ý nguyên liệu tiêu hao theo BOM của sản phẩm nhân với số lượng hỏng.
    /// </summary>
    public async Task<List<DongNguyenLieuBoSung>> GoiYDinhLuongMeLoiAsync(int sanPhamId, int soLuongHong)
    {
        var sp = await db.SanPhams.Include(s => s.DinhLuongs).FirstOrDefaultAsync(s => s.Id == sanPhamId);
        if (sp == null || sp.DinhLuongs.Count == 0) return [];

        return sp.DinhLuongs.Select(dl => new DongNguyenLieuBoSung(
            dl.NguyenLieuId,
            dl.DinhMuc * soLuongHong
        )).ToList();
    }

    /// <summary>
    /// Quản lý duyệt hoặc từ chối phiếu báo lỗi mẻ bánh.
    /// Nếu duyệt: tự động cộng định lượng tiêu hao vào ca làm việc của bếp.
    /// </summary>
    public async Task DuyetPhieuBaoLoiAsync(int phieuId, int nguoiDuyetId, bool dongY, string? ghiChuQuanLy)
    {
        var phieu = await db.PhieuBaoLoiMeBanhs
            .Include(p => p.ChiTiets)
            .Include(p => p.SanPham)
            .Include(p => p.CaNhanVien)
            .FirstOrDefaultAsync(p => p.Id == phieuId)
            ?? throw new NghiepVuException("Không tìm thấy phiếu báo lỗi mẻ bánh.");

        if (phieu.TrangThai != TrangThaiPhieu.CHO_DUYET)
            throw new NghiepVuException("Phiếu này đã được xử lý trước đó.");

        phieu.NguoiDuyetId = nguoiDuyetId;
        phieu.NgayDuyet = BayGio;
        phieu.GhiChuQuanLy = ghiChuQuanLy;

        if (dongY)
        {
            phieu.TrangThai = TrangThaiPhieu.DA_DUYET;

            // Cộng vào định lượng tiêu hao của ca
            var spTen = phieu.SanPham?.TenBanh ?? "Mẻ bánh";
            foreach (var ct in phieu.ChiTiets)
            {
                db.TieuHaoDinhLuongCas.Add(new TieuHaoDinhLuongCa
                {
                    CaNhanVienId = phieu.CaNhanVienId,
                    NguyenLieuId = ct.NguyenLieuId,
                    SoLuong = ct.SoLuong,
                    Nguon = NguonTieuHao.ME_LOI_DUYET,
                    PhieuBaoLoiId = phieu.Id,
                    SanPhamId = phieu.SanPhamId,
                    ThoiGian = BayGio,
                    GhiChu = $"Mẻ bánh lỗi ({phieu.Ma}) - {spTen} hỏng x{phieu.SoLuongHong}: {phieu.LyDoLoi}"
                });
            }
        }
        else
        {
            phieu.TrangThai = TrangThaiPhieu.TU_CHOI;
        }

        await db.LuuNghiepVuAsync("Phiếu vừa được xử lý ở nơi khác.");
    }

    /// <summary>
    /// Lấy danh sách phiếu báo lỗi theo trạng thái hoặc lọc theo ca.
    /// </summary>
    public Task<List<PhieuBaoLoiMeBanh>> LayDanhSachPhieuLoiAsync(TrangThaiPhieu? trangThai = null, int? caId = null)
    {
        var q = db.PhieuBaoLoiMeBanhs
            .Include(p => p.SanPham)
            .Include(p => p.NhanVienLap)
            .Include(p => p.NguoiDuyet)
            .Include(p => p.ChiTiets).ThenInclude(c => c.NguyenLieu)
            .Include(p => p.CaNhanVien)
            .AsQueryable();

        if (trangThai.HasValue) q = q.Where(p => p.TrangThai == trangThai.Value);
        if (caId.HasValue) q = q.Where(p => p.CaNhanVienId == caId.Value);

        return q.OrderByDescending(p => p.NgayLap).ToListAsync();
    }
}
