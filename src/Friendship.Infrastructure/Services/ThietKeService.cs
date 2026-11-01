using System.Text.Json;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure.Services;

public record DongChon(int Id, int SoLuong);
public record DuLieuThietKe(int SanPhamNenId, List<DongChon> ThanhPhan, int? SoNen = null, int? SoTang = null);

public record LuuThietKeInput(
    string? MaTraCuu,
    string HoTen,
    string SoDienThoai,
    DateTime NgayGioLay,
    string DuLieuJson,
    string? ChuTrenBanh,
    string? LoiNhan,
    string AnhThietKe,
    string? AnhInAn);

public class ThietKeService(
    FriendshipDb db,
    TimeProvider clock,
    KhachHangService khach,
    ThanhPhanService thanhPhan,
    SanPhamService sanPham,
    DonDatService donDat)
{
    static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public static string TaoMaTraCuu()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);   // 128 bit ngẫu nhiên đầy đủ
        return Convert.ToBase64String(bytes).Replace('/', '_').Replace('+', '-')[..22];
    }

    public async Task<KetQuaGiaThietKe> TinhGiaAsync(string duLieuJson)
    {
        if (string.IsNullOrWhiteSpace(duLieuJson))
            throw new NghiepVuException("Dữ liệu thiết kế không được để trống.");
        if (System.Text.Encoding.UTF8.GetByteCount(duLieuJson) > ThietKeRule.JsonToiDa)
            throw new NghiepVuException("Dữ liệu thiết kế quá lớn.");

        DuLieuThietKe duLieu;
        try
        {
            duLieu = JsonSerializer.Deserialize<DuLieuThietKe>(duLieuJson, JsonOpts)
                ?? throw new NghiepVuException("Dữ liệu thiết kế không hợp lệ.");
        }
        catch (JsonException)
        {
            throw new NghiepVuException("Dữ liệu thiết kế không đúng định dạng JSON.");
        }

        if (duLieu.SoNen.HasValue && (duLieu.SoNen.Value < 0 || duLieu.SoNen.Value > ThietKeRule.SoNenToiDa))
            throw new NghiepVuException($"Số nến phải từ 0 đến {ThietKeRule.SoNenToiDa}.");

        if (duLieu.SoTang.HasValue && (duLieu.SoTang.Value < 1 || duLieu.SoTang.Value > 2))
            throw new NghiepVuException("Số tầng bánh chỉ hỗ trợ 1 hoặc 2 tầng.");

        var spNen = await db.SanPhams.FirstOrDefaultAsync(s => s.Id == duLieu.SanPhamNenId)
            ?? throw new NghiepVuException("Không tìm thấy bánh nền.");

        if (!spNen.DangKinhDoanh)
            throw new NghiepVuException("Bánh nền đã ngừng kinh doanh.");

        var dsDong = new List<DongThanhPhanGia>();
        var thanhPhanIds = (duLieu.ThanhPhan ?? []).Select(t => t.Id).Distinct().ToList();
        var tps = await db.ThanhPhanTrangTris.Where(t => thanhPhanIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id);

        foreach (var c in duLieu.ThanhPhan ?? [])
        {
            if (!tps.TryGetValue(c.Id, out var tp))
                throw new NghiepVuException($"Không tìm thấy thành phần #{c.Id}.");

            dsDong.Add(new DongThanhPhanGia(
                tp.Id,
                tp.Ten,
                tp.Nhom,
                tp.SanPhamNenId,
                tp.GiaBan,
                tp.SoLuongToiDa,
                tp.DangKinhDoanh,
                c.SoLuong));
        }

        if (duLieu.SoTang == 2 && !dsDong.Any(d => d.Nhom == NhomThanhPhan.TANG))
            throw new NghiepVuException("Thiết kế 2 tầng cần chọn thành phần tầng phụ.");

        if (duLieu.SoTang == 1 && dsDong.Any(d => d.Nhom == NhomThanhPhan.TANG))
            throw new NghiepVuException("Số tầng không khớp với thành phần tầng phụ đã chọn.");

        return TinhGiaThietKe.Tinh(spNen.Id, spNen.DonGia, dsDong);
    }

    public async Task<ThietKeBanh> LuuNhapAsync(LuuThietKeInput i)
    {
        if (string.IsNullOrWhiteSpace(i.HoTen))
            throw new NghiepVuException("Họ tên khách hàng không được để trống.");
        if (string.IsNullOrWhiteSpace(i.SoDienThoai))
            throw new NghiepVuException("Số điện thoại không được để trống.");

        var bayGio = clock.GetLocalNow().DateTime;
        if (i.NgayGioLay <= bayGio)
            throw new NghiepVuException("Thời gian lấy bánh không được ở quá khứ.");

        if ((i.ChuTrenBanh?.Length ?? 0) > ThietKeRule.ChuToiDa)
            throw new NghiepVuException($"Chữ trên bánh tối đa {ThietKeRule.ChuToiDa} ký tự.");
        if ((i.LoiNhan?.Length ?? 0) > ThietKeRule.LoiNhanToiDa)
            throw new NghiepVuException($"Lời nhắn tối đa {ThietKeRule.LoiNhanToiDa} ký tự.");
        if (string.IsNullOrWhiteSpace(i.DuLieuJson))
            throw new NghiepVuException("Dữ liệu thiết kế không được để trống.");
        if (System.Text.Encoding.UTF8.GetByteCount(i.DuLieuJson) > ThietKeRule.JsonToiDa)
            throw new NghiepVuException("Dữ liệu thiết kế quá lớn.");

        var kqGia = await TinhGiaAsync(i.DuLieuJson);
        var duLieu = JsonSerializer.Deserialize<DuLieuThietKe>(i.DuLieuJson, JsonOpts)!;

        var (kh, moi) = await khach.TimHoacTaoAsync(i.SoDienThoai, i.HoTen);
        if (kh is null)
            throw new NghiepVuException("Số điện thoại không hợp lệ.");
        if (moi)
            db.KhachHangs.Add(kh);   // khách đã có: không sửa tên, vì trang công khai ai cũng nhập được sđt bất kỳ

        var thanhPhanIds = duLieu.ThanhPhan.Select(t => t.Id).Distinct().ToList();
        var tps = await db.ThanhPhanTrangTris.Where(t => thanhPhanIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id);

        ThietKeBanh tk;
        if (string.IsNullOrWhiteSpace(i.MaTraCuu))
        {
            tk = new ThietKeBanh
            {
                MaTraCuu = TaoMaTraCuu(),
                KhachHang = kh,
                SanPhamNenId = duLieu.SanPhamNenId,
                DuLieuJson = i.DuLieuJson,
                AnhThietKe = i.AnhThietKe,
                AnhInAn = i.AnhInAn,
                ChuTrenBanh = i.ChuTrenBanh,
                LoiNhan = i.LoiNhan,
                NgayGioLay = i.NgayGioLay,
                TrangThai = TrangThaiThietKe.NHAP,
                GiaTamTinh = kqGia.GiaTamTinh
            };
            foreach (var c in duLieu.ThanhPhan)
            {
                var tp = tps[c.Id];
                tk.ThanhPhans.Add(new ThietKeThanhPhan
                {
                    ThanhPhanId = tp.Id,
                    SoLuong = c.SoLuong,
                    DonGia = tp.GiaBan
                });
            }
            db.ThietKeBanhs.Add(tk);
        }
        else
        {
            tk = await db.ThietKeBanhs
                .Include(x => x.KhachHang)
                .Include(x => x.ThanhPhans)
                .FirstOrDefaultAsync(x => x.MaTraCuu == i.MaTraCuu)
                ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

            if (!ThietKeRule.KhachSuaDuoc(tk.TrangThai))
                throw new NghiepVuException("Thiết kế đang ở trạng thái không thể sửa.");

            tk.KhachHang = kh;
            tk.SanPhamNenId = duLieu.SanPhamNenId;
            tk.DuLieuJson = i.DuLieuJson;
            tk.AnhThietKe = i.AnhThietKe;
            tk.AnhInAn = i.AnhInAn;
            tk.ChuTrenBanh = i.ChuTrenBanh;
            tk.LoiNhan = i.LoiNhan;
            tk.NgayGioLay = i.NgayGioLay;
            tk.GiaTamTinh = kqGia.GiaTamTinh;

            db.ThietKeThanhPhans.RemoveRange(tk.ThanhPhans);
            tk.ThanhPhans.Clear();
            foreach (var c in duLieu.ThanhPhan)
            {
                var tp = tps[c.Id];
                tk.ThanhPhans.Add(new ThietKeThanhPhan
                {
                    ThanhPhanId = tp.Id,
                    SoLuong = c.SoLuong,
                    DonGia = tp.GiaBan
                });
            }
        }

        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Tải lại trang và thử lại.");
        return tk;
    }

    public async Task GuiAsync(string ma)
    {
        var tk = await db.ThietKeBanhs.FirstOrDefaultAsync(x => x.MaTraCuu == ma)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        var bayGio = clock.GetLocalNow().DateTime;
        if (tk.NgayGioLay < bayGio.AddHours(ThietKeRule.GioDatTruocToiThieu))
            throw new NghiepVuException($"Thời gian lấy bánh phải trước ít nhất {ThietKeRule.GioDatTruocToiThieu} giờ.");

        ThietKeRule.Chuyen(tk, TrangThaiThietKe.CHO_DUYET);
        tk.NgayGui = bayGio;
        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Tải lại trang và thử lại.");
    }

    public async Task HuyAsync(string ma)
    {
        var tk = await db.ThietKeBanhs.FirstOrDefaultAsync(x => x.MaTraCuu == ma)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        ThietKeRule.Chuyen(tk, TrangThaiThietKe.KHACH_HUY);
        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Tải lại trang và thử lại.");
    }

    public async Task<ThietKeBanh?> LayTheoMaAsync(string ma)
    {
        await QuetHetHanAsync();
        return await db.ThietKeBanhs
            .Include(x => x.KhachHang)
            .Include(x => x.SanPhamNen)
            .Include(x => x.NguoiDuyet)
            .Include(x => x.DonHang)
            .Include(x => x.ThanhPhans).ThenInclude(t => t.ThanhPhan)
            .FirstOrDefaultAsync(x => x.MaTraCuu == ma);
    }

    public async Task<List<ThietKeBanh>> TimAsync(string sdt, string bonKyTuCuoi)
    {
        // Cần cả sđt lẫn 4 ký tự cuối MÃ TRA CỨU: chỉ biết sđt thì không dò ra được thiết kế (và mã) của khách.
        var d = KhachHangService.ChuanHoaSdt(sdt);
        var duoi = (bonKyTuCuoi ?? "").Trim();
        if (d is null || duoi.Length != 4) return [];

        await QuetHetHanAsync();
        var ds = await db.ThietKeBanhs
            .Include(x => x.KhachHang)
            .Include(x => x.SanPhamNen)
            .Where(x => x.KhachHang.SoDienThoai == d)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
        return ds.Where(x => x.MaTraCuu.EndsWith(duoi, StringComparison.Ordinal)).ToList();
    }

    public async Task<List<ThietKeBanh>> ChoDuyetAsync()
    {
        await QuetHetHanAsync();
        return await db.ThietKeBanhs
            .Include(x => x.KhachHang)
            .Include(x => x.SanPhamNen)
            .Where(x => x.TrangThai == TrangThaiThietKe.CHO_DUYET)
            .OrderBy(x => x.NgayGioLay)
            .ToListAsync();
    }

    public async Task<List<ThietKeBanh>> DaBaoGiaAsync()
    {
        await QuetHetHanAsync();
        var bayGio = clock.GetLocalNow().DateTime;
        return await db.ThietKeBanhs
            .Include(x => x.KhachHang)
            .Include(x => x.SanPhamNen)
            .Where(x => x.TrangThai == TrangThaiThietKe.DA_BAO_GIA && (x.HanBaoGia == null || x.HanBaoGia >= bayGio))
            .OrderBy(x => x.NgayGioLay)
            .ToListAsync();
    }

    public async Task<DonHang> XacNhanCocAsync(int thietKeId, int nhanVienId, decimal tienCoc)
    {
        var tk = await db.ThietKeBanhs.Include(x => x.KhachHang).FirstOrDefaultAsync(x => x.Id == thietKeId)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        var yeuCau = new YeuCauDat(
            nhanVienId,
            tk.KhachHang.SoDienThoai,
            tk.KhachHang.TenKhachHang,
            [new DongDat(tk.SanPhamNenId, 1, tk.ChuTrenBanh, tk.Id)],
            tk.NgayGioLay,
            null,
            0,
            tienCoc,
            PhuongThucTT.CHUYEN_KHOAN,
            null);

        return await donDat.TaoAsync(yeuCau);
    }

    public async Task<List<string>> CanhBaoNguyenLieuAsync(int id)
    {
        var tk = await db.ThietKeBanhs
            .Include(x => x.SanPhamNen).ThenInclude(s => s.DinhLuongs).ThenInclude(d => d.NguyenLieu)
            .Include(x => x.ThanhPhans).ThenInclude(t => t.ThanhPhan).ThenInclude(tp => tp.DinhLuongs).ThenInclude(d => d.NguyenLieu)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        var nhuCau = new Dictionary<int, (NguyenLieu Nl, decimal SoLuong)>();

        foreach (var d in tk.SanPhamNen.DinhLuongs)
        {
            if (!nhuCau.TryGetValue(d.NguyenLieuId, out var hien))
                nhuCau[d.NguyenLieuId] = (d.NguyenLieu, d.DinhMuc);
            else
                nhuCau[d.NguyenLieuId] = (d.NguyenLieu, hien.SoLuong + d.DinhMuc);
        }

        foreach (var tp in tk.ThanhPhans)
        {
            foreach (var d in tp.ThanhPhan.DinhLuongs)
            {
                var can = d.DinhMuc * tp.SoLuong;
                if (!nhuCau.TryGetValue(d.NguyenLieuId, out var hien))
                    nhuCau[d.NguyenLieuId] = (d.NguyenLieu, can);
                else
                    nhuCau[d.NguyenLieuId] = (d.NguyenLieu, hien.SoLuong + can);
            }
        }

        var ngayLay = DateOnly.FromDateTime(tk.NgayGioLay);
        var nlIds = nhuCau.Keys.ToList();
        var tonLos = await db.LoNguyenLieus
            .Where(l => nlIds.Contains(l.NguyenLieuId) && l.SoLuongCon > 0 && l.HanSuDung >= ngayLay)
            .GroupBy(l => l.NguyenLieuId)
            .Select(g => new { NguyenLieuId = g.Key, TongCon = g.Sum(l => l.SoLuongCon) })
            .ToDictionaryAsync(x => x.NguyenLieuId, x => x.TongCon);

        var canhBaos = new List<string>();
        foreach (var (nlId, (nl, tongCan)) in nhuCau)
        {
            var tonCon = tonLos.GetValueOrDefault(nlId, 0m);
            if (tonCon < tongCan)
            {
                var thieu = tongCan - tonCon;
                canhBaos.Add($"{nl.TenGoi} thiếu {thieu:0.###} {nl.DonViTinh}");
            }
        }

        return canhBaos;
    }

    public async Task DuyetAsync(int id, int nguoiDuyetId, decimal phuPhi, string? lyDoPhuPhi)
    {
        var tk = await db.ThietKeBanhs
            .Include(x => x.ThanhPhans)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        if (phuPhi < 0)
            throw new NghiepVuException("Phụ phí không được âm.");

        var maxPhuPhi = tk.GiaTamTinh * ThietKeRule.PhuPhiToiDa;
        if (phuPhi > maxPhuPhi)
            throw new NghiepVuException($"Phụ phí tối đa 30% giá tạm tính ({maxPhuPhi:N0} ₫).");

        if (phuPhi > 0 && string.IsNullOrWhiteSpace(lyDoPhuPhi))
            throw new NghiepVuException("Phải nhập lý do khi có phụ phí.");

        tk.PhuPhi = phuPhi;
        tk.LyDoPhuPhi = phuPhi > 0 ? lyDoPhuPhi?.Trim() : null;
        tk.GiaCuoi = tk.GiaTamTinh + phuPhi;

        var gvNen = await sanPham.GiaVonAsync(tk.SanPhamNenId);
        decimal tongGiaVon = gvNen.GiaVon;
        foreach (var tp in tk.ThanhPhans)
        {
            var gvTp = await thanhPhan.GiaVonAsync(tp.ThanhPhanId);
            tongGiaVon += gvTp.GiaVon * tp.SoLuong;
        }
        tk.GiaVonDuKien = tongGiaVon;

        var bayGio = clock.GetLocalNow().DateTime;
        tk.NgayDuyet = bayGio;
        tk.NguoiDuyetId = nguoiDuyetId;
        tk.HanBaoGia = bayGio.AddHours(ThietKeRule.GioHanBaoGia);

        ThietKeRule.Chuyen(tk, TrangThaiThietKe.DA_BAO_GIA);
        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Tải lại trang và thử lại.");
    }

    public async Task YeuCauSuaAsync(int id, int nguoiId, string ghiChu)
    {
        if (string.IsNullOrWhiteSpace(ghiChu))
            throw new NghiepVuException("Ghi chú yêu cầu sửa không được để trống.");

        var tk = await db.ThietKeBanhs.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        ThietKeRule.Chuyen(tk, TrangThaiThietKe.CAN_SUA);
        tk.GhiChuBep = ghiChu.Trim();
        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Tải lại trang và thử lại.");
    }

    public async Task TuChoiAsync(int id, int nguoiId, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new NghiepVuException("Lý do từ chối không được để trống.");

        var tk = await db.ThietKeBanhs.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NghiepVuException("Không tìm thấy thiết kế.");

        ThietKeRule.Chuyen(tk, TrangThaiThietKe.TU_CHOI);
        tk.GhiChuBep = lyDo.Trim();
        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Tải lại trang và thử lại.");
    }

    public async Task<int> QuetHetHanAsync()
    {
        var bayGio = clock.GetLocalNow().DateTime;
        var hetHans = await db.ThietKeBanhs
            .Where(x => x.TrangThai == TrangThaiThietKe.DA_BAO_GIA && x.HanBaoGia != null && x.HanBaoGia < bayGio)
            .ToListAsync();

        foreach (var tk in hetHans)
        {
            tk.TrangThai = TrangThaiThietKe.HET_HAN;
        }

        if (hetHans.Count > 0)
        {
            // Chạy kèm các lần đọc: nếu người khác vừa sửa (vd. thu ngân đang xác nhận cọc) thì bỏ qua, lần đọc sau quét lại.
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var e in db.ChangeTracker.Entries<ThietKeBanh>().Where(e => e.State == EntityState.Modified))
                    await e.ReloadAsync();
                return 0;
            }
        }

        return hetHans.Count;
    }

    public async Task LuuAnhThanhPhamAsync(int donHangId, string tenFile)
    {
        if (string.IsNullOrWhiteSpace(tenFile))
            throw new NghiepVuException("Tên file ảnh thành phẩm không được để trống.");

        var don = await db.DonHangs.Include(d => d.ChiTiets).ThenInclude(c => c.ThietKeBanh)
            .FirstOrDefaultAsync(d => d.Id == donHangId)
            ?? throw new NghiepVuException("Không tìm thấy đơn hàng.");

        if (don.TrangThai != TrangThaiDon.CHO_GIAO)
            throw new NghiepVuException("Chỉ được lưu ảnh thành phẩm khi đơn hàng ở trạng thái chờ giao.");

        var tk = don.ChiTiets.FirstOrDefault(c => c.ThietKeBanh != null)?.ThietKeBanh
            ?? (don.ChiTiets.FirstOrDefault(c => c.ThietKeBanhId.HasValue) is { } ctWithId
                ? await db.ThietKeBanhs.FindAsync(ctWithId.ThietKeBanhId!.Value)
                : null)
            ?? throw new NghiepVuException("Đơn hàng không có bánh thiết kế.");

        tk.AnhThanhPham = tenFile;
        await db.LuuNghiepVuAsync("Thiết kế vừa được cập nhật ở nơi khác. Vui lòng thử lại.");
    }
}
