using System.Text.Json;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Friendship.Infrastructure;

public static partial class DbSeeder
{
    public static async Task SeedThietKeAsync(FriendshipDb db, DateTime? bayGio = null, AnhService? anhService = null)
    {
        var luc = bayGio ?? DateTime.Now;

        // Đảm bảo có dữ liệu nền nếu chạy độc lập
        if (!await db.NguyenLieus.AnyAsync()) await SeedDanhMucAsync(db, DateOnly.FromDateTime(luc));
        if (!await db.KhachHangs.AnyAsync()) await SeedKhachHangAsync(db);
        if (!await db.NhanViens.AnyAsync()) await SeedNhanVienAsync(db);

        var kemTuoi = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Kem tươi");
        var bo = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Bơ");

        // 1. 3 Bánh nền tùy chỉnh (BOM gồm kem phủ và bơ, không gồm cốt)
        var sp16 = await db.SanPhams.Include(s => s.DinhLuongs).FirstOrDefaultAsync(s => s.TenBanh == "Bánh kem tùy chỉnh" && s.KichThuoc == "16cm");
        if (sp16 == null)
        {
            sp16 = new SanPham
            {
                TenBanh = "Bánh kem tùy chỉnh",
                KichThuoc = "16cm",
                DonGia = 250000m,
                HanSuDungGio = 72,
                DangKinhDoanh = true
            };
            sp16.DinhLuongs.Add(new DinhLuong { NguyenLieuId = kemTuoi.Id, DinhMuc = 0.3m });
            sp16.DinhLuongs.Add(new DinhLuong { NguyenLieuId = bo.Id, DinhMuc = 0.05m });
            db.SanPhams.Add(sp16);
        }

        var sp20 = await db.SanPhams.Include(s => s.DinhLuongs).FirstOrDefaultAsync(s => s.TenBanh == "Bánh kem tùy chỉnh" && s.KichThuoc == "20cm");
        if (sp20 == null)
        {
            sp20 = new SanPham
            {
                TenBanh = "Bánh kem tùy chỉnh",
                KichThuoc = "20cm",
                DonGia = 320000m,
                HanSuDungGio = 72,
                DangKinhDoanh = true
            };
            sp20.DinhLuongs.Add(new DinhLuong { NguyenLieuId = kemTuoi.Id, DinhMuc = 0.4m });
            sp20.DinhLuongs.Add(new DinhLuong { NguyenLieuId = bo.Id, DinhMuc = 0.08m });
            db.SanPhams.Add(sp20);
        }

        var sp24 = await db.SanPhams.Include(s => s.DinhLuongs).FirstOrDefaultAsync(s => s.TenBanh == "Bánh kem tùy chỉnh" && s.KichThuoc == "24cm");
        if (sp24 == null)
        {
            sp24 = new SanPham
            {
                TenBanh = "Bánh kem tùy chỉnh",
                KichThuoc = "24cm",
                DonGia = 420000m,
                HanSuDungGio = 72,
                DangKinhDoanh = true
            };
            sp24.DinhLuongs.Add(new DinhLuong { NguyenLieuId = kemTuoi.Id, DinhMuc = 0.5m });
            sp24.DinhLuongs.Add(new DinhLuong { NguyenLieuId = bo.Id, DinhMuc = 0.1m });
            db.SanPhams.Add(sp24);
        }
        await db.SaveChangesAsync();

        // 2. 17 thành phần trang trí có định lượng
        var botMi = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Bột mì");
        var trung = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Trứng gà");
        var duong = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Đường");
        var vani = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Vani");
        var socola = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Socola");
        var matcha = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Matcha");
        var dauTay = await db.NguyenLieus.SingleAsync(n => n.TenGoi == "Dâu tây");

        var giayInNl = await db.NguyenLieus.FirstOrDefaultAsync(n => n.TenGoi == "Giấy in ăn được");
        if (giayInNl == null)
        {
            giayInNl = new NguyenLieu { TenGoi = "Giấy in ăn được", DonViTinh = "tờ", MucCanhBaoTon = 20m };
            db.NguyenLieus.Add(giayInNl);
            await db.SaveChangesAsync();
        }

        var cherryNl = await db.NguyenLieus.FirstOrDefaultAsync(n => n.TenGoi == "Cherry");
        if (cherryNl == null)
        {
            cherryNl = new NguyenLieu { TenGoi = "Cherry", DonViTinh = "kg", MucCanhBaoTon = 0.5m };
            db.NguyenLieus.Add(cherryNl);
            await db.SaveChangesAsync();
        }

        var homNay = DateOnly.FromDateTime(luc);
        if (!await db.LoNguyenLieus.AnyAsync(l => l.NguyenLieuId == giayInNl.Id))
        {
            db.LoNguyenLieus.Add(new LoNguyenLieu
            {
                NguyenLieuId = giayInNl.Id,
                NgayNhap = homNay.AddDays(-2),
                HanSuDung = homNay.AddDays(180),
                SoLuongNhap = 50m,
                SoLuongCon = 50m,
                DonGiaNhap = 5000m
            });
            await db.SaveChangesAsync();
        }
        if (!await db.LoNguyenLieus.AnyAsync(l => l.NguyenLieuId == cherryNl.Id))
        {
            db.LoNguyenLieus.Add(new LoNguyenLieu
            {
                NguyenLieuId = cherryNl.Id,
                NgayNhap = homNay.AddDays(-2),
                HanSuDung = homNay.AddDays(60),
                SoLuongNhap = 3m,
                SoLuongCon = 3m,
                DonGiaNhap = 220000m
            });
            await db.SaveChangesAsync();
        }

        var existingTps = await db.ThanhPhanTrangTris.ToListAsync();
        var existingTpMap = existingTps.ToDictionary(t => t.Ten, StringComparer.OrdinalIgnoreCase);

        ThanhPhanTrangTri TaoTp(NhomThanhPhan nhom, string ten, decimal gia, int toiDa, int? spNenId, string? icon, params (NguyenLieu nl, decimal muc)[] bom)
        {
            if (existingTpMap.TryGetValue(ten, out var existing))
            {
                return existing;
            }

            var tp = new ThanhPhanTrangTri
            {
                Nhom = nhom,
                Ten = ten,
                GiaBan = gia,
                SoLuongToiDa = toiDa,
                SanPhamNenId = spNenId,
                AnhMinhHoa = icon,
                DangKinhDoanh = true
            };
            foreach (var (nl, muc) in bom)
                tp.DinhLuongs.Add(new DinhLuongThanhPhan { NguyenLieuId = nl.Id, DinhMuc = muc });
            db.ThanhPhanTrangTris.Add(tp);
            existingTpMap[ten] = tp;
            return tp;
        }

        var cotVani16 = TaoTp(NhomThanhPhan.COT, "Cốt vani 16cm", 40000m, 1, sp16.Id, "macaron.svg", (botMi, 0.15m), (trung, 3m), (duong, 0.08m), (vani, 2m));
        var cotVani20 = TaoTp(NhomThanhPhan.COT, "Cốt vani 20cm", 50000m, 1, sp20.Id, "macaron.svg", (botMi, 0.20m), (trung, 4m), (duong, 0.10m), (vani, 3m));
        var cotVani24 = TaoTp(NhomThanhPhan.COT, "Cốt vani 24cm", 65000m, 1, sp24.Id, "macaron.svg", (botMi, 0.28m), (trung, 6m), (duong, 0.15m), (vani, 5m));

        var cotSocola16 = TaoTp(NhomThanhPhan.COT, "Cốt socola 16cm", 45000m, 1, sp16.Id, "socola.svg", (botMi, 0.15m), (trung, 3m), (duong, 0.08m), (socola, 0.06m));
        var cotSocola20 = TaoTp(NhomThanhPhan.COT, "Cốt socola 20cm", 55000m, 1, sp20.Id, "socola.svg", (botMi, 0.20m), (trung, 4m), (duong, 0.10m), (socola, 0.08m));
        var cotSocola24 = TaoTp(NhomThanhPhan.COT, "Cốt socola 24cm", 70000m, 1, sp24.Id, "socola.svg", (botMi, 0.28m), (trung, 6m), (duong, 0.15m), (socola, 0.12m));

        var cotMatcha16 = TaoTp(NhomThanhPhan.COT, "Cốt matcha 16cm", 50000m, 1, sp16.Id, null, (botMi, 0.15m), (trung, 3m), (duong, 0.08m), (matcha, 0.015m));
        var cotMatcha20 = TaoTp(NhomThanhPhan.COT, "Cốt matcha 20cm", 60000m, 1, sp20.Id, null, (botMi, 0.20m), (trung, 4m), (duong, 0.10m), (matcha, 0.02m));
        var cotMatcha24 = TaoTp(NhomThanhPhan.COT, "Cốt matcha 24cm", 75000m, 1, sp24.Id, null, (botMi, 0.28m), (trung, 6m), (duong, 0.15m), (matcha, 0.03m));

        var vienKem16 = TaoTp(NhomThanhPhan.VIEN, "Viền kem xoắn 16cm", 15000m, 1, sp16.Id, "nen.svg", (kemTuoi, 0.08m), (duong, 0.015m));
        var vienKem20 = TaoTp(NhomThanhPhan.VIEN, "Viền kem xoắn 20cm", 20000m, 1, sp20.Id, "nen.svg", (kemTuoi, 0.1m), (duong, 0.02m));
        var vienKem24 = TaoTp(NhomThanhPhan.VIEN, "Viền kem xoắn 24cm", 25000m, 1, sp24.Id, "nen.svg", (kemTuoi, 0.13m), (duong, 0.025m));
        var toppingDau = TaoTp(NhomThanhPhan.TOPPING, "Dâu tây", 5000m, 12, null, "dau.svg", (dauTay, 0.02m));
        var toppingCherry = TaoTp(NhomThanhPhan.TOPPING, "Cherry", 4000m, 12, null, "cherry.svg", (cherryNl, 0.01m));
        var toppingMacaron = TaoTp(NhomThanhPhan.TOPPING, "Macaron", 8000m, 8, null, "macaron.svg", (botMi, 0.01m), (duong, 0.02m), (trung, 0.5m));
        var toppingSocola = TaoTp(NhomThanhPhan.TOPPING, "Socola miếng", 3000m, 12, null, "socola.svg", (socola, 0.01m));
        var giayIn = TaoTp(NhomThanhPhan.IN_AN, "Giấy in ăn được", 30000m, 1, null, "in-hinh.svg", (giayInNl, 1m));
        var tang20 = TaoTp(NhomThanhPhan.TANG, "Tầng 2 (16cm)", 120000m, 1, sp20.Id, "tang-banh.svg", (botMi, 0.12m), (trung, 2m), (kemTuoi, 0.15m));
        var tang24 = TaoTp(NhomThanhPhan.TANG, "Tầng 2 (18cm)", 160000m, 1, sp24.Id, "tang-banh.svg", (botMi, 0.15m), (trung, 3m), (kemTuoi, 0.20m));

        await db.SaveChangesAsync();

        // 3. 5 thiết kế mẫu
        anhService ??= new AnhService(Path.Combine("App_Data", "anh"));

        async Task<string> TaoAnhMauAsync(Rgba32 mauNen)
        {
            using var img = new Image<Rgba32>(200, 200);
            int cx = 100, cy = 100, r = 80;
            for (int y = 0; y < 200; y++)
            {
                for (int x = 0; x < 200; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= r * r)
                        img[x, y] = mauNen;
                    else
                        img[x, y] = new Rgba32(0, 0, 0, 0);
                }
            }
            using var ms = new MemoryStream();
            await img.SaveAsPngAsync(ms);
            ms.Position = 0;
            return await anhService.LuuAsync(ms);
        }

        string TaoJson(int sanPhamNenId, params (int id, int soLuong)[] tps)
        {
            var obj = new
            {
                sanPhamNenId,
                thanhPhan = tps.Select(t => new { id = t.id, soLuong = t.soLuong }).ToList()
            };
            return JsonSerializer.Serialize(obj);
        }

        var thuNgan = await db.NhanViens.SingleAsync(x => x.TenDangNhap == "thungan");
        var bep = await db.NhanViens.SingleAsync(x => x.TenDangNhap == "bep");

        var khach1 = await db.KhachHangs.SingleAsync(x => x.SoDienThoai == "0901000001");
        var khach2 = await db.KhachHangs.SingleAsync(x => x.SoDienThoai == "0901000002");
        var khach3 = await db.KhachHangs.SingleAsync(x => x.SoDienThoai == "0901000003");
        var khach4 = await db.KhachHangs.SingleAsync(x => x.SoDienThoai == "0901000004");
        var khach5 = await db.KhachHangs.FirstOrDefaultAsync(x => x.SoDienThoai == "0901000005")
                     ?? await db.KhachHangs.FirstAsync();

        if (await db.ThietKeBanhs.AnyAsync()) return;

        // Thiết kế 1: CHO_DUYET
        var tk1 = new ThietKeBanh
        {
            MaTraCuu = ThietKeService.TaoMaTraCuu(),
            KhachHangId = khach1.Id,
            SanPhamNenId = sp20.Id,
            TrangThai = TrangThaiThietKe.CHO_DUYET,
            ChuTrenBanh = "Mừng sinh nhật Minh",
            LoiNhan = "Giao bánh lúc 14h chiều",
            NgayGioLay = luc.AddDays(3),
            NgayGui = luc.AddHours(-4),
            DuLieuJson = TaoJson(sp20.Id, (cotVani20.Id, 1), (vienKem20.Id, 1), (toppingDau.Id, 6)),
            GiaTamTinh = 420000m,
            PhuPhi = 0,
            GiaCuoi = 420000m,
            GiaVonDuKien = 120000m,
            AnhThietKe = await TaoAnhMauAsync(new Rgba32(255, 248, 220))
        };
        tk1.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = cotVani20.Id, SoLuong = 1, DonGia = cotVani20.GiaBan });
        tk1.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = vienKem20.Id, SoLuong = 1, DonGia = vienKem20.GiaBan });
        tk1.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = toppingDau.Id, SoLuong = 6, DonGia = toppingDau.GiaBan });
        db.ThietKeBanhs.Add(tk1);

        // Thiết kế 2: CAN_SUA
        var tk2 = new ThietKeBanh
        {
            MaTraCuu = ThietKeService.TaoMaTraCuu(),
            KhachHangId = khach2.Id,
            SanPhamNenId = sp16.Id,
            TrangThai = TrangThaiThietKe.CAN_SUA,
            ChuTrenBanh = "Chúc mừng sinh nhật em gái yêu quý",
            GhiChuBep = "Chữ quá dài cho bánh 16cm, vui lòng rút gọn dưới 20 ký tự.",
            NgayGioLay = luc.AddDays(3),
            NgayGui = luc.AddHours(-8),
            DuLieuJson = TaoJson(sp16.Id, (cotSocola16.Id, 1), (toppingDau.Id, 4)),
            GiaTamTinh = 315000m,
            PhuPhi = 0,
            GiaCuoi = 315000m,
            GiaVonDuKien = 95000m,
            AnhThietKe = await TaoAnhMauAsync(new Rgba32(93, 64, 55))
        };
        tk2.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = cotSocola16.Id, SoLuong = 1, DonGia = cotSocola16.GiaBan });
        tk2.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = toppingDau.Id, SoLuong = 4, DonGia = toppingDau.GiaBan });
        db.ThietKeBanhs.Add(tk2);

        // Thiết kế 3: DA_BAO_GIA
        var tk3 = new ThietKeBanh
        {
            MaTraCuu = ThietKeService.TaoMaTraCuu(),
            KhachHangId = khach3.Id,
            SanPhamNenId = sp20.Id,
            TrangThai = TrangThaiThietKe.DA_BAO_GIA,
            ChuTrenBanh = "Kỷ niệm ngày cưới",
            NgayGioLay = luc.AddDays(4),
            NgayGui = luc.AddHours(-12),
            NgayDuyet = luc.AddHours(-6),
            NguoiDuyetId = bep.Id,
            HanBaoGia = luc.AddHours(18),
            PhuPhi = 20000m,
            LyDoPhuPhi = "Họa tiết in ảnh cần xử lý màu sắc tỉ mỉ.",
            DuLieuJson = TaoJson(sp20.Id, (cotMatcha20.Id, 1), (vienKem20.Id, 1), (giayIn.Id, 1)),
            GiaTamTinh = 430000m,
            GiaCuoi = 450000m,
            GiaVonDuKien = 135000m,
            AnhThietKe = await TaoAnhMauAsync(new Rgba32(143, 188, 143)),
            AnhInAn = await TaoAnhMauAsync(new Rgba32(255, 255, 255))
        };
        tk3.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = cotMatcha20.Id, SoLuong = 1, DonGia = cotMatcha20.GiaBan });
        tk3.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = vienKem20.Id, SoLuong = 1, DonGia = vienKem20.GiaBan });
        tk3.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = giayIn.Id, SoLuong = 1, DonGia = giayIn.GiaBan });
        db.ThietKeBanhs.Add(tk3);

        // Thiết kế 4: DA_DAT (đơn đang DANG_LAM)
        var tk4 = new ThietKeBanh
        {
            MaTraCuu = ThietKeService.TaoMaTraCuu(),
            KhachHangId = khach4.Id,
            SanPhamNenId = sp20.Id,
            TrangThai = TrangThaiThietKe.DA_DAT,
            ChuTrenBanh = "Mừng sinh nhật Mẹ",
            NgayGioLay = luc.AddDays(1),
            NgayGui = luc.AddDays(-2),
            NgayDuyet = luc.AddDays(-2).AddHours(2),
            NguoiDuyetId = bep.Id,
            HanBaoGia = luc.AddDays(-1).AddHours(2),
            PhuPhi = 20000m,
            LyDoPhuPhi = "Đính nhiều dâu tây tạo hình tim",
            DuLieuJson = TaoJson(sp20.Id, (cotVani20.Id, 1), (vienKem20.Id, 1), (toppingDau.Id, 8)),
            GiaTamTinh = 430000m,
            GiaCuoi = 450000m,
            GiaVonDuKien = 140000m,
            AnhThietKe = await TaoAnhMauAsync(new Rgba32(255, 182, 193))
        };
        tk4.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = cotVani20.Id, SoLuong = 1, DonGia = cotVani20.GiaBan });
        tk4.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = vienKem20.Id, SoLuong = 1, DonGia = vienKem20.GiaBan });
        tk4.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = toppingDau.Id, SoLuong = 8, DonGia = toppingDau.GiaBan });

        var don4 = new DonHang
        {
            LoaiDon = LoaiDon.DAT_TRUOC,
            TrangThai = TrangThaiDon.DANG_LAM,
            KhachHangId = khach4.Id,
            NhanVienId = thuNgan.Id,
            NgayLap = luc.AddDays(-2).AddHours(4),
            NgayGioLay = luc.AddDays(1),
            TongTienHang = 450000m,
            TongThanhToan = 450000m
        };
        var ct4 = new ChiTietDonHang
        {
            SanPhamId = sp20.Id,
            SoLuong = 1,
            DonGia = 450000m,
            ThanhTien = 450000m,
            LoiChuc = "Mừng sinh nhật Mẹ",
            ThietKeBanh = tk4
        };
        don4.ChiTiets.Add(ct4);
        db.DonHangs.Add(don4);
        tk4.DonHang = don4;
        db.ThietKeBanhs.Add(tk4);

        // Thiết kế 5: DA_DAT (đơn DA_GIAO, có ảnh thành phẩm)
        var tk5 = new ThietKeBanh
        {
            MaTraCuu = ThietKeService.TaoMaTraCuu(),
            KhachHangId = khach5.Id,
            SanPhamNenId = sp24.Id,
            TrangThai = TrangThaiThietKe.DA_DAT,
            ChuTrenBanh = "Happy Company 10 Years",
            NgayGioLay = luc.AddDays(-1),
            NgayGui = luc.AddDays(-3),
            NgayDuyet = luc.AddDays(-3).AddHours(2),
            NguoiDuyetId = bep.Id,
            HanBaoGia = luc.AddDays(-2).AddHours(2),
            PhuPhi = 50000m,
            LyDoPhuPhi = "Bánh cỡ lớn 24cm cần hộp đặc biệt",
            DuLieuJson = TaoJson(sp24.Id, (cotSocola24.Id, 1), (toppingDau.Id, 10)),
            GiaTamTinh = 540000m,
            GiaCuoi = 590000m,
            GiaVonDuKien = 180000m,
            AnhThietKe = await TaoAnhMauAsync(new Rgba32(93, 64, 55)),
            AnhThanhPham = await TaoAnhMauAsync(new Rgba32(255, 230, 180))
        };
        tk5.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = cotSocola24.Id, SoLuong = 1, DonGia = cotSocola24.GiaBan });
        tk5.ThanhPhans.Add(new ThietKeThanhPhan { ThanhPhanId = toppingDau.Id, SoLuong = 10, DonGia = toppingDau.GiaBan });

        var don5 = new DonHang
        {
            LoaiDon = LoaiDon.DAT_TRUOC,
            TrangThai = TrangThaiDon.DA_GIAO,
            KhachHangId = khach5.Id,
            NhanVienId = thuNgan.Id,
            NgayLap = luc.AddDays(-3).AddHours(4),
            NgayGioLay = luc.AddDays(-1),
            TongTienHang = 590000m,
            TongThanhToan = 590000m
        };
        var ct5 = new ChiTietDonHang
        {
            SanPhamId = sp24.Id,
            SoLuong = 1,
            DonGia = 590000m,
            ThanhTien = 590000m,
            LoiChuc = "Happy Company 10 Years",
            ThietKeBanh = tk5
        };
        don5.ChiTiets.Add(ct5);
        db.DonHangs.Add(don5);
        tk5.DonHang = don5;
        db.ThietKeBanhs.Add(tk5);

        await db.SaveChangesAsync();
    }
}
