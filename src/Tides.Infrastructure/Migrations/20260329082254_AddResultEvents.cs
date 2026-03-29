using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tides.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResultEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "result_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarnivalId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Placing = table.Column<int>(type: "integer", nullable: true),
                    Time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    JudgeScore = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecorderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedHeatVersion = table.Column<int>(type: "integer", nullable: false),
                    SupersededBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_result_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_result_events_CarnivalId",
                table: "result_events",
                column: "CarnivalId");

            migrationBuilder.CreateIndex(
                name: "IX_result_events_EventId",
                table: "result_events",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_result_events_HeatId",
                table: "result_events",
                column: "HeatId");

            migrationBuilder.CreateIndex(
                name: "IX_result_events_HeatId_EntryId",
                table: "result_events",
                columns: new[] { "HeatId", "EntryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "result_events");
        }
    }
}
