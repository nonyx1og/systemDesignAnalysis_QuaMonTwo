using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuanLyDinhLuongCa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaNhanViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhanVienId = table.Column<int>(type: "int", nullable: false),
                    LoaiCa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GioMoCa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioDongCa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaNhanViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaNhanViens_NhanViens_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuXuatKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhanVienXuatId = table.Column<int>(type: "int", nullable: false),
                    NgayXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuXuatKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKhos_NhanViens_NhanVienXuatId",
                        column: x => x.NhanVienXuatId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuBaoLoiMeBanhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaNhanVienId = table.Column<int>(type: "int", nullable: false),
                    NhanVienLapId = table.Column<int>(type: "int", nullable: false),
                    NguoiDuyetId = table.Column<int>(type: "int", nullable: true),
                    SanPhamId = table.Column<int>(type: "int", nullable: true),
                    SoLuongHong = table.Column<int>(type: "int", nullable: false),
                    LyDoLoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChuQuanLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuBaoLoiMeBanhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuBaoLoiMeBanhs_CaNhanViens_CaNhanVienId",
                        column: x => x.CaNhanVienId,
                        principalTable: "CaNhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuBaoLoiMeBanhs_NhanViens_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuBaoLoiMeBanhs_NhanViens_NhanVienLapId",
                        column: x => x.NhanVienLapId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuBaoLoiMeBanhs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuDinhLuongCas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaNhanVienId = table.Column<int>(type: "int", nullable: false),
                    NguoiLapId = table.Column<int>(type: "int", nullable: false),
                    ThoiGianXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuDinhLuongCas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuDinhLuongCas_CaNhanViens_CaNhanVienId",
                        column: x => x.CaNhanVienId,
                        principalTable: "CaNhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuDinhLuongCas_NhanViens_NguoiLapId",
                        column: x => x.NguoiLapId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietXuatKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuXuatKhoId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietXuatKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatKhos_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatKhos_PhieuXuatKhos_PhieuXuatKhoId",
                        column: x => x.PhieuXuatKhoId,
                        principalTable: "PhieuXuatKhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietBaoLois",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuBaoLoiId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietBaoLois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietBaoLois_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietBaoLois_PhieuBaoLoiMeBanhs_PhieuBaoLoiId",
                        column: x => x.PhieuBaoLoiId,
                        principalTable: "PhieuBaoLoiMeBanhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TieuHaoDinhLuongCas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaNhanVienId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Nguon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DonHangId = table.Column<int>(type: "int", nullable: true),
                    SanPhamId = table.Column<int>(type: "int", nullable: true),
                    PhieuBaoLoiId = table.Column<int>(type: "int", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TieuHaoDinhLuongCas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TieuHaoDinhLuongCas_CaNhanViens_CaNhanVienId",
                        column: x => x.CaNhanVienId,
                        principalTable: "CaNhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TieuHaoDinhLuongCas_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TieuHaoDinhLuongCas_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TieuHaoDinhLuongCas_PhieuBaoLoiMeBanhs_PhieuBaoLoiId",
                        column: x => x.PhieuBaoLoiId,
                        principalTable: "PhieuBaoLoiMeBanhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TieuHaoDinhLuongCas_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BienBanDoiChieuKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuDinhLuongCaId = table.Column<int>(type: "int", nullable: false),
                    NguoiDoiChieuId = table.Column<int>(type: "int", nullable: false),
                    ThoiGianDoiChieu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChuGiaiTrinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienBanDoiChieuKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BienBanDoiChieuKhos_NhanViens_NguoiDoiChieuId",
                        column: x => x.NguoiDoiChieuId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BienBanDoiChieuKhos_PhieuDinhLuongCas_PhieuDinhLuongCaId",
                        column: x => x.PhieuDinhLuongCaId,
                        principalTable: "PhieuDinhLuongCas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDinhLuongCas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuDinhLuongCaId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuongDinhLuong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDinhLuongCas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDinhLuongCas_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDinhLuongCas_PhieuDinhLuongCas_PhieuDinhLuongCaId",
                        column: x => x.PhieuDinhLuongCaId,
                        principalTable: "PhieuDinhLuongCas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDoiChieuKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BienBanDoiChieuKhoId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuongXuatKho = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SoLuongDinhLuong = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDoiChieuKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDoiChieuKhos_BienBanDoiChieuKhos_BienBanDoiChieuKhoId",
                        column: x => x.BienBanDoiChieuKhoId,
                        principalTable: "BienBanDoiChieuKhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDoiChieuKhos_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BienBanDoiChieuKhos_NguoiDoiChieuId",
                table: "BienBanDoiChieuKhos",
                column: "NguoiDoiChieuId");

            migrationBuilder.CreateIndex(
                name: "IX_BienBanDoiChieuKhos_PhieuDinhLuongCaId",
                table: "BienBanDoiChieuKhos",
                column: "PhieuDinhLuongCaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaNhanViens_NhanVienId_LoaiCa",
                table: "CaNhanViens",
                columns: new[] { "NhanVienId", "LoaiCa" },
                unique: true,
                filter: "[TrangThai] = 'DANG_MO'");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietBaoLois_NguyenLieuId",
                table: "ChiTietBaoLois",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietBaoLois_PhieuBaoLoiId",
                table: "ChiTietBaoLois",
                column: "PhieuBaoLoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDinhLuongCas_NguyenLieuId",
                table: "ChiTietDinhLuongCas",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDinhLuongCas_PhieuDinhLuongCaId",
                table: "ChiTietDinhLuongCas",
                column: "PhieuDinhLuongCaId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDoiChieuKhos_BienBanDoiChieuKhoId",
                table: "ChiTietDoiChieuKhos",
                column: "BienBanDoiChieuKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDoiChieuKhos_NguyenLieuId",
                table: "ChiTietDoiChieuKhos",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatKhos_NguyenLieuId",
                table: "ChiTietXuatKhos",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatKhos_PhieuXuatKhoId",
                table: "ChiTietXuatKhos",
                column: "PhieuXuatKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuBaoLoiMeBanhs_CaNhanVienId",
                table: "PhieuBaoLoiMeBanhs",
                column: "CaNhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuBaoLoiMeBanhs_NguoiDuyetId",
                table: "PhieuBaoLoiMeBanhs",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuBaoLoiMeBanhs_NhanVienLapId",
                table: "PhieuBaoLoiMeBanhs",
                column: "NhanVienLapId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuBaoLoiMeBanhs_SanPhamId",
                table: "PhieuBaoLoiMeBanhs",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuDinhLuongCas_CaNhanVienId",
                table: "PhieuDinhLuongCas",
                column: "CaNhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuDinhLuongCas_NguoiLapId",
                table: "PhieuDinhLuongCas",
                column: "NguoiLapId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKhos_NhanVienXuatId",
                table: "PhieuXuatKhos",
                column: "NhanVienXuatId");

            migrationBuilder.CreateIndex(
                name: "IX_TieuHaoDinhLuongCas_CaNhanVienId",
                table: "TieuHaoDinhLuongCas",
                column: "CaNhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_TieuHaoDinhLuongCas_DonHangId",
                table: "TieuHaoDinhLuongCas",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_TieuHaoDinhLuongCas_NguyenLieuId",
                table: "TieuHaoDinhLuongCas",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_TieuHaoDinhLuongCas_PhieuBaoLoiId",
                table: "TieuHaoDinhLuongCas",
                column: "PhieuBaoLoiId");

            migrationBuilder.CreateIndex(
                name: "IX_TieuHaoDinhLuongCas_SanPhamId",
                table: "TieuHaoDinhLuongCas",
                column: "SanPhamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietBaoLois");

            migrationBuilder.DropTable(
                name: "ChiTietDinhLuongCas");

            migrationBuilder.DropTable(
                name: "ChiTietDoiChieuKhos");

            migrationBuilder.DropTable(
                name: "ChiTietXuatKhos");

            migrationBuilder.DropTable(
                name: "TieuHaoDinhLuongCas");

            migrationBuilder.DropTable(
                name: "BienBanDoiChieuKhos");

            migrationBuilder.DropTable(
                name: "PhieuXuatKhos");

            migrationBuilder.DropTable(
                name: "PhieuBaoLoiMeBanhs");

            migrationBuilder.DropTable(
                name: "PhieuDinhLuongCas");

            migrationBuilder.DropTable(
                name: "CaNhanViens");
        }
    }
}
