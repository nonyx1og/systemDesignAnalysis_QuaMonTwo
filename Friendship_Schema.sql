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

