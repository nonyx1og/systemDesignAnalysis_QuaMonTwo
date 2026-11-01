using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DatBanhTuyChinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThietKeBanhId",
                table: "ChiTietDonHangs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThanhPhanTrangTris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nhom = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhMinhHoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: false),
                    SanPhamNenId = table.Column<int>(type: "int", nullable: true),
                    DangKinhDoanh = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhPhanTrangTris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhPhanTrangTris_SanPhams_SanPhamNenId",
                        column: x => x.SanPhamNenId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThietKeBanhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTraCuu = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    KhachHangId = table.Column<int>(type: "int", nullable: false),
                    SanPhamNenId = table.Column<int>(type: "int", nullable: false),
                    DuLieuJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhThietKe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhInAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuTrenBanh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGioLay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GiaTamTinh = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PhuPhi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LyDoPhuPhi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaCuoi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GiaVonDuKien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GhiChuBep = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiDuyetId = table.Column<int>(type: "int", nullable: true),
                    HanBaoGia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonHangId = table.Column<int>(type: "int", nullable: true),
                    AnhThanhPham = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietKeBanhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThietKeBanhs_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThietKeBanhs_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThietKeBanhs_NhanViens_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThietKeBanhs_SanPhams_SanPhamNenId",
                        column: x => x.SanPhamNenId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DinhLuongThanhPhans",
                columns: table => new
                {
                    ThanhPhanId = table.Column<int>(type: "int", nullable: false),
                    NguyenLieuId = table.Column<int>(type: "int", nullable: false),
                    DinhMuc = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DinhLuongThanhPhans", x => new { x.ThanhPhanId, x.NguyenLieuId });
                    table.CheckConstraint("CK_DinhLuongThanhPhan_DinhMuc", "[DinhMuc] > 0");
                    table.ForeignKey(
                        name: "FK_DinhLuongThanhPhans_NguyenLieus_NguyenLieuId",
                        column: x => x.NguyenLieuId,
                        principalTable: "NguyenLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DinhLuongThanhPhans_ThanhPhanTrangTris_ThanhPhanId",
                        column: x => x.ThanhPhanId,
                        principalTable: "ThanhPhanTrangTris",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThietKeThanhPhans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThietKeId = table.Column<int>(type: "int", nullable: false),
                    ThanhPhanId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietKeThanhPhans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThietKeThanhPhans_ThanhPhanTrangTris_ThanhPhanId",
                        column: x => x.ThanhPhanId,
                        principalTable: "ThanhPhanTrangTris",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThietKeThanhPhans_ThietKeBanhs_ThietKeId",
                        column: x => x.ThietKeId,
                        principalTable: "ThietKeBanhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_ThietKeBanhId",
                table: "ChiTietDonHangs",
                column: "ThietKeBanhId");

            migrationBuilder.CreateIndex(
                name: "IX_DinhLuongThanhPhans_NguyenLieuId",
                table: "DinhLuongThanhPhans",
                column: "NguyenLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhPhanTrangTris_SanPhamNenId",
                table: "ThanhPhanTrangTris",
                column: "SanPhamNenId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeBanhs_DonHangId",
                table: "ThietKeBanhs",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeBanhs_KhachHangId",
                table: "ThietKeBanhs",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeBanhs_MaTraCuu",
                table: "ThietKeBanhs",
                column: "MaTraCuu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeBanhs_NguoiDuyetId",
                table: "ThietKeBanhs",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeBanhs_SanPhamNenId",
                table: "ThietKeBanhs",
                column: "SanPhamNenId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeThanhPhans_ThanhPhanId",
                table: "ThietKeThanhPhans",
                column: "ThanhPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietKeThanhPhans_ThietKeId",
                table: "ThietKeThanhPhans",
                column: "ThietKeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonHangs_ThietKeBanhs_ThietKeBanhId",
                table: "ChiTietDonHangs",
                column: "ThietKeBanhId",
                principalTable: "ThietKeBanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonHangs_ThietKeBanhs_ThietKeBanhId",
                table: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "DinhLuongThanhPhans");

            migrationBuilder.DropTable(
                name: "ThietKeThanhPhans");

            migrationBuilder.DropTable(
                name: "ThanhPhanTrangTris");

            migrationBuilder.DropTable(
                name: "ThietKeBanhs");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietDonHangs_ThietKeBanhId",
                table: "ChiTietDonHangs");

            migrationBuilder.DropColumn(
                name: "ThietKeBanhId",
                table: "ChiTietDonHangs");
        }
    }
}
