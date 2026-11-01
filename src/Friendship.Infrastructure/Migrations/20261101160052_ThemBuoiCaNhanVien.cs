using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ThemBuoiCaNhanVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuoiCa",
                table: "CaNhanViens",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuoiCa",
                table: "CaNhanViens");
        }
    }
}
