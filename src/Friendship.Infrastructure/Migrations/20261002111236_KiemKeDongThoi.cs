using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KiemKeDongThoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PhieuKiemKes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKiemKes_TrangThai",
                table: "PhieuKiemKes",
                column: "TrangThai",
                unique: true,
                filter: "[TrangThai] IN ('NHAP','CHO_DUYET')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhieuKiemKes_TrangThai",
                table: "PhieuKiemKes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PhieuKiemKes");
        }
    }
}
