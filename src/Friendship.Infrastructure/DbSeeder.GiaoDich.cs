using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure;

public static partial class DbSeeder
{
    /// <summary>Đồng hồ của seed: chỉnh tới từng mốc để service ghi đúng thời điểm quá khứ.</summary>
    sealed class DongHoSeed(DateTime luc) : TimeProvider
    {
        public DateTime Luc { get; set; } = luc;
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Luc, DateTimeKind.Utc));
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    /// <summary>Spec mục 7: một ngày bán hàng hai hôm trước (ca đã chốt, lệch 20.000đ có giải trình), đơn ở đủ 8 trạng thái,
    /// phiếu hủy đủ 4 trạng thái, một kiểm kê đã duyệt. Đi qua chính các service nên số liệu khớp luật nghiệp vụ.
    /// Chỉ chạy trên CSDL chưa có đơn, chưa có ca và đã có đủ danh mục mẫu hiện hành (CSDL tạo từ bản cũ thì bỏ qua).
    /// Cả kịch bản nằm trong một transaction: lỗi giữa chừng thì không để lại ca mở hay đơn dở dang.</summary>
    static async Task SeedGiaoDichAsync(FriendshipDb db, DateTime bayGio)
    {
        if (await db.DonHangs.AnyAsync() || await db.CaThuNgans.AnyAsync()) return;
        string[] banhCan = ["Bánh su kem", "Tart trứng", "Croissant", "Bông lan", "Bánh kem sinh nhật", "Bánh kem matcha dâu", "Cookie socola"];
        if (await db.SanPhams.CountAsync(s => banhCan.Contains(s.TenBanh)) < banhCan.Length) return;

        // InMemory (test) không có transaction; SQL Server thì có.
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync() : null;
        try
        {
            await KichBanAsync(db, bayGio);
            if (tx is not null) await tx.CommitAsync();
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;     // transaction chưa commit tự rollback khi dispose
        }
    }

    static async Task KichBanAsync(FriendshipDb db, DateTime bayGio)
    {
        var gio = new DongHoSeed(bayGio);
        var ca = new CaService(db, gio);
        var tt = new ThanhToanService(ca, gio);
        var khach = new KhachHangService(db);
        var km = new KhuyenMaiService(db);
        var sp = new SanPhamService(db);
        var banLe = new BanLeService(db, gio, khach, km, ca, tt, sp);
        var donDat = new DonDatService(db, gio, khach, km, ca, tt, sp);
        var bep = new BepService(db, gio);
        var loBanh = new LoBanhService(db, gio);
        var huy = new HuyHangService(db, gio, sp);
        var kiemKe = new KiemKeService(db, gio);

        async Task<int> Nv(string user) => (await db.NhanViens.SingleAsync(x => x.TenDangNhap == user)).Id;
        async Task<int> Banh(string ten) => (await db.SanPhams.SingleAsync(x => x.TenBanh == ten)).Id;
        DateTime Luc(int ngay, int h, int m = 0) => bayGio.Date.AddDays(ngay).AddHours(h).AddMinutes(m);
        var thuNgan = await Nv("thungan");
        var quanLy = await Nv("quanly");
        var thoBanh = await Nv("bep");
        var thuKho = await Nv("kho");

        // ===== Ngày N−2: một ca bán hàng trọn vẹn =====
        gio.Luc = Luc(-2, 7);
        var suKem = await loBanh.GhiNhanMeAsync(await Banh("Bánh su kem"), 30);
        var tart = await loBanh.GhiNhanMeAsync(await Banh("Tart trứng"), 15);
        var croissant = await loBanh.GhiNhanMeAsync(await Banh("Croissant"), 20);

        gio.Luc = Luc(-2, 7, 30);
        await ca.MoCaAsync(thuNgan, 500000);

        gio.Luc = Luc(-2, 8, 15);
        await banLe.BanAsync(new(thuNgan, null, null, [new(suKem.SanPhamId, 4), new(croissant.SanPhamId, 2)], null, 0, PhuongThucTT.TIEN_MAT, 100000));

        gio.Luc = Luc(-2, 9);
        var donA = await donDat.TaoAsync(new(thuNgan, "0901000002", null, [new(await Banh("Bông lan"), 1, null)],
            Luc(-2, 16), null, 0, 50000, PhuongThucTT.TIEN_MAT, 50000));
        gio.Luc = Luc(-2, 9, 30);
        var donB = await donDat.TaoAsync(new(thuNgan, "0901000004", null, [new(await Banh("Bánh kem sinh nhật"), 1, "Mừng sinh nhật Minh")],
            Luc(-2, 18), null, 0, 150000, PhuongThucTT.CHUYEN_KHOAN, null));
        gio.Luc = Luc(-2, 10);
        var donC = await donDat.TaoAsync(new(thuNgan, "0901000001", null, [new(await Banh("Bánh kem sinh nhật"), 1, "Chúc mừng 20/10")],
            Luc(3, 10), null, 0, 150000, PhuongThucTT.TIEN_MAT, 200000));

        gio.Luc = Luc(-2, 10, 30);
        await banLe.BanAsync(new(thuNgan, "0901000001", null, [new(tart.SanPhamId, 6)], "GIAM20K", 0, PhuongThucTT.CHUYEN_KHOAN, null));

        gio.Luc = Luc(-2, 10, 45);
        await bep.BatDauLamAsync(donA.Id);
        await bep.BatDauLamAsync(donB.Id);

        gio.Luc = Luc(-2, 11);
        await donDat.HuyAsync(donC.Id, thuNgan, KieuHuy.KHACH_HUY);           // còn hơn 24 giờ trước hẹn: hoàn 100% cọc

        gio.Luc = Luc(-2, 13);
        await banLe.BanAsync(new(thuNgan, "0901000003", null, [new(suKem.SanPhamId, 10)], null, 50, PhuongThucTT.TIEN_MAT, 200000));

        gio.Luc = Luc(-2, 14);
        await bep.ChoGiaoAsync(donA.Id);
        gio.Luc = Luc(-2, 15);
        await bep.ChoGiaoAsync(donB.Id);

        gio.Luc = Luc(-2, 16, 30);
        await donDat.GiaoAsync(donA.Id, thuNgan, PhuongThucTT.TIEN_MAT, 100000);

        gio.Luc = Luc(-2, 17, 30);
        var donTra = await banLe.BanAsync(new(thuNgan, null, null, [new(croissant.SanPhamId, 12)], null, 0, PhuongThucTT.CHUYEN_KHOAN, null));
        gio.Luc = Luc(-2, 17, 45);
        await banLe.TraHangAsync(donTra.Don.Id, thuNgan, "Khách đổi ý trả lại");

        gio.Luc = Luc(-2, 18);
        var donD = await donDat.TaoAsync(new(thuNgan, "0901000003", null, [new(await Banh("Bông lan"), 2, null)],
            bayGio.AddHours(3), null, 0, 100000, PhuongThucTT.CHUYEN_KHOAN, null));
        var donE = await donDat.TaoAsync(new(thuNgan, "0901000002", null, [new(tart.SanPhamId, 20, null)],
            Luc(1, 9), null, 0, 200000, PhuongThucTT.TIEN_MAT, 200000));

        gio.Luc = Luc(-2, 19);
        await donDat.TaoAsync(new(thuNgan, "0901000005", "Võ Thanh Tâm", [new(await Banh("Bánh kem matcha dâu"), 1, "Chúc mừng kỷ niệm")],
            Luc(2, 15), null, 0, 100000, PhuongThucTT.CHUYEN_KHOAN, null));
        await donDat.TaoAsync(new(thuNgan, "0901000004", null, [new(await Banh("Cookie socola"), 3, null)],
            Luc(2, 10), null, 0, null, PhuongThucTT.TIEN_MAT, null));

        gio.Luc = Luc(-2, 21);
        var tong = await ca.TongHopAsync((await ca.CanDangMoAsync(thuNgan)).Id);
        await ca.ChotCaAsync(thuNgan, tong.TienMatHeThong - 20000, tong.ChuyenKhoanHeThong, "Thối nhầm 20.000đ cho khách lẻ buổi chiều.");

        // ===== Ngày N−1: hủy bánh hết hạn, kiểm kê, một phiếu bị từ chối =====
        gio.Luc = Luc(-1, 7, 30);
        var phieuQuaHan = await huy.LapAsync(thoBanh, new[] { suKem, tart, croissant }
            .Select(l => new DongHuyInput(LoaiDoiTuongHuy.LO_BANH, l.Id, l.SoLuongTon, LyDoHuy.QUA_HAN)).ToList());
        gio.Luc = Luc(-1, 8);
        await huy.DuyetAsync(phieuQuaHan.Id, quanLy);

        gio.Luc = Luc(-1, 9);
        var kk = await kiemKe.TaoPhieuAsync(thuKho);
        var loDuong = await db.LoNguyenLieus.SingleAsync(l => l.NguyenLieu.TenGoi == "Đường");
        var dongDuong = kk.ChiTiets.Single(c => c.LoNguyenLieuId == loDuong.Id);
        await kiemKe.LuuSoLieuAsync(kk.Id, [new(dongDuong.Id, dongDuong.SoLuongHeThong - 0.5m, "Hao hụt khi chia túi")]);
        await kiemKe.GuiDuyetAsync(kk.Id);
        gio.Luc = Luc(-1, 10);
        await kiemKe.DuyetAsync(kk.Id, quanLy);

        gio.Luc = Luc(-1, 14);
        var loBo = await db.LoNguyenLieus.Where(l => l.NguyenLieu.TenGoi == "Bơ").OrderBy(l => l.HanSuDung).FirstAsync();
        var phieuBo = await huy.LapAsync(thuKho, [new(LoaiDoiTuongHuy.LO_NGUYEN_LIEU, loBo.Id, 1, LyDoHuy.QUA_HAN)]);
        gio.Luc = Luc(-1, 15);
        await huy.TuChoiAsync(phieuBo.Id, quanLy, "Bơ còn hạn, ưu tiên xuất làm croissant trước.");

        // ===== Hôm nay: bếp đang làm, một phiếu chờ duyệt, quét bùng cọc =====
        gio.Luc = bayGio.AddHours(-2);
        await bep.BatDauLamAsync(donD.Id);
        await bep.BatDauLamAsync(donE.Id);
        gio.Luc = bayGio.AddMinutes(-30);
        await bep.ChoGiaoAsync(donD.Id);

        gio.Luc = bayGio.AddMinutes(-20);
        var loTrung = await db.LoNguyenLieus.Where(l => l.NguyenLieu.TenGoi == "Trứng gà").OrderBy(l => l.HanSuDung).FirstAsync();
        await huy.LapAsync(thuKho, [new(LoaiDoiTuongHuy.LO_NGUYEN_LIEU, loTrung.Id, 6, LyDoHuy.LOI_SAN_XUAT)]);

        gio.Luc = bayGio.AddMinutes(-10);
        await donDat.QuetBungCocAsync(thuNgan);                               // đơn B quá 24 giờ sau hẹn → BUNG_COC + phiếu hủy nháp
    }
}
