using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TraHangBanLe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LyDoTra",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LyDoTra",
                table: "DonHangs");
        }
    }
}
