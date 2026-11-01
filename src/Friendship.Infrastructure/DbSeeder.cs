using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure;

public static partial class DbSeeder
{
    /// <summary>Mật khẩu của 5 tài khoản mẫu; Program.cs gán từ cấu hình DuLieuMau:MatKhau trước khi seed.</summary>
    public static string MatKhauMau { get; set; } = "";

    public static Task SeedAsync(FriendshipDb db, DateOnly? homNay = null, bool giaoDich = true, bool thietKe = false, AnhService? anh = null) =>
        SeedAsync(db, (homNay ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.FromDateTime(DateTime.Now)), giaoDich, thietKe, anh);

    /// <param name="giaoDich">Có seed đơn, ca, phiếu hủy, kiểm kê mẫu không (test web tắt vì các test đó tự đếm dữ liệu).</param>
    /// <param name="thietKe">Có seed thành phần trang trí, bánh nền và thiết kế mẫu không.</param>
    /// <param name="anh">Kho ảnh của app, để ảnh thiết kế mẫu nằm đúng thư mục app đọc.</param>
    public static async Task SeedAsync(FriendshipDb db, DateTime bayGio, bool giaoDich = true, bool thietKe = false, AnhService? anh = null)
    {
        await SeedNhanVienAsync(db);
        await SeedDanhMucAsync(db, DateOnly.FromDateTime(bayGio));
        await SeedKhachHangAsync(db);
        var chuaCoLoBanh = !await db.LoSanPhams.AnyAsync();  // lô bánh mẫu chỉ thêm vào CSDL mới, không bù lại mỗi lần khởi động
        if (giaoDich) await SeedGiaoDichAsync(db, bayGio);   // trước lô bánh hiện hành để FEFO ngày N−2 chỉ thấy lô của ngày đó
        if (chuaCoLoBanh) await SeedLoBanhAsync(db, bayGio);
        if (thietKe) await SeedThietKeAsync(db, bayGio, anh);
    }

    static async Task SeedNhanVienAsync(FriendshipDb db)
    {
        if (await db.NhanViens.AnyAsync()) return;

        var hasher = new PasswordHasher<NhanVien>();
        NhanVien Tao(string user, string ten, VaiTro vaiTro, string sdt)
        {
            var nv = new NhanVien { HoTen = ten, TenDangNhap = user, SoDienThoai = sdt, VaiTro = vaiTro };
            nv.MatKhauHash = hasher.HashPassword(nv, MatKhauMau);
            return nv;
        }

        db.NhanViens.AddRange(
            Tao("admin", "Quản trị viên", VaiTro.QUAN_TRI, "0900000001"),
            Tao("quanly", "Trần Quản Lý", VaiTro.QUAN_LY, "0900000002"),
            Tao("thungan", "Lê Thu Ngân", VaiTro.BAN_HANG, "0900000003"),
            Tao("bep", "Phạm Thợ Bánh", VaiTro.SAN_XUAT, "0900000004"),
            Tao("kho", "Hoàng Thủ Kho", VaiTro.KHO, "0900000005"));
        await db.SaveChangesAsync();
    }

    static async Task SeedDanhMucAsync(FriendshipDb db, DateOnly homNay)
    {
        if (!await db.NguyenLieus.AnyAsync())
        {
            NguyenLieu Nl(string ten, string dvt, decimal muc) => new() { TenGoi = ten, DonViTinh = dvt, MucCanhBaoTon = muc };
            var ds = new[]
            {
                Nl("Bột mì", "kg", 10), Nl("Đường", "kg", 5), Nl("Trứng gà", "quả", 60), Nl("Bơ", "kg", 3),
                Nl("Sữa tươi", "lít", 5), Nl("Kem tươi", "lít", 3), Nl("Socola", "kg", 2), Nl("Men nở", "g", 200),
                Nl("Tỏi", "kg", 0.5m), Nl("Phô mai", "kg", 1), Nl("Trứng muối", "quả", 20), Nl("Chà bông", "kg", 0.5m),
                Nl("Vani", "ml", 50), Nl("Matcha", "kg", 0.2m), Nl("Dâu tây", "kg", 1)
            };
            db.NguyenLieus.AddRange(ds);
            await db.SaveChangesAsync();
            NguyenLieu N(string ten) => ds.Single(x => x.TenGoi == ten);

            // (nguyên liệu, số lượng, hạn sử dụng tính từ hôm nay, đơn giá nhập, nhập cách đây bao nhiêu ngày)
            (string, decimal, int, decimal, int)[] lo =
            [
                ("Bột mì", 2, -1, 22000, 20),    // hết hạn
                ("Bột mì", 5, 2, 22000, 10),     // cận date
                ("Bột mì", 25, 90, 24000, 2),
                ("Đường", 20, 120, 18000, 5),
                ("Trứng gà", 100, 5, 3000, 3),
                ("Trứng gà", 200, 12, 3200, 1),
                ("Bơ", 2, 3, 150000, 15),        // cận date
                ("Bơ", 6, 30, 155000, 2),
                ("Sữa tươi", 6, 2, 28000, 4),    // cận date
                ("Kem tươi", 2, 4, 90000, 6),
                ("Kem tươi", 6, 10, 92000, 1),
                ("Socola", 5, 200, 180000, 7),
                ("Men nở", 500, 150, 400, 7),
                ("Tỏi", 1, 20, 60000, 3),
                ("Phô mai", 3, 25, 210000, 4),
                ("Trứng muối", 40, 2, 6000, 5),  // cận date
                ("Chà bông", 1.5m, 30, 320000, 6),
                ("Vani", 200, 300, 900, 30),
                ("Matcha", 0.5m, 180, 1200000, 20),
                ("Dâu tây", 0.8m, 3, 150000, 1), // cận date và dưới mức cảnh báo tồn (1 kg)
            ];
            db.LoNguyenLieus.AddRange(lo.Select(x => new LoNguyenLieu
            {
                NguyenLieuId = N(x.Item1).Id, NgayNhap = homNay.AddDays(-x.Item5), HanSuDung = homNay.AddDays(x.Item3),
                SoLuongNhap = x.Item2, SoLuongCon = x.Item2, DonGiaNhap = x.Item4
            }));

            SanPham Sp(string ten, string kt, decimal gia, int gio, params (string nl, decimal dm)[] bom)
            {
                var sp = new SanPham { TenBanh = ten, KichThuoc = kt, DonGia = gia, HanSuDungGio = gio };
                foreach (var (nl, dm) in bom) sp.DinhLuongs.Add(new DinhLuong { NguyenLieuId = N(nl).Id, DinhMuc = dm });
                return sp;
            }
            db.SanPhams.AddRange(
                Sp("Bánh su kem", "Cái", 15000, 24, ("Bột mì", 0.03m), ("Trứng gà", 0.5m), ("Bơ", 0.01m), ("Kem tươi", 0.03m)),
                Sp("Tart trứng", "Cái", 25000, 24, ("Bột mì", 0.04m), ("Trứng gà", 1m), ("Bơ", 0.02m), ("Đường", 0.01m)),
                Sp("Bông lan", "18cm", 120000, 48, ("Bột mì", 0.2m), ("Trứng gà", 4m), ("Đường", 0.1m), ("Sữa tươi", 0.1m)),
                Sp("Bánh kem sinh nhật", "20cm", 450000, 72, ("Bột mì", 0.3m), ("Trứng gà", 6m), ("Đường", 0.15m), ("Kem tươi", 0.5m), ("Socola", 0.1m)),
                Sp("Croissant", "Cái", 20000, 12, ("Bột mì", 0.05m), ("Bơ", 0.04m), ("Men nở", 2m), ("Đường", 0.005m)),
                Sp("Bánh mì bơ tỏi phô mai", "Cái", 18000, 24, ("Bột mì", 0.06m), ("Bơ", 0.015m), ("Tỏi", 0.005m), ("Phô mai", 0.01m), ("Men nở", 2m)),
                Sp("Bánh flan", "Hũ", 12000, 48, ("Trứng gà", 1m), ("Sữa tươi", 0.08m), ("Đường", 0.02m), ("Vani", 1m)),
                Sp("Cookie socola", "Hộp 10 cái", 65000, 168, ("Bột mì", 0.15m), ("Bơ", 0.08m), ("Đường", 0.06m), ("Socola", 0.08m), ("Trứng gà", 1m)),
                Sp("Bông lan trứng muối", "Hộp", 85000, 48, ("Bột mì", 0.12m), ("Trứng gà", 3m), ("Trứng muối", 2m), ("Chà bông", 0.03m), ("Đường", 0.05m)),
                Sp("Bánh kem matcha dâu", "16cm", 320000, 72, ("Bột mì", 0.2m), ("Trứng gà", 4m), ("Kem tươi", 0.4m), ("Matcha", 0.02m), ("Dâu tây", 0.2m), ("Đường", 0.1m)));
            await db.SaveChangesAsync();
        }

        if (!await db.KhuyenMais.AnyAsync())
        {
            db.KhuyenMais.AddRange(
                new KhuyenMai { MaKhuyenMai = "SINHNHAT10", TenChuongTrinh = "Giảm 10% đơn từ 200.000đ", LoaiGiam = LoaiGiam.PHAN_TRAM, GiaTriGiam = 10, GiaTriDonToiThieu = 200000, NgayBatDau = homNay.AddDays(-10), NgayKetThuc = homNay.AddDays(50) },
                new KhuyenMai { MaKhuyenMai = "GIAM20K", TenChuongTrinh = "Giảm 20.000đ đơn từ 100.000đ", LoaiGiam = LoaiGiam.SO_TIEN, GiaTriGiam = 20000, GiaTriDonToiThieu = 100000, NgayBatDau = homNay.AddDays(-5), NgayKetThuc = homNay.AddDays(25) },
                new KhuyenMai { MaKhuyenMai = "HETHAN5", TenChuongTrinh = "Khuyến mãi tháng trước", LoaiGiam = LoaiGiam.PHAN_TRAM, GiaTriGiam = 5, NgayBatDau = homNay.AddDays(-40), NgayKetThuc = homNay.AddDays(-10) });
            await db.SaveChangesAsync();
        }
    }


    static async Task SeedKhachHangAsync(FriendshipDb db)
    {
        if (await db.KhachHangs.AnyAsync()) return;
        db.KhachHangs.AddRange(
            new KhachHang { TenKhachHang = "Nguyễn Thị Lan", SoDienThoai = "0901000001", DiemTichLuy = 35 },
            new KhachHang { TenKhachHang = "Trần Văn Nam", SoDienThoai = "0901000002", DiemTichLuy = 0 },
            new KhachHang { TenKhachHang = "Lê Hoa", SoDienThoai = "0901000003", DiemTichLuy = 120 },
            new KhachHang { TenKhachHang = "Phạm Minh", SoDienThoai = "0901000004", DiemTichLuy = 8 });
        await db.SaveChangesAsync();
    }

    /// <summary>Vài lô bánh còn hạn (không cận date, để không làm hiện cảnh báo ngoài ý muốn) cho bán lẻ hôm nay.</summary>
    static async Task SeedLoBanhAsync(FriendshipDb db, DateTime bayGio)
    {
        // (tên bánh, số lượng làm ra, đã bán, còn bao nhiêu giờ đến HSD); ra lò cách đây (hanSuDungGio - giờ còn lại).
        (string, int, int, int)[] me = [("Bánh su kem", 24, 4, 20), ("Tart trứng", 12, 0, 10), ("Croissant", 16, 6, 11)];
        foreach (var (ten, sx, ban, gioConLai) in me)
        {
            var sp = await db.SanPhams.SingleAsync(x => x.TenBanh == ten);
            db.LoSanPhams.Add(new LoSanPham
            {
                SanPhamId = sp.Id, NgaySanXuat = bayGio.AddHours(gioConLai - sp.HanSuDungGio), HanSuDung = bayGio.AddHours(gioConLai),
                SoLuongSanXuat = sx, SoLuongTon = sx - ban
            });
        }
        await db.SaveChangesAsync();
    }
}
