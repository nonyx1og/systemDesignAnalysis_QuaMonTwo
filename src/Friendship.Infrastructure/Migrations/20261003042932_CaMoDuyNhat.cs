using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaMoDuyNhat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CaThuNgans_NhanVienId",
                table: "CaThuNgans");

            migrationBuilder.CreateIndex(
                name: "IX_CaThuNgans_NhanVienId",
                table: "CaThuNgans",
                column: "NhanVienId",
                unique: true,
                filter: "[TrangThai] = 'DANG_MO'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CaThuNgans_NhanVienId",
                table: "CaThuNgans");

            migrationBuilder.CreateIndex(
                name: "IX_CaThuNgans_NhanVienId",
                table: "CaThuNgans",
                column: "NhanVienId");
        }
    }
}
