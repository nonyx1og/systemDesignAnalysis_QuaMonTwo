using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.Id);
                    table.CheckConstraint("CK_KhachHang_Diem", "[DiemTichLuy] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuongTrinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiGiam = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    GiaTriDonToiThieu = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    DangApDung = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.Id);
                    table.CheckConstraint("CK_KhuyenMai_Ngay", "[NgayKetThuc] >= [NgayBatDau]");
                });

            migrationBuilder.CreateTable(
                name: "NguyenLieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenGoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MucCanhBaoTon = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguyenLieus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenDangNhap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBanh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KichThuoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    HanSuDungGio = table.Column<int>(type: "int", nullable: false),
                    DangKinhDoanh = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoNguyenLieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    NgayNhap = table.Column<DateOnly>(type: "date", nullable: false),
                    HanSuDung = table.Column<DateOnly>(type: "date", nullable: false),
                    SoLuongNhap = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SoLuongCon = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DonGiaNhap = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoNguyenLieus", x => x.Id);
                    table.CheckConstraint("CK_LoNguyenLieu_Ton", "[SoLuongCon] >= 0");
                    table.ForeignKey(
                        name: "FK_LoNguyenLieus_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaThuNgans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhanVienId = table.Column<int>(type: "int", nullable: false),
                    GioMoCa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioDongCa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TienDauCa = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TienMatHeThong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ChuyenKhoanHeThong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TienMatThucTe = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ChuyenKhoanThucTe = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    GhiChuChenhLech = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaThuNgans", x => x.Id);
                    table.CheckConstraint("CK_CaThuNgan_TienDauCa", "[TienDauCa] >= 0");
                    table.ForeignKey(
                        name: "FK_CaThuNgans_NhanViens_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiDon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayGioLay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TongTienHang = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TienGiamGia = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DiemSuDung = table.Column<int>(type: "int", nullable: false),
                    TongThanhToan = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NhanVienId = table.Column<int>(type: "int", nullable: false),
                    KhachHangId = table.Column<int>(type: "int", nullable: true),
                    KhuyenMaiId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonHangs_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHangs_KhuyenMais_KhuyenMaiId",
                        column: x => x.KhuyenMaiId,
                        principalTable: "KhuyenMais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHangs_NhanViens_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuKiemKes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiThucHienId = table.Column<int>(type: "int", nullable: false),
                    NguoiDuyetId = table.Column<int>(type: "int", nullable: true),
                    NgayKiemKe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoTuChoi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuKiemKes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuKiemKes_NhanViens_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuKiemKes_NhanViens_NguoiThucHienId",
                        column: x => x.NguoiThucHienId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuXuatHuys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiLapId = table.Column<int>(type: "int", nullable: false),
                    NguoiDuyetId = table.Column<int>(type: "int", nullable: true),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoTuChoi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuXuatHuys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuXuatHuys_NhanViens_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuXuatHuys_NhanViens_NguoiLapId",
                        column: x => x.NguoiLapId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DinhLuongs",
                columns: table => new
                {
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    DinhMuc = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DinhLuongs", x => new { x.SanPhamId, x.NguyenLieuId });
                    table.CheckConstraint("CK_DinhLuong_DinhMuc", "[DinhMuc] > 0");
                    table.ForeignKey(
                        name: "FK_DinhLuongs_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DinhLuongs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    NgaySanXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanSuDung = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongSanXuat = table.Column<int>(type: "int", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    TyLeGiamXaHang = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoSanPhams", x => x.Id);
                    table.CheckConstraint("CK_LoSanPham_Ton", "[SoLuongTon] >= 0 AND [TyLeGiamXaHang] >= 0 AND [TyLeGiamXaHang] <= 1");
                    table.ForeignKey(
                        name: "FK_LoSanPhams_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonHangId = table.Column<int>(type: "int", nullable: false),
                    CaThuNganId = table.Column<int>(type: "int", nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhToans_CaThuNgans_CaThuNganId",
                        column: x => x.CaThuNganId,
                        principalTable: "CaThuNgans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToans_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietKiemKes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuKiemKeId = table.Column<int>(type: "int", nullable: false),
                    LoNguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuongHeThong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SoLuongThucTe = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKiemKes", x => x.Id);
                    table.CheckConstraint("CK_ChiTietKiemKe_LyDo", "[SoLuongThucTe] = [SoLuongHeThong] OR [LyDo] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_ChiTietKiemKes_LoNguyenLieus_LoNguyenLieuId",
                        column: x => x.LoNguyenLieuId,
                        principalTable: "LoNguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietKiemKes_PhieuKiemKes_PhieuKiemKeId",
                        column: x => x.PhieuKiemKeId,
                        principalTable: "PhieuKiemKes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonHangId = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    LoSanPhamId = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TienGiam = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LoiChuc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.Id);
                    table.CheckConstraint("CK_ChiTietDonHang_SoLuong", "[SoLuong] > 0");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_LoSanPhams_LoSanPhamId",
                        column: x => x.LoSanPhamId,
                        principalTable: "LoSanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietXuatHuys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuXuatHuyId = table.Column<int>(type: "int", nullable: false),
                    LoNguyenLieuId = table.Column<int>(type: "int", nullable: true),
                    LoSanPhamId = table.Column<int>(type: "int", nullable: true),
                    DonHangId = table.Column<int>(type: "int", nullable: true),
                    SoLuongHuy = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DonGiaVon = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietXuatHuys", x => x.Id);
                    table.CheckConstraint("CK_ChiTietXuatHuy_SoLuong", "[SoLuongHuy] > 0");
                    table.CheckConstraint("CK_ChiTietXuatHuy_Xor", "(CASE WHEN [LoNguyenLieuId] IS NULL THEN 0 ELSE 1 END + CASE WHEN [LoSanPhamId] IS NULL THEN 0 ELSE 1 END + CASE WHEN [DonHangId] IS NULL THEN 0 ELSE 1 END) = 1");
                    table.ForeignKey(
                        name: "FK_ChiTietXuatHuys_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatHuys_LoNguyenLieus_LoNguyenLieuId",
                        column: x => x.LoNguyenLieuId,
                        principalTable: "LoNguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatHuys_LoSanPhams_LoSanPhamId",
                        column: x => x.LoSanPhamId,
                        principalTable: "LoSanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatHuys_PhieuXuatHuys_PhieuXuatHuyId",
                        column: x => x.PhieuXuatHuyId,
                        principalTable: "PhieuXuatHuys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaThuNgans_NhanVienId",
                table: "CaThuNgans",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_DonHangId",
                table: "ChiTietDonHangs",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_LoSanPhamId",
                table: "ChiTietDonHangs",
                column: "LoSanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_SanPhamId",
                table: "ChiTietDonHangs",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKiemKes_LoNguyenLieuId",
                table: "ChiTietKiemKes",
                column: "LoNguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKiemKes_PhieuKiemKeId",
                table: "ChiTietKiemKes",
                column: "PhieuKiemKeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatHuys_DonHangId",
                table: "ChiTietXuatHuys",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatHuys_LoNguyenLieuId",
                table: "ChiTietXuatHuys",
                column: "LoNguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatHuys_LoSanPhamId",
                table: "ChiTietXuatHuys",
                column: "LoSanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatHuys_PhieuXuatHuyId",
                table: "ChiTietXuatHuys",
                column: "PhieuXuatHuyId");

            migrationBuilder.CreateIndex(
                name: "IX_DinhLuongs_NguyenLieuId",
                table: "DinhLuongs",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_KhachHangId",
                table: "DonHangs",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_KhuyenMaiId",
                table: "DonHangs",
                column: "KhuyenMaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_NhanVienId",
                table: "DonHangs",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_SoDienThoai",
                table: "KhachHangs",
                column: "SoDienThoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoNguyenLieus_NguyenLieuId",
                table: "LoNguyenLieus",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_LoSanPhams_SanPhamId",
                table: "LoSanPhams",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_TenDangNhap",
                table: "NhanViens",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKiemKes_NguoiDuyetId",
                table: "PhieuKiemKes",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKiemKes_NguoiThucHienId",
                table: "PhieuKiemKes",
                column: "NguoiThucHienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatHuys_NguoiDuyetId",
                table: "PhieuXuatHuys",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatHuys_NguoiLapId",
                table: "PhieuXuatHuys",
                column: "NguoiLapId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_CaThuNganId",
                table: "ThanhToans",
                column: "CaThuNganId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_DonHangId",
                table: "ThanhToans",
                column: "DonHangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "ChiTietKiemKes");

            migrationBuilder.DropTable(
                name: "ChiTietXuatHuys");

            migrationBuilder.DropTable(
                name: "DinhLuongs");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropTable(
                name: "PhieuKiemKes");

            migrationBuilder.DropTable(
                name: "LoNguyenLieus");

            migrationBuilder.DropTable(
                name: "LoSanPhams");

            migrationBuilder.DropTable(
                name: "PhieuXuatHuys");

            migrationBuilder.DropTable(
                name: "CaThuNgans");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "NguyenLieus");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "NhanViens");
        }
    }
}
