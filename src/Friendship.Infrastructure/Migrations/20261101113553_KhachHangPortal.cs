using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KhachHangPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatKhauHash",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "KhachHangs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GopYs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhachHangId = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiemDanhGia = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GopYs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GopYs_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GopYs_KhachHangId",
                table: "GopYs",
                column: "KhachHangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GopYs");

            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "MatKhauHash",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "KhachHangs");
        }
    }
}
