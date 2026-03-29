using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tides.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeatVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "heats",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "heats");
        }
    }
}
