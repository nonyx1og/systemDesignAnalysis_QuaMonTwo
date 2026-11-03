-- =========================================================================
-- TIá»†M BÃNH FRIENDSHIP (FRIENDSHIP BAKERY) - DATABASE RESTORE SCRIPT
-- Bao gom: Toan bo Cau truc Bang (Schema DDL) + Du lieu khoi tao (Seed Data)
-- Ngay xuat: 01/11/2026 23:09:37
-- =========================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Friendship')
BEGIN
    CREATE DATABASE [Friendship];
END;
GO

USE [Friendship];
GO

-- ==================== 1. CAU TRUC BANG (SCHEMA DDL) ====================
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [KhachHangs] (
        [Id] int NOT NULL IDENTITY,
        [TenKhachHang] nvarchar(max) NOT NULL,
        [SoDienThoai] nvarchar(450) NOT NULL,
        [DiemTichLuy] int NOT NULL,
        CONSTRAINT [PK_KhachHangs] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_KhachHang_Diem] CHECK ([DiemTichLuy] >= 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [KhuyenMais] (
        [Id] int NOT NULL IDENTITY,
        [TenChuongTrinh] nvarchar(max) NOT NULL,
        [LoaiGiam] nvarchar(30) NOT NULL,
        [GiaTriGiam] decimal(18,3) NOT NULL,
        [GiaTriDonToiThieu] decimal(18,3) NOT NULL,
        [NgayBatDau] date NOT NULL,
        [NgayKetThuc] date NOT NULL,
        [DangApDung] bit NOT NULL,
        CONSTRAINT [PK_KhuyenMais] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_KhuyenMai_Ngay] CHECK ([NgayKetThuc] >= [NgayBatDau])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [NguyenLieus] (
        [Id] int NOT NULL IDENTITY,
        [TenGoi] nvarchar(max) NOT NULL,
        [DonViTinh] nvarchar(max) NOT NULL,
        [MucCanhBaoTon] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_NguyenLieus] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [NhanViens] (
        [Id] int NOT NULL IDENTITY,
        [HoTen] nvarchar(max) NOT NULL,
        [GioiTinh] nvarchar(max) NULL,
        [NgaySinh] date NULL,
        [SoDienThoai] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [TenDangNhap] nvarchar(450) NOT NULL,
        [MatKhauHash] nvarchar(max) NOT NULL,
        [VaiTro] nvarchar(30) NOT NULL,
        [DangHoatDong] bit NOT NULL,
        CONSTRAINT [PK_NhanViens] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [SanPhams] (
        [Id] int NOT NULL IDENTITY,
        [TenBanh] nvarchar(max) NOT NULL,
        [KichThuoc] nvarchar(max) NOT NULL,
        [DonGia] decimal(18,3) NOT NULL,
        [HanSuDungGio] int NOT NULL,
        [DangKinhDoanh] bit NOT NULL,
        CONSTRAINT [PK_SanPhams] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [LoNguyenLieus] (
        [Id] int NOT NULL IDENTITY,
        [NguyenLieuId] int NOT NULL,
        [NgayNhap] date NOT NULL,
        [HanSuDung] date NOT NULL,
        [SoLuongNhap] decimal(18,3) NOT NULL,
        [SoLuongCon] decimal(18,3) NOT NULL,
        [DonGiaNhap] decimal(18,3) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_LoNguyenLieus] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LoNguyenLieu_Ton] CHECK ([SoLuongCon] >= 0),
        CONSTRAINT [FK_LoNguyenLieus_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [CaThuNgans] (
        [Id] int NOT NULL IDENTITY,
        [NhanVienId] int NOT NULL,
        [GioMoCa] datetime2 NOT NULL,
        [GioDongCa] datetime2 NULL,
        [TienDauCa] decimal(18,3) NOT NULL,
        [TienMatHeThong] decimal(18,3) NOT NULL,
        [ChuyenKhoanHeThong] decimal(18,3) NOT NULL,
        [TienMatThucTe] decimal(18,3) NULL,
        [ChuyenKhoanThucTe] decimal(18,3) NULL,
        [GhiChuChenhLech] nvarchar(max) NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CaThuNgans] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_CaThuNgan_TienDauCa] CHECK ([TienDauCa] >= 0),
        CONSTRAINT [FK_CaThuNgans_NhanViens_NhanVienId] FOREIGN KEY ([NhanVienId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [DonHangs] (
        [Id] int NOT NULL IDENTITY,
        [LoaiDon] nvarchar(30) NOT NULL,
        [NgayLap] datetime2 NOT NULL,
        [NgayGioLay] datetime2 NULL,
        [TongTienHang] decimal(18,3) NOT NULL,
        [TienGiamGia] decimal(18,3) NOT NULL,
        [DiemSuDung] int NOT NULL,
        [TongThanhToan] decimal(18,3) NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [NhanVienId] int NOT NULL,
        [KhachHangId] int NULL,
        [KhuyenMaiId] int NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_DonHangs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DonHangs_KhachHangs_KhachHangId] FOREIGN KEY ([KhachHangId]) REFERENCES [KhachHangs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonHangs_KhuyenMais_KhuyenMaiId] FOREIGN KEY ([KhuyenMaiId]) REFERENCES [KhuyenMais] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonHangs_NhanViens_NhanVienId] FOREIGN KEY ([NhanVienId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [PhieuKiemKes] (
        [Id] int NOT NULL IDENTITY,
        [NguoiThucHienId] int NOT NULL,
        [NguoiDuyetId] int NULL,
        [NgayKiemKe] datetime2 NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [NgayDuyet] datetime2 NULL,
        [LyDoTuChoi] nvarchar(max) NULL,
        CONSTRAINT [PK_PhieuKiemKes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhieuKiemKes_NhanViens_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuKiemKes_NhanViens_NguoiThucHienId] FOREIGN KEY ([NguoiThucHienId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [PhieuXuatHuys] (
        [Id] int NOT NULL IDENTITY,
        [NguoiLapId] int NOT NULL,
        [NguoiDuyetId] int NULL,
        [NgayLap] datetime2 NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [NgayDuyet] datetime2 NULL,
        [LyDoTuChoi] nvarchar(max) NULL,
        CONSTRAINT [PK_PhieuXuatHuys] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhieuXuatHuys_NhanViens_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuXuatHuys_NhanViens_NguoiLapId] FOREIGN KEY ([NguoiLapId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [DinhLuongs] (
        [SanPhamId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [DinhMuc] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_DinhLuongs] PRIMARY KEY ([SanPhamId], [NguyenLieuId]),
        CONSTRAINT [CK_DinhLuong_DinhMuc] CHECK ([DinhMuc] > 0),
        CONSTRAINT [FK_DinhLuongs_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DinhLuongs_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [LoSanPhams] (
        [Id] int NOT NULL IDENTITY,
        [SanPhamId] int NOT NULL,
        [NgaySanXuat] datetime2 NOT NULL,
        [HanSuDung] datetime2 NOT NULL,
        [SoLuongSanXuat] int NOT NULL,
        [SoLuongTon] int NOT NULL,
        [TyLeGiamXaHang] decimal(18,3) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_LoSanPhams] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LoSanPham_Ton] CHECK ([SoLuongTon] >= 0 AND [TyLeGiamXaHang] >= 0 AND [TyLeGiamXaHang] <= 1),
        CONSTRAINT [FK_LoSanPhams_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [ThanhToans] (
        [Id] int NOT NULL IDENTITY,
        [DonHangId] int NOT NULL,
        [CaThuNganId] int NOT NULL,
        [Loai] nvarchar(30) NOT NULL,
        [PhuongThuc] nvarchar(30) NOT NULL,
        [SoTien] decimal(18,3) NOT NULL,
        [ThoiGian] datetime2 NOT NULL,
        CONSTRAINT [PK_ThanhToans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThanhToans_CaThuNgans_CaThuNganId] FOREIGN KEY ([CaThuNganId]) REFERENCES [CaThuNgans] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThanhToans_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [ChiTietKiemKes] (
        [Id] int NOT NULL IDENTITY,
        [PhieuKiemKeId] int NOT NULL,
        [LoNguyenLieuId] int NOT NULL,
        [SoLuongHeThong] decimal(18,3) NOT NULL,
        [SoLuongThucTe] decimal(18,3) NOT NULL,
        [LyDo] nvarchar(max) NULL,
        CONSTRAINT [PK_ChiTietKiemKes] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ChiTietKiemKe_LyDo] CHECK ([SoLuongThucTe] = [SoLuongHeThong] OR [LyDo] IS NOT NULL),
        CONSTRAINT [FK_ChiTietKiemKes_LoNguyenLieus_LoNguyenLieuId] FOREIGN KEY ([LoNguyenLieuId]) REFERENCES [LoNguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietKiemKes_PhieuKiemKes_PhieuKiemKeId] FOREIGN KEY ([PhieuKiemKeId]) REFERENCES [PhieuKiemKes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [ChiTietDonHangs] (
        [Id] int NOT NULL IDENTITY,
        [DonHangId] int NOT NULL,
        [SanPhamId] int NOT NULL,
        [LoSanPhamId] int NULL,
        [SoLuong] int NOT NULL,
        [DonGia] decimal(18,3) NOT NULL,
        [TienGiam] decimal(18,3) NOT NULL,
        [ThanhTien] decimal(18,3) NOT NULL,
        [LoiChuc] nvarchar(max) NULL,
        CONSTRAINT [PK_ChiTietDonHangs] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ChiTietDonHang_SoLuong] CHECK ([SoLuong] > 0),
        CONSTRAINT [FK_ChiTietDonHangs_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChiTietDonHangs_LoSanPhams_LoSanPhamId] FOREIGN KEY ([LoSanPhamId]) REFERENCES [LoSanPhams] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietDonHangs_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE TABLE [ChiTietXuatHuys] (
        [Id] int NOT NULL IDENTITY,
        [PhieuXuatHuyId] int NOT NULL,
        [LoNguyenLieuId] int NULL,
        [LoSanPhamId] int NULL,
        [DonHangId] int NULL,
        [SoLuongHuy] decimal(18,3) NOT NULL,
        [DonGiaVon] decimal(18,3) NOT NULL,
        [LyDo] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_ChiTietXuatHuys] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ChiTietXuatHuy_SoLuong] CHECK ([SoLuongHuy] > 0),
        CONSTRAINT [CK_ChiTietXuatHuy_Xor] CHECK ((CASE WHEN [LoNguyenLieuId] IS NULL THEN 0 ELSE 1 END + CASE WHEN [LoSanPhamId] IS NULL THEN 0 ELSE 1 END + CASE WHEN [DonHangId] IS NULL THEN 0 ELSE 1 END) = 1),
        CONSTRAINT [FK_ChiTietXuatHuys_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietXuatHuys_LoNguyenLieus_LoNguyenLieuId] FOREIGN KEY ([LoNguyenLieuId]) REFERENCES [LoNguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietXuatHuys_LoSanPhams_LoSanPhamId] FOREIGN KEY ([LoSanPhamId]) REFERENCES [LoSanPhams] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietXuatHuys_PhieuXuatHuys_PhieuXuatHuyId] FOREIGN KEY ([PhieuXuatHuyId]) REFERENCES [PhieuXuatHuys] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_CaThuNgans_NhanVienId] ON [CaThuNgans] ([NhanVienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietDonHangs_DonHangId] ON [ChiTietDonHangs] ([DonHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietDonHangs_LoSanPhamId] ON [ChiTietDonHangs] ([LoSanPhamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietDonHangs_SanPhamId] ON [ChiTietDonHangs] ([SanPhamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietKiemKes_LoNguyenLieuId] ON [ChiTietKiemKes] ([LoNguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietKiemKes_PhieuKiemKeId] ON [ChiTietKiemKes] ([PhieuKiemKeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietXuatHuys_DonHangId] ON [ChiTietXuatHuys] ([DonHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietXuatHuys_LoNguyenLieuId] ON [ChiTietXuatHuys] ([LoNguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietXuatHuys_LoSanPhamId] ON [ChiTietXuatHuys] ([LoSanPhamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ChiTietXuatHuys_PhieuXuatHuyId] ON [ChiTietXuatHuys] ([PhieuXuatHuyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_DinhLuongs_NguyenLieuId] ON [DinhLuongs] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_DonHangs_KhachHangId] ON [DonHangs] ([KhachHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_DonHangs_KhuyenMaiId] ON [DonHangs] ([KhuyenMaiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_DonHangs_NhanVienId] ON [DonHangs] ([NhanVienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KhachHangs_SoDienThoai] ON [KhachHangs] ([SoDienThoai]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_LoNguyenLieus_NguyenLieuId] ON [LoNguyenLieus] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_LoSanPhams_SanPhamId] ON [LoSanPhams] ([SanPhamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NhanViens_TenDangNhap] ON [NhanViens] ([TenDangNhap]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_PhieuKiemKes_NguoiDuyetId] ON [PhieuKiemKes] ([NguoiDuyetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_PhieuKiemKes_NguoiThucHienId] ON [PhieuKiemKes] ([NguoiThucHienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_PhieuXuatHuys_NguoiDuyetId] ON [PhieuXuatHuys] ([NguoiDuyetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_PhieuXuatHuys_NguoiLapId] ON [PhieuXuatHuys] ([NguoiLapId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ThanhToans_CaThuNganId] ON [ThanhToans] ([CaThuNganId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    CREATE INDEX [IX_ThanhToans_DonHangId] ON [ThanhToans] ([DonHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002091137_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002091137_Initial', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002100911_DanhMucKho'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NguyenLieus]') AND [c].[name] = N'TenGoi');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [NguyenLieus] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [NguyenLieus] ALTER COLUMN [TenGoi] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002100911_DanhMucKho'
)
BEGIN
    ALTER TABLE [KhuyenMais] ADD [MaKhuyenMai] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002100911_DanhMucKho'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NguyenLieus_TenGoi] ON [NguyenLieus] ([TenGoi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002100911_DanhMucKho'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KhuyenMais_MaKhuyenMai] ON [KhuyenMais] ([MaKhuyenMai]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002100911_DanhMucKho'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002100911_DanhMucKho', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002111236_KiemKeDongThoi'
)
BEGIN
    ALTER TABLE [PhieuKiemKes] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002111236_KiemKeDongThoi'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PhieuKiemKes_TrangThai] ON [PhieuKiemKes] ([TrangThai]) WHERE [TrangThai] IN (''NHAP'',''CHO_DUYET'')');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002111236_KiemKeDongThoi'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002111236_KiemKeDongThoi', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003042932_CaMoDuyNhat'
)
BEGIN
    DROP INDEX [IX_CaThuNgans_NhanVienId] ON [CaThuNgans];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003042932_CaMoDuyNhat'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CaThuNgans_NhanVienId] ON [CaThuNgans] ([NhanVienId]) WHERE [TrangThai] = ''DANG_MO''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003042932_CaMoDuyNhat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003042932_CaMoDuyNhat', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003043937_KhachHangRowVersion'
)
BEGIN
    ALTER TABLE [KhachHangs] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003043937_KhachHangRowVersion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003043937_KhachHangRowVersion', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003054754_PhieuHuyRowVersion'
)
BEGIN
    ALTER TABLE [PhieuXuatHuys] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003054754_PhieuHuyRowVersion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003054754_PhieuHuyRowVersion', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103024_TraHangBanLe'
)
BEGIN
    ALTER TABLE [DonHangs] ADD [LyDoTra] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020103024_TraHangBanLe'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020103024_TraHangBanLe', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    ALTER TABLE [ChiTietDonHangs] ADD [ThietKeBanhId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE TABLE [ThanhPhanTrangTris] (
        [Id] int NOT NULL IDENTITY,
        [Nhom] nvarchar(30) NOT NULL,
        [Ten] nvarchar(max) NOT NULL,
        [AnhMinhHoa] nvarchar(max) NULL,
        [GiaBan] decimal(18,2) NOT NULL,
        [SoLuongToiDa] int NOT NULL,
        [SanPhamNenId] int NULL,
        [DangKinhDoanh] bit NOT NULL,
        CONSTRAINT [PK_ThanhPhanTrangTris] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThanhPhanTrangTris_SanPhams_SanPhamNenId] FOREIGN KEY ([SanPhamNenId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE TABLE [ThietKeBanhs] (
        [Id] int NOT NULL IDENTITY,
        [MaTraCuu] nvarchar(22) NOT NULL,
        [KhachHangId] int NOT NULL,
        [SanPhamNenId] int NOT NULL,
        [DuLieuJson] nvarchar(max) NOT NULL,
        [AnhThietKe] nvarchar(max) NOT NULL,
        [AnhInAn] nvarchar(max) NULL,
        [ChuTrenBanh] nvarchar(max) NULL,
        [LoiNhan] nvarchar(max) NULL,
        [NgayGioLay] datetime2 NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [GiaTamTinh] decimal(18,2) NOT NULL,
        [PhuPhi] decimal(18,2) NOT NULL,
        [LyDoPhuPhi] nvarchar(max) NULL,
        [GiaCuoi] decimal(18,2) NOT NULL,
        [GiaVonDuKien] decimal(18,2) NOT NULL,
        [GhiChuBep] nvarchar(max) NULL,
        [NgayGui] datetime2 NULL,
        [NgayDuyet] datetime2 NULL,
        [NguoiDuyetId] int NULL,
        [HanBaoGia] datetime2 NULL,
        [DonHangId] int NULL,
        [AnhThanhPham] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ThietKeBanhs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThietKeBanhs_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThietKeBanhs_KhachHangs_KhachHangId] FOREIGN KEY ([KhachHangId]) REFERENCES [KhachHangs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThietKeBanhs_NhanViens_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThietKeBanhs_SanPhams_SanPhamNenId] FOREIGN KEY ([SanPhamNenId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE TABLE [DinhLuongThanhPhans] (
        [ThanhPhanId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [DinhMuc] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_DinhLuongThanhPhans] PRIMARY KEY ([ThanhPhanId], [NguyenLieuId]),
        CONSTRAINT [CK_DinhLuongThanhPhan_DinhMuc] CHECK ([DinhMuc] > 0),
        CONSTRAINT [FK_DinhLuongThanhPhans_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DinhLuongThanhPhans_ThanhPhanTrangTris_ThanhPhanId] FOREIGN KEY ([ThanhPhanId]) REFERENCES [ThanhPhanTrangTris] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE TABLE [ThietKeThanhPhans] (
        [Id] int NOT NULL IDENTITY,
        [ThietKeId] int NOT NULL,
        [ThanhPhanId] int NOT NULL,
        [SoLuong] int NOT NULL,
        [DonGia] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ThietKeThanhPhans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThietKeThanhPhans_ThanhPhanTrangTris_ThanhPhanId] FOREIGN KEY ([ThanhPhanId]) REFERENCES [ThanhPhanTrangTris] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThietKeThanhPhans_ThietKeBanhs_ThietKeId] FOREIGN KEY ([ThietKeId]) REFERENCES [ThietKeBanhs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ChiTietDonHangs_ThietKeBanhId] ON [ChiTietDonHangs] ([ThietKeBanhId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_DinhLuongThanhPhans_NguyenLieuId] ON [DinhLuongThanhPhans] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThanhPhanTrangTris_SanPhamNenId] ON [ThanhPhanTrangTris] ([SanPhamNenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThietKeBanhs_DonHangId] ON [ThietKeBanhs] ([DonHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThietKeBanhs_KhachHangId] ON [ThietKeBanhs] ([KhachHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ThietKeBanhs_MaTraCuu] ON [ThietKeBanhs] ([MaTraCuu]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThietKeBanhs_NguoiDuyetId] ON [ThietKeBanhs] ([NguoiDuyetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThietKeBanhs_SanPhamNenId] ON [ThietKeBanhs] ([SanPhamNenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThietKeThanhPhans_ThanhPhanId] ON [ThietKeThanhPhans] ([ThanhPhanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    CREATE INDEX [IX_ThietKeThanhPhans_ThietKeId] ON [ThietKeThanhPhans] ([ThietKeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    ALTER TABLE [ChiTietDonHangs] ADD CONSTRAINT [FK_ChiTietDonHangs_ThietKeBanhs_ThietKeBanhId] FOREIGN KEY ([ThietKeBanhId]) REFERENCES [ThietKeBanhs] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022085921_DatBanhTuyChinh'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022085921_DatBanhTuyChinh', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    ALTER TABLE [KhachHangs] ADD [DiaChi] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    ALTER TABLE [KhachHangs] ADD [Email] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    ALTER TABLE [KhachHangs] ADD [MatKhauHash] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    ALTER TABLE [KhachHangs] ADD [NgayTao] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    CREATE TABLE [GopYs] (
        [Id] int NOT NULL IDENTITY,
        [KhachHangId] int NULL,
        [HoTen] nvarchar(max) NOT NULL,
        [SoDienThoai] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [DiemDanhGia] int NOT NULL,
        [TieuDe] nvarchar(max) NOT NULL,
        [NoiDung] nvarchar(max) NOT NULL,
        [NgayGui] datetime2 NOT NULL,
        [TrangThai] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_GopYs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GopYs_KhachHangs_KhachHangId] FOREIGN KEY ([KhachHangId]) REFERENCES [KhachHangs] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    CREATE INDEX [IX_GopYs_KhachHangId] ON [GopYs] ([KhachHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101113553_KhachHangPortal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101113553_KhachHangPortal', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101132227_SanPhamHinhAnh'
)
BEGIN
    ALTER TABLE [SanPhams] ADD [HinhAnh] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101132227_SanPhamHinhAnh'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101132227_SanPhamHinhAnh', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [CaNhanViens] (
        [Id] int NOT NULL IDENTITY,
        [NhanVienId] int NOT NULL,
        [LoaiCa] nvarchar(30) NOT NULL,
        [GioMoCa] datetime2 NOT NULL,
        [GioDongCa] datetime2 NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [GhiChu] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CaNhanViens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CaNhanViens_NhanViens_NhanVienId] FOREIGN KEY ([NhanVienId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [PhieuXuatKhos] (
        [Id] int NOT NULL IDENTITY,
        [NhanVienXuatId] int NOT NULL,
        [NgayXuat] datetime2 NOT NULL,
        [GhiChu] nvarchar(max) NULL,
        CONSTRAINT [PK_PhieuXuatKhos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhieuXuatKhos_NhanViens_NhanVienXuatId] FOREIGN KEY ([NhanVienXuatId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [PhieuBaoLoiMeBanhs] (
        [Id] int NOT NULL IDENTITY,
        [CaNhanVienId] int NOT NULL,
        [NhanVienLapId] int NOT NULL,
        [NguoiDuyetId] int NULL,
        [SanPhamId] int NULL,
        [SoLuongHong] int NOT NULL,
        [LyDoLoi] nvarchar(max) NOT NULL,
        [GhiChuQuanLy] nvarchar(max) NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [NgayLap] datetime2 NOT NULL,
        [NgayDuyet] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PhieuBaoLoiMeBanhs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhieuBaoLoiMeBanhs_CaNhanViens_CaNhanVienId] FOREIGN KEY ([CaNhanVienId]) REFERENCES [CaNhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuBaoLoiMeBanhs_NhanViens_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuBaoLoiMeBanhs_NhanViens_NhanVienLapId] FOREIGN KEY ([NhanVienLapId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuBaoLoiMeBanhs_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [PhieuDinhLuongCas] (
        [Id] int NOT NULL IDENTITY,
        [CaNhanVienId] int NOT NULL,
        [NguoiLapId] int NOT NULL,
        [ThoiGianXuat] datetime2 NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [GhiChu] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PhieuDinhLuongCas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhieuDinhLuongCas_CaNhanViens_CaNhanVienId] FOREIGN KEY ([CaNhanVienId]) REFERENCES [CaNhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuDinhLuongCas_NhanViens_NguoiLapId] FOREIGN KEY ([NguoiLapId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [ChiTietXuatKhos] (
        [Id] int NOT NULL IDENTITY,
        [PhieuXuatKhoId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [SoLuong] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_ChiTietXuatKhos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiTietXuatKhos_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietXuatKhos_PhieuXuatKhos_PhieuXuatKhoId] FOREIGN KEY ([PhieuXuatKhoId]) REFERENCES [PhieuXuatKhos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [ChiTietBaoLois] (
        [Id] int NOT NULL IDENTITY,
        [PhieuBaoLoiId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [SoLuong] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_ChiTietBaoLois] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiTietBaoLois_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietBaoLois_PhieuBaoLoiMeBanhs_PhieuBaoLoiId] FOREIGN KEY ([PhieuBaoLoiId]) REFERENCES [PhieuBaoLoiMeBanhs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [TieuHaoDinhLuongCas] (
        [Id] int NOT NULL IDENTITY,
        [CaNhanVienId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [SoLuong] decimal(18,3) NOT NULL,
        [Nguon] nvarchar(30) NOT NULL,
        [DonHangId] int NULL,
        [SanPhamId] int NULL,
        [PhieuBaoLoiId] int NULL,
        [ThoiGian] datetime2 NOT NULL,
        [GhiChu] nvarchar(max) NULL,
        CONSTRAINT [PK_TieuHaoDinhLuongCas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TieuHaoDinhLuongCas_CaNhanViens_CaNhanVienId] FOREIGN KEY ([CaNhanVienId]) REFERENCES [CaNhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TieuHaoDinhLuongCas_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TieuHaoDinhLuongCas_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TieuHaoDinhLuongCas_PhieuBaoLoiMeBanhs_PhieuBaoLoiId] FOREIGN KEY ([PhieuBaoLoiId]) REFERENCES [PhieuBaoLoiMeBanhs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TieuHaoDinhLuongCas_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [BienBanDoiChieuKhos] (
        [Id] int NOT NULL IDENTITY,
        [PhieuDinhLuongCaId] int NOT NULL,
        [NguoiDoiChieuId] int NOT NULL,
        [ThoiGianDoiChieu] datetime2 NOT NULL,
        [TrangThai] nvarchar(30) NOT NULL,
        [GhiChuGiaiTrinh] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_BienBanDoiChieuKhos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BienBanDoiChieuKhos_NhanViens_NguoiDoiChieuId] FOREIGN KEY ([NguoiDoiChieuId]) REFERENCES [NhanViens] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BienBanDoiChieuKhos_PhieuDinhLuongCas_PhieuDinhLuongCaId] FOREIGN KEY ([PhieuDinhLuongCaId]) REFERENCES [PhieuDinhLuongCas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [ChiTietDinhLuongCas] (
        [Id] int NOT NULL IDENTITY,
        [PhieuDinhLuongCaId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [SoLuongDinhLuong] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_ChiTietDinhLuongCas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiTietDinhLuongCas_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietDinhLuongCas_PhieuDinhLuongCas_PhieuDinhLuongCaId] FOREIGN KEY ([PhieuDinhLuongCaId]) REFERENCES [PhieuDinhLuongCas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE TABLE [ChiTietDoiChieuKhos] (
        [Id] int NOT NULL IDENTITY,
        [BienBanDoiChieuKhoId] int NOT NULL,
        [NguyenLieuId] int NOT NULL,
        [SoLuongXuatKho] decimal(18,3) NOT NULL,
        [SoLuongDinhLuong] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_ChiTietDoiChieuKhos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiTietDoiChieuKhos_BienBanDoiChieuKhos_BienBanDoiChieuKhoId] FOREIGN KEY ([BienBanDoiChieuKhoId]) REFERENCES [BienBanDoiChieuKhos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChiTietDoiChieuKhos_NguyenLieus_NguyenLieuId] FOREIGN KEY ([NguyenLieuId]) REFERENCES [NguyenLieus] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_BienBanDoiChieuKhos_NguoiDoiChieuId] ON [BienBanDoiChieuKhos] ([NguoiDoiChieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BienBanDoiChieuKhos_PhieuDinhLuongCaId] ON [BienBanDoiChieuKhos] ([PhieuDinhLuongCaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CaNhanViens_NhanVienId_LoaiCa] ON [CaNhanViens] ([NhanVienId], [LoaiCa]) WHERE [TrangThai] = ''DANG_MO''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietBaoLois_NguyenLieuId] ON [ChiTietBaoLois] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietBaoLois_PhieuBaoLoiId] ON [ChiTietBaoLois] ([PhieuBaoLoiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietDinhLuongCas_NguyenLieuId] ON [ChiTietDinhLuongCas] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietDinhLuongCas_PhieuDinhLuongCaId] ON [ChiTietDinhLuongCas] ([PhieuDinhLuongCaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietDoiChieuKhos_BienBanDoiChieuKhoId] ON [ChiTietDoiChieuKhos] ([BienBanDoiChieuKhoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietDoiChieuKhos_NguyenLieuId] ON [ChiTietDoiChieuKhos] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietXuatKhos_NguyenLieuId] ON [ChiTietXuatKhos] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_ChiTietXuatKhos_PhieuXuatKhoId] ON [ChiTietXuatKhos] ([PhieuXuatKhoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuBaoLoiMeBanhs_CaNhanVienId] ON [PhieuBaoLoiMeBanhs] ([CaNhanVienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuBaoLoiMeBanhs_NguoiDuyetId] ON [PhieuBaoLoiMeBanhs] ([NguoiDuyetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuBaoLoiMeBanhs_NhanVienLapId] ON [PhieuBaoLoiMeBanhs] ([NhanVienLapId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuBaoLoiMeBanhs_SanPhamId] ON [PhieuBaoLoiMeBanhs] ([SanPhamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuDinhLuongCas_CaNhanVienId] ON [PhieuDinhLuongCas] ([CaNhanVienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuDinhLuongCas_NguoiLapId] ON [PhieuDinhLuongCas] ([NguoiLapId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_PhieuXuatKhos_NhanVienXuatId] ON [PhieuXuatKhos] ([NhanVienXuatId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_TieuHaoDinhLuongCas_CaNhanVienId] ON [TieuHaoDinhLuongCas] ([CaNhanVienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_TieuHaoDinhLuongCas_DonHangId] ON [TieuHaoDinhLuongCas] ([DonHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_TieuHaoDinhLuongCas_NguyenLieuId] ON [TieuHaoDinhLuongCas] ([NguyenLieuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_TieuHaoDinhLuongCas_PhieuBaoLoiId] ON [TieuHaoDinhLuongCas] ([PhieuBaoLoiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    CREATE INDEX [IX_TieuHaoDinhLuongCas_SanPhamId] ON [TieuHaoDinhLuongCas] ([SanPhamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101154503_QuanLyDinhLuongCa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101154503_QuanLyDinhLuongCa', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101160052_ThemBuoiCaNhanVien'
)
BEGIN
    ALTER TABLE [CaNhanViens] ADD [BuoiCa] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101160052_ThemBuoiCaNhanVien'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101160052_ThemBuoiCaNhanVien', N'10.0.12');
END;

COMMIT;
GO



-- ==================== 2. DU LIEU BANG (DATA INSERTS) ====================
-- =============================================
-- FRIENDSHIP BAKERY - DATABASE DATA EXPORT
-- Exported at: 2026-11-01 23:09:22
-- =============================================
USE [Friendship];
GO

-- Data for table [__EFMigrationsHistory] (12 rows)
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
(N'20261002091137_Initial', N'10.0.12'),
(N'20261002100911_DanhMucKho', N'10.0.12'),
(N'20261002111236_KiemKeDongThoi', N'10.0.12'),
(N'20261003042932_CaMoDuyNhat', N'10.0.12'),
(N'20261003043937_KhachHangRowVersion', N'10.0.12'),
(N'20261003054754_PhieuHuyRowVersion', N'10.0.12'),
(N'20261020103024_TraHangBanLe', N'10.0.12'),
(N'20261022085921_DatBanhTuyChinh', N'10.0.12'),
(N'20261101113553_KhachHangPortal', N'10.0.12'),
(N'20261101132227_SanPhamHinhAnh', N'10.0.12'),
(N'20261101154503_QuanLyDinhLuongCa', N'10.0.12'),
(N'20261101160052_ThemBuoiCaNhanVien', N'10.0.12');
GO

-- Data for table [NhanViens] (5 rows)
SET IDENTITY_INSERT [NhanViens] ON;
INSERT INTO [NhanViens] ([Id], [HoTen], [GioiTinh], [NgaySinh], [SoDienThoai], [Email], [TenDangNhap], [MatKhauHash], [VaiTro], [DangHoatDong]) VALUES
(1, N'Quản trị viên', NULL, NULL, N'0900000001', NULL, N'admin', N'AQAAAAIAAYagAAAAENMC8lcWqQUM1IvwI7XHprqNZU+uIDlSr4a1XfyMMRfTZQS0ZPOG5pKla+ism4cK+g==', N'QUAN_TRI', 1),
(2, N'Quản lý', NULL, NULL, N'0900000002', NULL, N'quanly', N'AQAAAAIAAYagAAAAEFlrgwNPgrd6+lW6tL5OrFJsLa1AsCAUE8dehYPZTN9+ZNa/BDmhWVBcUb5/cuWM/Q==', N'QUAN_LY', 1),
(3, N'Nhân viên', NULL, NULL, N'0900000003', NULL, N'thungan', N'AQAAAAIAAYagAAAAEP9iJdpjcCz0y6AFP8lTvZQcqEjZzR2JJXSOCajV22EfS2Tmuy8IhRIm6GVDXl3sog==', N'BAN_HANG', 1),
(4, N'Thợ bánh', NULL, NULL, N'0900000004', NULL, N'bep', N'AQAAAAIAAYagAAAAEMq1rtR0dpIPuVbZmY+G45+adzzqSR7kGt4GQe0665evBLoVaLHS8HICD1UHJhzMJA==', N'SAN_XUAT', 1),
(5, N'Thủ kho', NULL, NULL, N'0900000005', NULL, N'kho', N'AQAAAAIAAYagAAAAEKO5x6FHr+O+IuG1Ds2x7ZraP8TxmEGCwrI/a+U0BCXm3O3Ix/Vn9KhoVVrZeRNyTA==', N'KHO', 1);
SET IDENTITY_INSERT [NhanViens] OFF;
GO

-- Data for table [KhachHangs] (9 rows)
SET IDENTITY_INSERT [KhachHangs] ON;
INSERT INTO [KhachHangs] ([Id], [TenKhachHang], [SoDienThoai], [DiemTichLuy], [DiaChi], [Email], [MatKhauHash], [NgayTao]) VALUES
(1, N'Nguyễn Thị Lan', N'0901000001', 48, NULL, NULL, NULL, NULL),
(2, N'Trần Văn Nam', N'0901000002', 12, NULL, NULL, NULL, NULL),
(3, N'Lê Hoa', N'0901000003', 80, NULL, NULL, NULL, NULL),
(4, N'Phạm Minh', N'0901000004', 8, NULL, NULL, NULL, NULL),
(5, N'Võ Thanh Tâm', N'0901000005', 0, NULL, NULL, NULL, NULL),
(6, N'Nguyễn Minh Anh', N'0988776655', 0, N'123 Lê Lợi, Phường Bến Nghé, Quận 1', N'minhanh@gmail.com', N'AQAAAAIAAYagAAAAEJDKaBPq+5myQhlaMV6gC/kkeG05M3JJV2tfcwuBiAPhRf08xEkulV9AZT/O8Iclhw==', '2026-11-01 18:42:29.111'),
(7, N'MrBao', N'0941454765', 0, N'trang Dai', N'bao@gmail.com', N'AQAAAAIAAYagAAAAELihRqVU82mqDHJ0fbcmdKIMJGIaYPfAmQZNo6pI2nuR2676vfOAhHjrEwAkwp/+Kg==', '2026-11-01 19:06:44.798'),
(8, N'MrBao', N'0123456789', 0, N'trang Dai', N'bao@gmail.com', N'AQAAAAIAAYagAAAAEGK88NZ4NmtfdWo+hYLhW7gSt2siExWR3jChELQJlNZ893FX71EQ3dpJ8SNU9LhtEA==', '2026-11-01 19:29:52.551'),
(9, N'Bao Luu Tien', N'0900000005', 0, NULL, NULL, NULL, '2026-11-01 21:11:23.982');
SET IDENTITY_INSERT [KhachHangs] OFF;
GO

-- Data for table [SanPhams] (13 rows)
SET IDENTITY_INSERT [SanPhams] ON;
INSERT INTO [SanPhams] ([Id], [TenBanh], [KichThuoc], [DonGia], [HanSuDungGio], [DangKinhDoanh], [HinhAnh]) VALUES
(1, N'Bánh su kem', N'Cái', 15000.000, 24, 1, NULL),
(2, N'Tart trứng', N'Cái', 25000.000, 24, 1, NULL),
(3, N'Bông lan', N'18cm', 120000.000, 48, 1, NULL),
(4, N'Bánh kem sinh nhật', N'20cm', 450000.000, 72, 1, NULL),
(5, N'Croissant', N'Cái', 20000.000, 12, 1, NULL),
(6, N'Bánh mì bơ tỏi phô mai', N'Cái', 18000.000, 24, 1, NULL),
(7, N'Bánh flan', N'Hũ', 12000.000, 48, 1, NULL),
(8, N'Cookie socola', N'Hộp 10 cái', 65000.000, 168, 1, NULL),
(9, N'Bông lan trứng muối', N'Hộp', 85000.000, 48, 1, NULL),
(10, N'Bánh kem matcha dâu', N'16cm', 320000.000, 72, 1, NULL),
(11, N'Bánh kem tùy chỉnh', N'16cm', 250000.000, 72, 1, NULL),
(12, N'Bánh kem tùy chỉnh', N'20cm', 320000.000, 72, 1, NULL),
(13, N'Bánh kem tùy chỉnh', N'24cm', 420000.000, 72, 1, NULL);
SET IDENTITY_INSERT [SanPhams] OFF;
GO

-- Data for table [NguyenLieus] (17 rows)
SET IDENTITY_INSERT [NguyenLieus] ON;
INSERT INTO [NguyenLieus] ([Id], [TenGoi], [DonViTinh], [MucCanhBaoTon]) VALUES
(1, N'Bột mì', N'kg', 10.000),
(2, N'Đường', N'kg', 5.000),
(3, N'Trứng gà', N'quả', 60.000),
(4, N'Bơ', N'kg', 3.000),
(5, N'Sữa tươi', N'lít', 5.000),
(6, N'Kem tươi', N'lít', 3.000),
(7, N'Socola', N'kg', 2.000),
(8, N'Men nở', N'g', 200.000),
(9, N'Tỏi', N'kg', 0.500),
(10, N'Phô mai', N'kg', 1.000),
(11, N'Trứng muối', N'quả', 20.000),
(12, N'Chà bông', N'kg', 0.500),
(13, N'Vani', N'ml', 50.000),
(14, N'Matcha', N'kg', 0.200),
(15, N'Dâu tây', N'kg', 1.000),
(16, N'Giấy in ăn được', N'tờ', 20.000),
(17, N'Cherry', N'kg', 0.500);
SET IDENTITY_INSERT [NguyenLieus] OFF;
GO

-- Data for table [DinhLuongs] (52 rows)
INSERT INTO [DinhLuongs] ([SanPhamId], [NguyenLieuId], [DinhMuc]) VALUES
(1, 1, 0.030),
(1, 3, 0.500),
(1, 4, 0.010),
(1, 6, 0.030),
(2, 1, 0.040),
(2, 2, 0.010),
(2, 3, 1.000),
(2, 4, 0.020),
(3, 1, 0.200),
(3, 2, 0.100),
(3, 3, 4.000),
(3, 5, 0.100),
(4, 1, 0.300),
(4, 2, 0.150),
(4, 3, 6.000),
(4, 6, 0.500),
(4, 7, 0.100),
(5, 1, 0.050),
(5, 2, 0.005),
(5, 4, 0.040),
(5, 8, 2.000),
(6, 1, 0.060),
(6, 4, 0.015),
(6, 8, 2.000),
(6, 9, 0.005),
(6, 10, 0.010),
(7, 2, 0.020),
(7, 3, 1.000),
(7, 5, 0.080),
(7, 13, 1.000),
(8, 1, 0.150),
(8, 2, 0.060),
(8, 3, 1.000),
(8, 4, 0.080),
(8, 7, 0.080),
(9, 1, 0.120),
(9, 2, 0.050),
(9, 3, 3.000),
(9, 11, 2.000),
(9, 12, 0.030),
(10, 1, 0.200),
(10, 2, 0.100),
(10, 3, 4.000),
(10, 6, 0.400),
(10, 14, 0.020),
(10, 15, 0.200),
(11, 4, 0.050),
(11, 6, 0.300),
(12, 4, 0.080),
(12, 6, 0.400),
(13, 4, 0.100),
(13, 6, 0.500);
GO

-- Data for table [LoNguyenLieus] (22 rows)
SET IDENTITY_INSERT [LoNguyenLieus] ON;
INSERT INTO [LoNguyenLieus] ([Id], [NguyenLieuId], [NgayNhap], [HanSuDung], [SoLuongNhap], [SoLuongCon], [DonGiaNhap]) VALUES
(1, 1, '2026-10-12 00:00:00.000', '2026-10-31 00:00:00.000', 2.000, 0.000, 22000.000),
(2, 1, '2026-10-22 00:00:00.000', '2026-11-03 00:00:00.000', 5.000, 5.000, 22000.000),
(3, 1, '2026-10-30 00:00:00.000', '2027-01-30 00:00:00.000', 25.000, 25.000, 24000.000),
(4, 2, '2026-10-27 00:00:00.000', '2027-03-01 00:00:00.000', 20.000, 19.500, 18000.000),
(5, 3, '2026-10-29 00:00:00.000', '2026-11-06 00:00:00.000', 100.000, 94.000, 3000.000),
(6, 3, '2026-10-31 00:00:00.000', '2026-11-13 00:00:00.000', 200.000, 200.000, 3200.000),
(7, 4, '2026-10-17 00:00:00.000', '2026-11-04 00:00:00.000', 2.000, 2.000, 150000.000),
(8, 4, '2026-10-30 00:00:00.000', '2026-12-01 00:00:00.000', 6.000, 6.000, 155000.000),
(9, 5, '2026-10-28 00:00:00.000', '2026-11-03 00:00:00.000', 6.000, 6.000, 28000.000),
(10, 6, '2026-10-26 00:00:00.000', '2026-11-05 00:00:00.000', 2.000, 2.000, 90000.000),
(11, 6, '2026-10-31 00:00:00.000', '2026-11-11 00:00:00.000', 6.000, 6.000, 92000.000),
(12, 7, '2026-10-25 00:00:00.000', '2027-05-20 00:00:00.000', 5.000, 5.000, 180000.000),
(13, 8, '2026-10-25 00:00:00.000', '2027-03-31 00:00:00.000', 500.000, 500.000, 400.000),
(14, 9, '2026-10-29 00:00:00.000', '2026-11-21 00:00:00.000', 1.000, 1.000, 60000.000),
(15, 10, '2026-10-28 00:00:00.000', '2026-11-26 00:00:00.000', 3.000, 3.000, 210000.000),
(16, 11, '2026-10-27 00:00:00.000', '2026-11-03 00:00:00.000', 40.000, 40.000, 6000.000),
(17, 12, '2026-10-26 00:00:00.000', '2026-12-01 00:00:00.000', 1.500, 1.500, 320000.000),
(18, 13, '2026-10-02 00:00:00.000', '2027-08-28 00:00:00.000', 200.000, 200.000, 900.000),
(19, 14, '2026-10-12 00:00:00.000', '2027-04-30 00:00:00.000', 0.500, 0.500, 1200000.000),
(20, 15, '2026-10-31 00:00:00.000', '2026-11-04 00:00:00.000', 0.800, 0.800, 150000.000),
(21, 16, '2026-10-30 00:00:00.000', '2027-04-30 00:00:00.000', 50.000, 50.000, 5000.000),
(22, 17, '2026-10-30 00:00:00.000', '2026-12-31 00:00:00.000', 3.000, 3.000, 220000.000);
SET IDENTITY_INSERT [LoNguyenLieus] OFF;
GO

-- Data for table [LoSanPhams] (6 rows)
SET IDENTITY_INSERT [LoSanPhams] ON;
INSERT INTO [LoSanPhams] ([Id], [SanPhamId], [NgaySanXuat], [HanSuDung], [SoLuongSanXuat], [SoLuongTon], [TyLeGiamXaHang]) VALUES
(1, 1, '2026-10-30 07:00:00.000', '2026-10-31 07:00:00.000', 30, 0, 0.000),
(2, 2, '2026-10-30 07:00:00.000', '2026-10-31 07:00:00.000', 15, 0, 0.000),
(3, 5, '2026-10-30 07:00:00.000', '2026-10-30 19:00:00.000', 20, 0, 0.000),
(4, 1, '2026-11-01 14:10:15.743', '2026-11-02 14:10:15.743', 24, 20, 0.000),
(5, 2, '2026-11-01 04:10:15.743', '2026-11-02 04:10:15.743', 12, 12, 0.000),
(6, 5, '2026-11-01 17:10:15.743', '2026-11-02 05:10:15.743', 16, 10, 0.000);
SET IDENTITY_INSERT [LoSanPhams] OFF;
GO

-- Data for table [KhuyenMais] (3 rows)
SET IDENTITY_INSERT [KhuyenMais] ON;
INSERT INTO [KhuyenMais] ([Id], [TenChuongTrinh], [LoaiGiam], [GiaTriGiam], [GiaTriDonToiThieu], [NgayBatDau], [NgayKetThuc], [DangApDung], [MaKhuyenMai]) VALUES
(1, N'Giảm 10% đơn từ 200.000đ', N'PHAN_TRAM', 10.000, 200000.000, '2026-10-22 00:00:00.000', '2026-12-21 00:00:00.000', 1, N'SINHNHAT10'),
(2, N'Giảm 20.000đ đơn từ 100.000đ', N'SO_TIEN', 20000.000, 100000.000, '2026-10-27 00:00:00.000', '2026-11-26 00:00:00.000', 1, N'GIAM20K'),
(3, N'Khuyến mãi tháng trước', N'PHAN_TRAM', 5.000, 0.000, '2026-09-22 00:00:00.000', '2026-10-22 00:00:00.000', 1, N'HETHAN5');
SET IDENTITY_INSERT [KhuyenMais] OFF;
GO

-- Data for table [ThanhPhanTrangTris] (19 rows)
SET IDENTITY_INSERT [ThanhPhanTrangTris] ON;
INSERT INTO [ThanhPhanTrangTris] ([Id], [Nhom], [Ten], [AnhMinhHoa], [GiaBan], [SoLuongToiDa], [SanPhamNenId], [DangKinhDoanh]) VALUES
(1, N'COT', N'Cốt vani 16cm', N'macaron.svg', 40000.00, 1, 11, 1),
(2, N'COT', N'Cốt vani 20cm', N'macaron.svg', 50000.00, 1, 12, 1),
(3, N'COT', N'Cốt vani 24cm', N'macaron.svg', 65000.00, 1, 13, 1),
(4, N'COT', N'Cốt socola 16cm', N'socola.svg', 45000.00, 1, 11, 1),
(5, N'COT', N'Cốt socola 20cm', N'socola.svg', 55000.00, 1, 12, 1),
(6, N'COT', N'Cốt socola 24cm', N'socola.svg', 70000.00, 1, 13, 1),
(7, N'COT', N'Cốt matcha 16cm', NULL, 50000.00, 1, 11, 1),
(8, N'COT', N'Cốt matcha 20cm', NULL, 60000.00, 1, 12, 1),
(9, N'COT', N'Cốt matcha 24cm', NULL, 75000.00, 1, 13, 1),
(10, N'VIEN', N'Viền kem xoắn 16cm', N'nen.svg', 15000.00, 1, 11, 1),
(11, N'VIEN', N'Viền kem xoắn 20cm', N'nen.svg', 20000.00, 1, 12, 1),
(12, N'VIEN', N'Viền kem xoắn 24cm', N'nen.svg', 25000.00, 1, 13, 1),
(13, N'TOPPING', N'Dâu tây', N'dau.svg', 5000.00, 12, NULL, 1),
(14, N'TOPPING', N'Cherry', N'cherry.svg', 4000.00, 12, NULL, 1),
(15, N'TOPPING', N'Macaron', N'macaron.svg', 8000.00, 8, NULL, 1),
(16, N'TOPPING', N'Socola miếng', N'socola.svg', 3000.00, 12, NULL, 1),
(17, N'IN_AN', N'Giấy in ăn được', N'in-hinh.svg', 30000.00, 1, NULL, 1),
(18, N'TANG', N'Tầng 2 (16cm)', N'tang-banh.svg', 120000.00, 1, 12, 1),
(19, N'TANG', N'Tầng 2 (18cm)', N'tang-banh.svg', 160000.00, 1, 13, 1);
SET IDENTITY_INSERT [ThanhPhanTrangTris] OFF;
GO

-- Data for table [DinhLuongThanhPhans] (55 rows)
INSERT INTO [DinhLuongThanhPhans] ([ThanhPhanId], [NguyenLieuId], [DinhMuc]) VALUES
(1, 1, 0.150),
(1, 2, 0.080),
(1, 3, 3.000),
(1, 13, 2.000),
(2, 1, 0.200),
(2, 2, 0.100),
(2, 3, 4.000),
(2, 13, 3.000),
(3, 1, 0.280),
(3, 2, 0.150),
(3, 3, 6.000),
(3, 13, 5.000),
(4, 1, 0.150),
(4, 2, 0.080),
(4, 3, 3.000),
(4, 7, 0.060),
(5, 1, 0.200),
(5, 2, 0.100),
(5, 3, 4.000),
(5, 7, 0.080),
(6, 1, 0.280),
(6, 2, 0.150),
(6, 3, 6.000),
(6, 7, 0.120),
(7, 1, 0.150),
(7, 2, 0.080),
(7, 3, 3.000),
(7, 14, 0.015),
(8, 1, 0.200),
(8, 2, 0.100),
(8, 3, 4.000),
(8, 14, 0.020),
(9, 1, 0.280),
(9, 2, 0.150),
(9, 3, 6.000),
(9, 14, 0.030),
(10, 2, 0.015),
(10, 6, 0.080),
(11, 2, 0.020),
(11, 6, 0.100),
(12, 2, 0.025),
(12, 6, 0.130),
(13, 15, 0.020),
(14, 17, 0.010),
(15, 1, 0.010),
(15, 2, 0.020),
(15, 3, 0.500),
(16, 7, 0.010),
(17, 16, 1.000),
(18, 1, 0.120),
(18, 3, 2.000),
(18, 6, 0.150),
(19, 1, 0.150),
(19, 3, 3.000),
(19, 6, 0.200);
GO

-- Data for table [ThietKeBanhs] (9 rows)
SET IDENTITY_INSERT [ThietKeBanhs] ON;
INSERT INTO [ThietKeBanhs] ([Id], [MaTraCuu], [KhachHangId], [SanPhamNenId], [DuLieuJson], [AnhThietKe], [AnhInAn], [ChuTrenBanh], [LoiNhan], [NgayGioLay], [TrangThai], [GiaTamTinh], [PhuPhi], [LyDoPhuPhi], [GiaCuoi], [GiaVonDuKien], [GhiChuBep], [NgayGui], [NgayDuyet], [NguoiDuyetId], [HanBaoGia], [DonHangId], [AnhThanhPham]) VALUES
(1, N'IBnlzZAiw1ChiHgdx1fqPQ', 1, 12, N'{"sanPhamNenId":12,"thanhPhan":[{"id":2,"soLuong":1},{"id":11,"soLuong":1},{"id":13,"soLuong":6}]}', N'53f119b3797542aaa334040f44e42613.png', NULL, N'Mừng sinh nhật Minh', N'Giao bánh lúc 14h chiều', '2026-11-04 18:10:15.743', N'DA_BAO_GIA', 420000.00, 0.00, NULL, 420000.00, 98860.00, NULL, '2026-11-01 14:10:15.743', '2026-11-01 21:12:14.508', 4, '2026-11-02 21:12:14.508', NULL, NULL),
(2, N'bDHAvLRAWPWoaiP3zrhO1A', 2, 11, N'{"sanPhamNenId":11,"thanhPhan":[{"id":4,"soLuong":1},{"id":13,"soLuong":4}]}', N'e2b253f0811c4bdb91e8b0e438c411f0.png', NULL, N'Chúc mừng sinh nhật em gái yêu quý', NULL, '2026-11-04 18:10:15.743', N'CAN_SUA', 315000.00, 0.00, NULL, 315000.00, 95000.00, N'Chữ quá dài cho bánh 16cm, vui lòng rút gọn dưới 20 ký tự.', '2026-11-01 10:10:15.743', NULL, NULL, NULL, NULL, NULL),
(3, N'GPT6dtIX7dLjLXVp50CMHQ', 3, 12, N'{"sanPhamNenId":12,"thanhPhan":[{"id":8,"soLuong":1},{"id":11,"soLuong":1},{"id":17,"soLuong":1}]}', N'd2dcb6581bb2414699c76a0c4bd53ed9.png', N'477407b3db75477a825299bc5586a008.png', N'Kỷ niệm ngày cưới', NULL, '2026-11-05 18:10:15.743', N'DA_BAO_GIA', 430000.00, 20000.00, N'Họa tiết in ảnh cần xử lý màu sắc tỉ mỉ.', 450000.00, 135000.00, NULL, '2026-11-01 06:10:15.743', '2026-11-01 12:10:15.743', 4, '2026-11-02 12:10:15.743', NULL, NULL),
(4, N'xn5HVtxBeys6yt9J1xnL3g', 4, 12, N'{"sanPhamNenId":12,"thanhPhan":[{"id":2,"soLuong":1},{"id":11,"soLuong":1},{"id":13,"soLuong":8}]}', N'f04e13926a3348d68f48f3a28d947262.png', NULL, N'Mừng sinh nhật Mẹ', NULL, '2026-11-02 18:10:15.743', N'DA_DAT', 430000.00, 20000.00, N'Đính nhiều dâu tây tạo hình tim', 450000.00, 140000.00, NULL, '2026-10-30 18:10:15.743', '2026-10-30 20:10:15.743', 4, '2026-10-31 20:10:15.743', 12, NULL),
(5, N'cOGV361Ehb_qMT4WemPKDA', 5, 13, N'{"sanPhamNenId":13,"thanhPhan":[{"id":6,"soLuong":1},{"id":13,"soLuong":10}]}', N'fc2e8fea5f1942f695fa2afd3a4b057c.png', NULL, N'Happy Company 10 Years', NULL, '2026-10-31 18:10:15.743', N'DA_DAT', 540000.00, 50000.00, N'Bánh cỡ lớn 24cm cần hộp đặc biệt', 590000.00, 180000.00, NULL, '2026-10-29 18:10:15.743', '2026-10-29 20:10:15.743', 4, '2026-10-30 20:10:15.743', 13, N'ce403e82e93640a7a355e59575c90449.png'),
(6, N'rEQQDxZZGovtPTCdfDIshg', 8, 13, N'{"sanPhamNenId":13,"soTang":2,"thanhPhan":[{"id":3,"soLuong":1},{"id":12,"soLuong":1},{"id":19,"soLuong":1},{"id":14,"soLuong":2},{"id":15,"soLuong":1},{"id":13,"soLuong":1},{"id":16,"soLuong":1}],"soNen":0,"mauKem":"#5D4037","fontChu":"''Dancing Script'', cursive","mauChu":"#8B0000","thongTinKhach":{"hoTen":"Bao Luu Tien","soDienThoai":"0900000005","ngayGioLay":"2026-11-04T21:10","loiNhan":"xxxx"},"toppings":[{"thanhPhanId":14,"left":218,"top":130,"angle":10,"tang":2},{"thanhPhanId":15,"left":235,"top":307,"angle":-6,"tang":2},{"thanhPhanId":14,"left":109,"top":266,"angle":4,"tang":2},{"thanhPhanId":13,"left":331,"top":181,"angle":-3,"tang":1},{"thanhPhanId":16,"left":176,"top":216,"angle":-2,"tang":2}],"canvas":{"version":"5.1.0","objects":[{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":344,"height":344,"fill":{"type":"linear","coords":{"x1":28,"y1":28,"x2":372,"y2":372},"colorStops":[{"offset":0,"color":"#E5C06E"},{"offset":0.25,"color":"#FFF3CF"},{"offset":0.5,"color":"#B5872E"},{"offset":0.75,"color":"#FCE7B3"},{"offset":1,"color":"#9E711E"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#E2BD68","strokeWidth":2,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 20, 10, 0.18)","blur":16,"offsetX":0,"offsetY":6,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":172,"startAngle":0,"endAngle":360,"customType":"base_board","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":310,"height":310,"fill":{"type":"radial","coords":{"x1":175,"y1":170,"x2":200,"y2":200,"r1":15,"r2":155},"colorStops":[{"offset":0,"color":"#775a51"},{"offset":0.65,"color":"#5D4037"},{"offset":1,"color":"#492c23"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#391c13","strokeWidth":1.5,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":155,"startAngle":0,"endAngle":360,"customType":"base_circle","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":160,"height":98.36,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":14,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.16,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"base_swirls","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"path":[["M",120,180],["C",130,130,260,120,280,180],["C",260,250,150,250,140,210]]},{"type":"group","version":"5.1.0","originX":"left","originY":"top","left":37.2,"top":37.2,"width":325.6,"height":325.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"cake_vien","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":151,"top":0,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":90,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":148.71,"top":26.22,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":100,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":141.89,"top":51.65,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":110,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":130.77,"top":75.5,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":120,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":115.67,"top":97.06,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":130,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":97.06,"top":115.67,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":140,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":75.5,"top":130.77,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":150,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":51.65,"top":141.89,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":160,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":26.22,"top":148.71,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":170,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":0,"top":151,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":180,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-26.22,"top":148.71,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":190,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-51.65,"top":141.89,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":200,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-75.5,"top":130.77,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":210,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-97.06,"top":115.67,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":220,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-115.67,"top":97.06,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":230,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-130.77,"top":75.5,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":240,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-141.89,"top":51.65,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":250,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-148.71,"top":26.22,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":260,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-151,"top":0,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":270,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-148.71,"top":-26.22,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":280,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-141.89,"top":-51.65,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":290,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-130.77,"top":-75.5,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":300,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-115.67,"top":-97.06,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":310,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-97.06,"top":-115.67,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":320,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-75.5,"top":-130.77,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":330,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-51.65,"top":-141.89,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":340,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-26.22,"top":-148.71,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":350,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":0,"top":-151,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":360,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":26.22,"top":-148.71,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":370,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":51.65,"top":-141.89,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":380,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":75.5,"top":-130.77,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":390,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":97.06,"top":-115.67,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":400,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":115.67,"top":-97.06,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":410,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":130.77,"top":-75.5,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":420,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":141.89,"top":-51.65,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":430,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":148.71,"top":-26.22,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":440,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]}]},{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":230,"height":230,"fill":{"type":"radial","coords":{"x1":175,"y1":170,"x2":200,"y2":200,"r1":15,"r2":155},"colorStops":[{"offset":0,"color":"#775a51"},{"offset":0.65,"color":"#5D4037"},{"offset":1,"color":"#492c23"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#391c13","strokeWidth":1.5,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 20, 10, 0.22)","blur":14,"offsetX":0,"offsetY":5,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":115,"startAngle":0,"endAngle":360,"customType":"tier2_circle","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"group","version":"5.1.0","originX":"left","originY":"top","left":79.01,"top":78.78,"width":242.02,"height":242.43,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"tier2_vien","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":110.98,"top":0,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":90,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":107.99,"top":25.6,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":103.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":99.17,"top":49.82,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":116.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":85.01,"top":71.35,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":130,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":66.27,"top":89.04,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":143.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":43.95,"top":101.92,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":156.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":19.26,"top":109.31,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":170,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-6.47,"top":110.81,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":183.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-31.85,"top":106.34,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":196.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-55.52,"top":96.13,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":210,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-76.19,"top":80.74,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":223.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-92.76,"top":61,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":236.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-104.32,"top":37.96,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":250,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-110.27,"top":12.89,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":263.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-110.27,"top":-12.89,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":276.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-104.32,"top":-37.96,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":290,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-92.76,"top":-61,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":303.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-76.19,"top":-80.74,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":316.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-55.52,"top":-96.13,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":330,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-31.85,"top":-106.34,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":343.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":-6.47,"top":-110.81,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":356.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":19.26,"top":-109.31,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":370,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":43.95,"top":-101.92,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":383.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":66.27,"top":-89.04,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":396.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":85.01,"top":-71.35,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":410,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":99.17,"top":-49.82,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":423.33,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]},{"type":"group","version":"5.1.0","originX":"center","originY":"center","left":107.99,"top":-25.6,"width":15.83,"height":23.6,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.85,"scaleY":0.85,"angle":436.67,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"objects":[{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":15.23,"height":23,"fill":"#83665d","stroke":"#55382f","strokeWidth":0.6,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-10],["C",6,-10,9,-5,7,2],["C",5,7,2,11,0,13],["C",-2,11,-5,7,-7,2],["C",-9,-5,-6,-10,0,-10],["Z"]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":0,"top":0,"width":3,"height":17,"fill":"transparent","stroke":"#55382f","strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.6,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",0,-8],["L",0,9],["M",-3,-6],["C",-2,0,0,4,0,8]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":1,"top":0,"width":2.39,"height":14,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":1.2,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.85,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"path":[["M",2,-7],["C",3,-2,2,3,0,7]]}]}]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":217.71,"top":130.13,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.32,"scaleY":0.32,"angle":10,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":14,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":2,"src":"http://localhost:5166/img/thanh-phan/cherry.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":235,"top":307,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.32,"scaleY":0.32,"angle":-6,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":15,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":2,"src":"http://localhost:5166/img/thanh-phan/macaron.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":109,"top":266,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.32,"scaleY":0.32,"angle":4,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":14,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":2,"src":"http://localhost:5166/img/thanh-phan/cherry.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":330.75,"top":181.25,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":-3,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":175.67,"top":216.29,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.32,"scaleY":0.32,"angle":-2,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":16,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":2,"src":"http://localhost:5166/img/thanh-phan/socola.svg","crossOrigin":"anonymous","filters":[]}]}}', N'54bef53b233d4fda80be7d0a9406bf05.png', NULL, NULL, N'xxxx', '2026-11-04 21:10:00.000', N'DA_BAO_GIA', 694000.00, 0.00, NULL, 694000.00, 150030.00, NULL, '2026-11-01 21:11:29.935', '2026-11-01 21:12:00.588', 4, '2026-11-02 21:12:00.588', NULL, NULL),
(7, N'wUY6nTTYgHw7_0_V0no4LA', 8, 11, N'{"sanPhamNenId":11,"soTang":1,"thanhPhan":[{"id":7,"soLuong":1},{"id":14,"soLuong":1},{"id":15,"soLuong":1}],"soNen":0,"mauKem":"#8FBC8F","fontChu":"''Pacifico'', cursive","mauChu":"#E91E63","thongTinKhach":{"hoTen":"Bao Luu Tien","soDienThoai":"0900000005","ngayGioLay":"2026-11-04T21:13","loiNhan":"sdad"},"toppings":[{"thanhPhanId":14,"left":200,"top":152,"angle":-3,"tang":1},{"thanhPhanId":15,"left":235,"top":307,"angle":8,"tang":1}],"canvas":{"version":"5.1.0","objects":[{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":344,"height":344,"fill":{"type":"linear","coords":{"x1":28,"y1":28,"x2":372,"y2":372},"colorStops":[{"offset":0,"color":"#E5C06E"},{"offset":0.25,"color":"#FFF3CF"},{"offset":0.5,"color":"#B5872E"},{"offset":0.75,"color":"#FCE7B3"},{"offset":1,"color":"#9E711E"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#E2BD68","strokeWidth":2,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 20, 10, 0.18)","blur":16,"offsetX":0,"offsetY":6,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":172,"startAngle":0,"endAngle":360,"customType":"base_board","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":310,"height":310,"fill":{"type":"radial","coords":{"x1":175,"y1":170,"x2":200,"y2":200,"r1":15,"r2":155},"colorStops":[{"offset":0,"color":"#a9d6a9"},{"offset":0.65,"color":"#8FBC8F"},{"offset":1,"color":"#7ba87b"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#6b986b","strokeWidth":1.5,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":155,"startAngle":0,"endAngle":360,"customType":"base_circle","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":160,"height":98.36,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":14,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.16,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"base_swirls","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"path":[["M",120,180],["C",130,130,260,120,280,180],["C",260,250,150,250,140,210]]},{"type":"i-text","version":"5.1.0","originX":"center","originY":"center","left":215,"top":231.04,"width":314.02,"height":24.86,"fill":"#E91E63","stroke":null,"strokeWidth":1,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.82,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"fontFamily":"''Pacifico'', cursive","fontWeight":"normal","fontSize":22,"text":"dasdasdasdasdasdsadsadasdas","underline":false,"overline":false,"linethrough":false,"textAlign":"left","fontStyle":"normal","lineHeight":1.16,"textBackgroundColor":"","charSpacing":0,"styles":{},"direction":"ltr","path":null,"pathStartOffset":0,"pathSide":"left","pathAlign":"baseline","customType":"chu","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":200,"top":152,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.32,"scaleY":0.32,"angle":-3,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":14,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/cherry.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":235,"top":307,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.32,"scaleY":0.32,"angle":8,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":15,"strawberryStyle":"bo_doi","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau-cat-doi.svg","crossOrigin":"anonymous","filters":[]}]}}', N'60f2320d610a4fb997290d00edaf28b7.png', NULL, N'dasdasdasdasdasdsadsadasdas', N'sdad', '2026-11-04 21:13:00.000', N'NHAP', 312000.00, 0.00, NULL, 0.00, 0.00, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(8, N'etNeRClupbVjEo3-40JRLg', 8, 11, N'{"sanPhamNenId":11,"soTang":1,"thanhPhan":[{"id":7,"soLuong":1}],"soNen":0,"mauKem":"#FFFDF5","fontChu":"''Dancing Script'', cursive","mauChu":"#8B0000","thongTinKhach":{"hoTen":"s","soDienThoai":"0900000005","ngayGioLay":"2026-11-04T21:15","loiNhan":"s"},"toppings":[],"canvas":{"version":"5.1.0","objects":[{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":344,"height":344,"fill":{"type":"linear","coords":{"x1":28,"y1":28,"x2":372,"y2":372},"colorStops":[{"offset":0,"color":"#E5C06E"},{"offset":0.25,"color":"#FFF3CF"},{"offset":0.5,"color":"#B5872E"},{"offset":0.75,"color":"#FCE7B3"},{"offset":1,"color":"#9E711E"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#E2BD68","strokeWidth":2,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 20, 10, 0.18)","blur":16,"offsetX":0,"offsetY":6,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":172,"startAngle":0,"endAngle":360,"customType":"base_board","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":310,"height":310,"fill":{"type":"radial","coords":{"x1":175,"y1":170,"x2":200,"y2":200,"r1":15,"r2":155},"colorStops":[{"offset":0,"color":"#ffffff"},{"offset":0.65,"color":"#FFFDF5"},{"offset":1,"color":"#ebe9e1"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#dbd9d1","strokeWidth":1.5,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":155,"startAngle":0,"endAngle":360,"customType":"base_circle","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":160,"height":98.36,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":14,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.16,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"base_swirls","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"path":[["M",120,180],["C",130,130,260,120,280,180],["C",260,250,150,250,140,210]]}]}}', N'cd2604a5eec04c6d988c0aeea5a98014.png', NULL, NULL, N's', '2026-11-04 21:15:00.000', N'DA_BAO_GIA', 300000.00, 0.00, NULL, 300000.00, 67990.00, NULL, '2026-11-01 21:15:59.278', '2026-11-01 21:16:47.786', 4, '2026-11-02 21:16:47.786', NULL, NULL),
(9, N'm6MpwT6CfR2Ibm1Y352ekw', 8, 11, N'{"sanPhamNenId":11,"soTang":1,"thanhPhan":[{"id":7,"soLuong":1},{"id":13,"soLuong":8}],"soNen":0,"mauKem":"#5D4037","fontChu":"''Dancing Script'', cursive","mauChu":"#8B0000","thongTinKhach":{"hoTen":"Bao Luu Tien","soDienThoai":"0900000005","ngayGioLay":"2026-11-04T21:18","loiNhan":"dsa"},"toppings":[{"thanhPhanId":13,"left":200,"top":82,"angle":0,"tang":1},{"thanhPhanId":13,"left":283,"top":117,"angle":45,"tang":1},{"thanhPhanId":13,"left":318,"top":200,"angle":90,"tang":1},{"thanhPhanId":13,"left":283,"top":283,"angle":135,"tang":1},{"thanhPhanId":13,"left":200,"top":318,"angle":180,"tang":1},{"thanhPhanId":13,"left":117,"top":283,"angle":-135,"tang":1},{"thanhPhanId":13,"left":82,"top":200,"angle":270,"tang":1},{"thanhPhanId":13,"left":117,"top":117,"angle":-45,"tang":1}],"canvas":{"version":"5.1.0","objects":[{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":344,"height":344,"fill":{"type":"linear","coords":{"x1":28,"y1":28,"x2":372,"y2":372},"colorStops":[{"offset":0,"color":"#E5C06E"},{"offset":0.25,"color":"#FFF3CF"},{"offset":0.5,"color":"#B5872E"},{"offset":0.75,"color":"#FCE7B3"},{"offset":1,"color":"#9E711E"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#E2BD68","strokeWidth":2,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 20, 10, 0.18)","blur":16,"offsetX":0,"offsetY":6,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":172,"startAngle":0,"endAngle":360,"customType":"base_board","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":344,"height":344,"fill":{"type":"linear","coords":{"x1":28,"y1":28,"x2":372,"y2":372},"colorStops":[{"offset":0,"color":"#E5C06E"},{"offset":0.25,"color":"#FFF3CF"},{"offset":0.5,"color":"#B5872E"},{"offset":0.75,"color":"#FCE7B3"},{"offset":1,"color":"#9E711E"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#E2BD68","strokeWidth":2,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 20, 10, 0.18)","blur":16,"offsetX":0,"offsetY":6,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":172,"startAngle":0,"endAngle":360,"customType":"base_board","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"circle","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":310,"height":310,"fill":{"type":"radial","coords":{"x1":175,"y1":170,"x2":200,"y2":200,"r1":15,"r2":155},"colorStops":[{"offset":0,"color":"#775a51"},{"offset":0.65,"color":"#5D4037"},{"offset":1,"color":"#492c23"}],"offsetX":0,"offsetY":0,"gradientUnits":"pixels","gradientTransform":null},"stroke":"#391c13","strokeWidth":1.5,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"radius":155,"startAngle":0,"endAngle":360,"customType":"base_circle","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":160,"height":98.36,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":14,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.16,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"base_swirls","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"path":[["M",120,180],["C",130,130,260,120,280,180],["C",260,250,150,250,140,210]]},{"type":"path","version":"5.1.0","originX":"center","originY":"center","left":200,"top":200,"width":160,"height":98.36,"fill":"transparent","stroke":"#FFFFFF","strokeWidth":14,"strokeDashArray":null,"strokeLineCap":"round","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":1,"scaleY":1,"angle":0,"flipX":false,"flipY":false,"opacity":0.16,"shadow":null,"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"customType":"base_swirls","selectable":false,"evented":false,"lockScalingX":false,"lockScalingY":false,"path":[["M",120,180],["C",130,130,260,120,280,180],["C",260,250,150,250,140,210]]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":200,"top":82,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":0,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":283,"top":117,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":45,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":318,"top":200,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":90,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"cat_lat","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau-lat.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":283,"top":283,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":135,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":200,"top":318,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":180,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":117,"top":283,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":-135,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":82,"top":200,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":270,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]},{"type":"image","version":"5.1.0","originX":"center","originY":"center","left":117,"top":117,"width":100,"height":100,"fill":"rgb(0,0,0)","stroke":null,"strokeWidth":0,"strokeDashArray":null,"strokeLineCap":"butt","strokeDashOffset":0,"strokeLineJoin":"miter","strokeUniform":false,"strokeMiterLimit":4,"scaleX":0.42,"scaleY":0.42,"angle":-45,"flipX":false,"flipY":false,"opacity":1,"shadow":{"color":"rgba(50, 10, 20, 0.32)","blur":10,"offsetX":0,"offsetY":4,"affectStroke":false,"nonScaling":false},"visible":true,"backgroundColor":"","fillRule":"nonzero","paintFirst":"fill","globalCompositeOperation":"source-over","skewX":0,"skewY":0,"cropX":0,"cropY":0,"customType":"topping","thanhPhanId":13,"strawberryStyle":"nguyen_qua","selectable":true,"evented":true,"lockScalingX":false,"lockScalingY":false,"tang":1,"src":"http://localhost:5166/img/thanh-phan/dau.svg","crossOrigin":"anonymous","filters":[]}]}}', N'e5ebef1da601428daae4e447f3eaef4b.png', NULL, NULL, N'dsa', '2026-11-04 21:18:00.000', N'DA_DAT', 340000.00, 0.00, NULL, 340000.00, 91990.00, NULL, '2026-11-01 21:50:26.599', '2026-11-01 21:51:09.385', 4, '2026-11-02 21:51:09.385', 17, NULL);
SET IDENTITY_INSERT [ThietKeBanhs] OFF;
GO

-- Data for table [DonHangs] (17 rows)
SET IDENTITY_INSERT [DonHangs] ON;
INSERT INTO [DonHangs] ([Id], [LoaiDon], [NgayLap], [NgayGioLay], [TongTienHang], [TienGiamGia], [DiemSuDung], [TongThanhToan], [TrangThai], [NhanVienId], [KhachHangId], [KhuyenMaiId], [LyDoTra]) VALUES
(1, N'BAN_LE', '2026-10-30 08:15:00.000', NULL, 100000.000, 0.000, 0, 100000.000, N'DA_THANH_TOAN', 3, NULL, NULL, NULL),
(2, N'DAT_TRUOC', '2026-10-30 09:00:00.000', '2026-10-30 16:00:00.000', 120000.000, 0.000, 0, 120000.000, N'DA_GIAO', 3, 2, NULL, NULL),
(3, N'DAT_TRUOC', '2026-10-30 09:30:00.000', '2026-10-30 18:00:00.000', 450000.000, 0.000, 0, 450000.000, N'BUNG_COC', 3, 4, NULL, NULL),
(4, N'DAT_TRUOC', '2026-10-30 10:00:00.000', '2026-11-04 10:00:00.000', 450000.000, 0.000, 0, 450000.000, N'DA_HUY', 3, 1, NULL, NULL),
(5, N'BAN_LE', '2026-10-30 10:30:00.000', NULL, 150000.000, 20000.000, 0, 130000.000, N'DA_THANH_TOAN', 3, 1, 2, NULL),
(6, N'BAN_LE', '2026-10-30 13:00:00.000', NULL, 150000.000, 0.000, 50, 100000.000, N'DA_THANH_TOAN', 3, 3, NULL, NULL),
(7, N'BAN_LE', '2026-10-30 17:30:00.000', NULL, 240000.000, 0.000, 0, 240000.000, N'DA_TRA', 3, NULL, NULL, N'Khách đổi ý trả lại'),
(8, N'DAT_TRUOC', '2026-10-30 18:00:00.000', '2026-11-01 21:10:15.743', 240000.000, 0.000, 0, 240000.000, N'CHO_GIAO', 3, 3, NULL, NULL),
(9, N'DAT_TRUOC', '2026-10-30 18:00:00.000', '2026-11-02 09:00:00.000', 500000.000, 0.000, 0, 500000.000, N'CHO_GIAO', 3, 2, NULL, NULL),
(10, N'DAT_TRUOC', '2026-10-30 19:00:00.000', '2026-11-03 15:00:00.000', 320000.000, 0.000, 0, 320000.000, N'CHO_GIAO', 3, 5, NULL, NULL),
(11, N'DAT_TRUOC', '2026-10-30 19:00:00.000', '2026-11-03 10:00:00.000', 195000.000, 0.000, 0, 195000.000, N'CHO_COC', 3, 4, NULL, NULL),
(12, N'DAT_TRUOC', '2026-10-30 22:10:15.743', '2026-11-02 18:10:15.743', 450000.000, 0.000, 0, 450000.000, N'CHO_GIAO', 3, 4, NULL, NULL),
(13, N'DAT_TRUOC', '2026-10-29 22:10:15.743', '2026-10-31 18:10:15.743', 590000.000, 0.000, 0, 590000.000, N'DA_GIAO', 3, 5, NULL, NULL),
(14, N'DAT_TRUOC', '2026-11-01 18:42:29.520', '2026-11-03 16:00:00.000', 30000.000, 0.000, 0, 30000.000, N'CHO_COC', 1, 6, NULL, N'[Giao hàng tận nơi: 123 Lê Lợi, Q1] Giao đúng giờ hẹn giúp em ạ'),
(15, N'DAT_TRUOC', '2026-11-01 19:07:07.786', '2026-11-01 23:06:54.074', 25000.000, 0.000, 0, 25000.000, N'CHO_COC', 1, 7, NULL, N'[Giao hàng tận nơi: trang Dai]'),
(16, N'DAT_TRUOC', '2026-11-01 19:49:39.981', '2026-11-01 23:49:35.782', 175000.000, 0.000, 0, 175000.000, N'CHO_COC', 1, 8, NULL, N'[Giao hàng tận nơi: trang Dai]'),
(17, N'DAT_TRUOC', '2026-11-01 22:00:35.516', '2026-11-04 21:18:00.000', 340000.000, 0.000, 0, 340000.000, N'CHO_GIAO', 3, 8, NULL, NULL);
SET IDENTITY_INSERT [DonHangs] OFF;
GO

-- Data for table [ChiTietDonHangs] (18 rows)
SET IDENTITY_INSERT [ChiTietDonHangs] ON;
INSERT INTO [ChiTietDonHangs] ([Id], [DonHangId], [SanPhamId], [LoSanPhamId], [SoLuong], [DonGia], [TienGiam], [ThanhTien], [LoiChuc], [ThietKeBanhId]) VALUES
(1, 1, 1, 1, 4, 15000.000, 0.000, 60000.000, NULL, NULL),
(2, 1, 5, 3, 2, 20000.000, 0.000, 40000.000, NULL, NULL),
(3, 2, 3, NULL, 1, 120000.000, 0.000, 120000.000, NULL, NULL),
(4, 3, 4, NULL, 1, 450000.000, 0.000, 450000.000, N'Mừng sinh nhật Minh', NULL),
(5, 4, 4, NULL, 1, 450000.000, 0.000, 450000.000, N'Chúc mừng 20/10', NULL),
(6, 5, 2, 2, 6, 25000.000, 0.000, 150000.000, NULL, NULL),
(7, 6, 1, 1, 10, 15000.000, 0.000, 150000.000, NULL, NULL),
(8, 7, 5, 3, 12, 20000.000, 0.000, 240000.000, NULL, NULL),
(9, 8, 3, NULL, 2, 120000.000, 0.000, 240000.000, NULL, NULL),
(10, 9, 2, NULL, 20, 25000.000, 0.000, 500000.000, NULL, NULL),
(11, 10, 10, NULL, 1, 320000.000, 0.000, 320000.000, N'Chúc mừng kỷ niệm', NULL),
(12, 11, 8, NULL, 3, 65000.000, 0.000, 195000.000, NULL, NULL),
(13, 12, 12, NULL, 1, 450000.000, 0.000, 450000.000, N'Mừng sinh nhật Mẹ', 4),
(14, 13, 13, NULL, 1, 590000.000, 0.000, 590000.000, N'Happy Company 10 Years', 5),
(15, 14, 1, NULL, 2, 15000.000, 0.000, 30000.000, N'Happy 20th Birthday', NULL),
(16, 15, 2, NULL, 1, 25000.000, 0.000, 25000.000, NULL, NULL),
(17, 16, 2, NULL, 7, 25000.000, 0.000, 175000.000, NULL, NULL),
(18, 17, 11, NULL, 1, 340000.000, 0.000, 340000.000, NULL, 9);
SET IDENTITY_INSERT [ChiTietDonHangs] OFF;
GO

-- Data for table [ThietKeThanhPhans] (26 rows)
SET IDENTITY_INSERT [ThietKeThanhPhans] ON;
INSERT INTO [ThietKeThanhPhans] ([Id], [ThietKeId], [ThanhPhanId], [SoLuong], [DonGia]) VALUES
(1, 1, 2, 1, 50000.00),
(2, 1, 11, 1, 20000.00),
(3, 1, 13, 6, 5000.00),
(4, 2, 4, 1, 45000.00),
(5, 2, 13, 4, 5000.00),
(6, 3, 8, 1, 60000.00),
(7, 3, 11, 1, 20000.00),
(8, 3, 17, 1, 30000.00),
(9, 4, 2, 1, 50000.00),
(10, 4, 11, 1, 20000.00),
(11, 4, 13, 8, 5000.00),
(12, 5, 6, 1, 70000.00),
(13, 5, 13, 10, 5000.00),
(14, 6, 3, 1, 65000.00),
(15, 6, 12, 1, 25000.00),
(16, 6, 19, 1, 160000.00),
(17, 6, 14, 2, 4000.00),
(18, 6, 15, 1, 8000.00),
(19, 6, 13, 1, 5000.00),
(20, 6, 16, 1, 3000.00),
(21, 7, 7, 1, 50000.00),
(22, 7, 14, 1, 4000.00),
(23, 7, 15, 1, 8000.00),
(24, 8, 7, 1, 50000.00),
(26, 9, 7, 1, 50000.00),
(27, 9, 13, 8, 5000.00);
SET IDENTITY_INSERT [ThietKeThanhPhans] OFF;
GO

-- Data for table [ThanhToans] (14 rows)
SET IDENTITY_INSERT [ThanhToans] ON;
INSERT INTO [ThanhToans] ([Id], [DonHangId], [CaThuNganId], [Loai], [PhuongThuc], [SoTien], [ThoiGian]) VALUES
(1, 1, 1, N'THANH_TOAN_DU', N'TIEN_MAT', 100000.000, '2026-10-30 08:15:00.000'),
(2, 2, 1, N'COC', N'TIEN_MAT', 50000.000, '2026-10-30 09:00:00.000'),
(3, 3, 1, N'COC', N'CHUYEN_KHOAN', 150000.000, '2026-10-30 09:30:00.000'),
(4, 4, 1, N'COC', N'TIEN_MAT', 150000.000, '2026-10-30 10:00:00.000'),
(5, 5, 1, N'THANH_TOAN_DU', N'CHUYEN_KHOAN', 130000.000, '2026-10-30 10:30:00.000'),
(6, 4, 1, N'HOAN_COC', N'TIEN_MAT', -150000.000, '2026-10-30 11:00:00.000'),
(7, 6, 1, N'THANH_TOAN_DU', N'TIEN_MAT', 100000.000, '2026-10-30 13:00:00.000'),
(8, 2, 1, N'TAT_TOAN', N'TIEN_MAT', 70000.000, '2026-10-30 16:30:00.000'),
(9, 7, 1, N'THANH_TOAN_DU', N'CHUYEN_KHOAN', 240000.000, '2026-10-30 17:30:00.000'),
(10, 7, 1, N'HOAN_TIEN', N'CHUYEN_KHOAN', -240000.000, '2026-10-30 17:45:00.000'),
(11, 8, 1, N'COC', N'CHUYEN_KHOAN', 100000.000, '2026-10-30 18:00:00.000'),
(12, 9, 1, N'COC', N'TIEN_MAT', 200000.000, '2026-10-30 18:00:00.000'),
(13, 10, 1, N'COC', N'CHUYEN_KHOAN', 100000.000, '2026-10-30 19:00:00.000'),
(14, 17, 2, N'COC', N'CHUYEN_KHOAN', 102000.000, '2026-11-01 22:00:35.620');
SET IDENTITY_INSERT [ThanhToans] OFF;
GO

-- Data for table [CaThuNgans] (2 rows)
SET IDENTITY_INSERT [CaThuNgans] ON;
INSERT INTO [CaThuNgans] ([Id], [NhanVienId], [GioMoCa], [GioDongCa], [TienDauCa], [TienMatHeThong], [ChuyenKhoanHeThong], [TienMatThucTe], [ChuyenKhoanThucTe], [GhiChuChenhLech], [TrangThai]) VALUES
(1, 3, '2026-10-30 07:30:00.000', '2026-10-30 21:00:00.000', 500000.000, 1020000.000, 480000.000, 1000000.000, 480000.000, N'Thối nhầm 20.000đ cho khách lẻ buổi chiều.', N'DA_CHOT'),
(2, 3, '2026-11-01 19:48:15.593', NULL, 1500000.000, 0.000, 0.000, NULL, NULL, NULL, N'DANG_MO');
SET IDENTITY_INSERT [CaThuNgans] OFF;
GO

-- Data for table [CaNhanViens] (2 rows)
SET IDENTITY_INSERT [CaNhanViens] ON;
INSERT INTO [CaNhanViens] ([Id], [NhanVienId], [LoaiCa], [GioMoCa], [GioDongCa], [TrangThai], [GhiChu], [BuoiCa]) VALUES
(1, 4, N'BEP_BANH', '2026-11-01 22:56:28.966', '2026-11-01 22:56:29.033', N'DA_CHOT', N'Ca sang test he thong dinh luong', N''),
(2, 4, N'BEP_BANH', '2026-11-01 22:58:27.088', '2026-11-01 23:04:58.046', N'DA_CHOT', N'ca sáng', N'');
SET IDENTITY_INSERT [CaNhanViens] OFF;
GO

-- Data for table [PhieuBaoLoiMeBanhs] (1 rows)
SET IDENTITY_INSERT [PhieuBaoLoiMeBanhs] ON;
INSERT INTO [PhieuBaoLoiMeBanhs] ([Id], [CaNhanVienId], [NhanVienLapId], [NguoiDuyetId], [SanPhamId], [SoLuongHong], [LyDoLoi], [GhiChuQuanLy], [TrangThai], [NgayLap], [NgayDuyet]) VALUES
(1, 1, 4, 2, 1, 2, N'Chay mat banh do nhiet lo cao', N'Dong y tinh hao phi', N'DA_DUYET', '2026-11-01 22:56:28.983', '2026-11-01 22:56:29.010');
SET IDENTITY_INSERT [PhieuBaoLoiMeBanhs] OFF;
GO

-- Data for table [ChiTietBaoLois] (2 rows)
SET IDENTITY_INSERT [ChiTietBaoLois] ON;
INSERT INTO [ChiTietBaoLois] ([Id], [PhieuBaoLoiId], [NguyenLieuId], [SoLuong]) VALUES
(1, 1, 4, 1.500),
(2, 1, 1, 1.500);
SET IDENTITY_INSERT [ChiTietBaoLois] OFF;
GO

-- Data for table [TieuHaoDinhLuongCas] (2 rows)
SET IDENTITY_INSERT [TieuHaoDinhLuongCas] ON;
INSERT INTO [TieuHaoDinhLuongCas] ([Id], [CaNhanVienId], [NguyenLieuId], [SoLuong], [Nguon], [DonHangId], [SanPhamId], [PhieuBaoLoiId], [ThoiGian], [GhiChu]) VALUES
(1, 1, 4, 1.500, N'ME_LOI_DUYET', NULL, NULL, 1, '2026-11-01 22:56:29.013', N'Me loi duoc duyet'),
(2, 1, 1, 1.500, N'ME_LOI_DUYET', NULL, NULL, 1, '2026-11-01 22:56:29.016', N'Me loi duoc duyet');
SET IDENTITY_INSERT [TieuHaoDinhLuongCas] OFF;
GO

-- Data for table [PhieuDinhLuongCas] (2 rows)
SET IDENTITY_INSERT [PhieuDinhLuongCas] ON;
INSERT INTO [PhieuDinhLuongCas] ([Id], [CaNhanVienId], [NguoiLapId], [ThoiGianXuat], [TrangThai], [GhiChu]) VALUES
(1, 1, 4, '2026-11-01 22:56:29.023', N'DA_DOI_CHIEU', N'Chot ca sang'),
(2, 2, 4, '2026-11-01 23:04:58.046', N'CHO_DOI_CHIEU', NULL);
SET IDENTITY_INSERT [PhieuDinhLuongCas] OFF;
GO

-- Data for table [ChiTietDinhLuongCas] (2 rows)
SET IDENTITY_INSERT [ChiTietDinhLuongCas] ON;
INSERT INTO [ChiTietDinhLuongCas] ([Id], [PhieuDinhLuongCaId], [NguyenLieuId], [SoLuongDinhLuong]) VALUES
(1, 1, 4, 1.500),
(2, 1, 1, 1.500);
SET IDENTITY_INSERT [ChiTietDinhLuongCas] OFF;
GO

-- Data for table [BienBanDoiChieuKhos] (1 rows)
SET IDENTITY_INSERT [BienBanDoiChieuKhos] ON;
INSERT INTO [BienBanDoiChieuKhos] ([Id], [PhieuDinhLuongCaId], [NguoiDoiChieuId], [ThoiGianDoiChieu], [TrangThai], [GhiChuGiaiTrinh]) VALUES
(1, 1, 5, '2026-11-01 22:56:29.043', N'HOP_LE', N'Khop 100%');
SET IDENTITY_INSERT [BienBanDoiChieuKhos] OFF;
GO

-- Data for table [ChiTietDoiChieuKhos] (2 rows)
SET IDENTITY_INSERT [ChiTietDoiChieuKhos] ON;
INSERT INTO [ChiTietDoiChieuKhos] ([Id], [BienBanDoiChieuKhoId], [NguyenLieuId], [SoLuongXuatKho], [SoLuongDinhLuong]) VALUES
(1, 1, 4, 1.500, 1.500),
(2, 1, 1, 1.500, 1.500);
SET IDENTITY_INSERT [ChiTietDoiChieuKhos] OFF;
GO

-- Data for table [PhieuXuatHuys] (6 rows)
SET IDENTITY_INSERT [PhieuXuatHuys] ON;
INSERT INTO [PhieuXuatHuys] ([Id], [NguoiLapId], [NguoiDuyetId], [NgayLap], [TrangThai], [NgayDuyet], [LyDoTuChoi]) VALUES
(1, 3, NULL, '2026-10-30 17:45:00.000', N'NHAP', NULL, NULL),
(2, 4, 2, '2026-10-31 07:30:00.000', N'DA_DUYET', '2026-10-31 08:00:00.000', NULL),
(3, 5, 2, '2026-10-31 14:00:00.000', N'TU_CHOI', '2026-10-31 15:00:00.000', N'Bơ còn hạn, ưu tiên xuất làm croissant trước.'),
(4, 5, 2, '2026-11-01 17:50:15.743', N'DA_DUYET', '2026-11-01 20:12:54.875', NULL),
(5, 3, NULL, '2026-11-01 18:00:15.743', N'NHAP', NULL, NULL),
(6, 5, 2, '2026-11-01 20:12:16.018', N'DA_DUYET', '2026-11-01 20:12:51.916', NULL);
SET IDENTITY_INSERT [PhieuXuatHuys] OFF;
GO

-- Data for table [ChiTietXuatHuys] (8 rows)
SET IDENTITY_INSERT [ChiTietXuatHuys] ON;
INSERT INTO [ChiTietXuatHuys] ([Id], [PhieuXuatHuyId], [LoNguyenLieuId], [LoSanPhamId], [DonHangId], [SoLuongHuy], [DonGiaVon], [LyDo]) VALUES
(1, 1, NULL, NULL, 7, 12.000, 8290.000, N'KHACH_TRA_HANG'),
(2, 2, NULL, 1, NULL, 16.000, 6630.000, N'QUA_HAN'),
(3, 2, NULL, 2, NULL, 9.000, 7440.000, N'QUA_HAN'),
(4, 2, NULL, 3, NULL, 6.000, 8290.000, N'QUA_HAN'),
(5, 3, 7, NULL, NULL, 1.000, 150000.000, N'QUA_HAN'),
(6, 4, 5, NULL, NULL, 6.000, 3000.000, N'LOI_SAN_XUAT'),
(7, 5, NULL, NULL, 3, 1.000, 93100.000, N'KHACH_BUNG_COC'),
(8, 6, 1, NULL, NULL, 2.000, 22000.000, N'QUA_HAN');
SET IDENTITY_INSERT [ChiTietXuatHuys] OFF;
GO

-- Data for table [PhieuKiemKes] (1 rows)
SET IDENTITY_INSERT [PhieuKiemKes] ON;
INSERT INTO [PhieuKiemKes] ([Id], [NguoiThucHienId], [NguoiDuyetId], [NgayKiemKe], [TrangThai], [NgayDuyet], [LyDoTuChoi]) VALUES
(1, 5, 2, '2026-10-31 09:00:00.000', N'DA_DUYET', '2026-10-31 10:00:00.000', NULL);
SET IDENTITY_INSERT [PhieuKiemKes] OFF;
GO

-- Data for table [ChiTietKiemKes] (20 rows)
SET IDENTITY_INSERT [ChiTietKiemKes] ON;
INSERT INTO [ChiTietKiemKes] ([Id], [PhieuKiemKeId], [LoNguyenLieuId], [SoLuongHeThong], [SoLuongThucTe], [LyDo]) VALUES
(1, 1, 1, 2.000, 2.000, NULL),
(2, 1, 2, 5.000, 5.000, NULL),
(3, 1, 3, 25.000, 25.000, NULL),
(4, 1, 4, 20.000, 19.500, N'Hao hụt khi chia túi'),
(5, 1, 5, 100.000, 100.000, NULL),
(6, 1, 6, 200.000, 200.000, NULL),
(7, 1, 7, 2.000, 2.000, NULL),
(8, 1, 8, 6.000, 6.000, NULL),
(9, 1, 9, 6.000, 6.000, NULL),
(10, 1, 10, 2.000, 2.000, NULL),
(11, 1, 11, 6.000, 6.000, NULL),
(12, 1, 12, 5.000, 5.000, NULL),
(13, 1, 13, 500.000, 500.000, NULL),
(14, 1, 14, 1.000, 1.000, NULL),
(15, 1, 15, 3.000, 3.000, NULL),
(16, 1, 16, 40.000, 40.000, NULL),
(17, 1, 17, 1.500, 1.500, NULL),
(18, 1, 18, 200.000, 200.000, NULL),
(19, 1, 19, 0.500, 0.500, NULL),
(20, 1, 20, 0.800, 0.800, NULL);
SET IDENTITY_INSERT [ChiTietKiemKes] OFF;
GO

-- Data for table [GopYs] (1 rows)
SET IDENTITY_INSERT [GopYs] ON;
INSERT INTO [GopYs] ([Id], [KhachHangId], [HoTen], [SoDienThoai], [Email], [DiemDanhGia], [TieuDe], [NoiDung], [NgayGui], [TrangThai]) VALUES
(1, 6, N'Nguyễn Minh Anh', N'0988776655', N'minhanh@gmail.com', 5, N'Bánh rất ngon và giao đúng giờ', N'Bánh kem bắp ăn thanh ngọt, cốt bánh mềm mịn xốp rất thích. Bếp trang trí đẹp hơn mong đợi!', '2026-11-01 18:42:29.967', N'Moi');
SET IDENTITY_INSERT [GopYs] OFF;
GO


