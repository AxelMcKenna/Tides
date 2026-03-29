using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tides.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EntryEventAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_entries_heats_HeatId",
                table: "entries");

            migrationBuilder.AlterColumn<Guid>(
                name: "HeatId",
                table: "entries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "EventDefinitionId",
                table: "entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_entries_EventDefinitionId",
                table: "entries",
                column: "EventDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_entries_heats_HeatId",
                table: "entries",
                column: "HeatId",
                principalTable: "heats",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_entries_heats_HeatId",
                table: "entries");

            migrationBuilder.DropIndex(
                name: "IX_entries_EventDefinitionId",
                table: "entries");

            migrationBuilder.DropColumn(
                name: "EventDefinitionId",
                table: "entries");

            migrationBuilder.AlterColumn<Guid>(
                name: "HeatId",
                table: "entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_entries_heats_HeatId",
                table: "entries",
                column: "HeatId",
                principalTable: "heats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
