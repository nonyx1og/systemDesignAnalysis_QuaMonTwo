using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DanhMucKho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenGoi",
                table: "NguyenLieus",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "MaKhuyenMai",
                table: "KhuyenMais",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NguyenLieus_TenGoi",
                table: "NguyenLieus",
                column: "TenGoi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_MaKhuyenMai",
                table: "KhuyenMais",
                column: "MaKhuyenMai",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NguyenLieus_TenGoi",
                table: "NguyenLieus");

            migrationBuilder.DropIndex(
                name: "IX_KhuyenMais_MaKhuyenMai",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "MaKhuyenMai",
                table: "KhuyenMais");

            migrationBuilder.AlterColumn<string>(
                name: "TenGoi",
                table: "NguyenLieus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
