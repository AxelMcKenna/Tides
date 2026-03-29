using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tides.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCarnivalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "carnivals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "carnivals");
        }
    }
}
