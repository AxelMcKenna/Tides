using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Tides.Core.Domain.ValueObjects;

#nullable disable

namespace Tides.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "carnivals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HostingClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sanction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carnivals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SurfguardId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organisations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarnivalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AgeGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxLanes = table.Column<int>(type: "integer", nullable: false),
                    AdvancementRule = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdvanceTopN = table.Column<int>(type: "integer", nullable: false),
                    AdvanceFastestN = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_definitions_carnivals_CarnivalId",
                        column: x => x.CarnivalId,
                        principalTable: "carnivals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "points_tables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entries = table.Column<IReadOnlyList<PointsTableEntry>>(type: "jsonb", nullable: false),
                    FractionalTiesEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CarnivalId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_points_tables_carnivals_CarnivalId",
                        column: x => x.CarnivalId,
                        principalTable: "carnivals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "protests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarnivalId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeatId = table.Column<Guid>(type: "uuid", nullable: true),
                    LodgedByClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdjudicationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LodgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdjudicatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_protests_carnivals_CarnivalId",
                        column: x => x.CarnivalId,
                        principalTable: "carnivals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rounds_event_definitions_EventDefinitionId",
                        column: x => x.EventDefinitionId,
                        principalTable: "event_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "heats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeatNumber = table.Column<int>(type: "integer", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_heats_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberIds = table.Column<string>(type: "jsonb", nullable: false),
                    Lane = table.Column<int>(type: "integer", nullable: true),
                    IsWithdrawn = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entries_heats_HeatId",
                        column: x => x.HeatId,
                        principalTable: "heats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    JudgeScore = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    audit_trail = table.Column<IReadOnlyList<AuditEntry>>(type: "jsonb", nullable: false),
                    placing = table.Column<int>(type: "integer", nullable: true),
                    time = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_results_heats_HeatId",
                        column: x => x.HeatId,
                        principalTable: "heats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_branches_RegionId",
                table: "branches",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_carnivals_HostingClubId",
                table: "carnivals",
                column: "HostingClubId");

            migrationBuilder.CreateIndex(
                name: "IX_clubs_BranchId",
                table: "clubs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_entries_ClubId",
                table: "entries",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_entries_HeatId",
                table: "entries",
                column: "HeatId");

            migrationBuilder.CreateIndex(
                name: "IX_event_definitions_CarnivalId",
                table: "event_definitions",
                column: "CarnivalId");

            migrationBuilder.CreateIndex(
                name: "IX_event_definitions_CarnivalId_AgeGroup_Category_Gender",
                table: "event_definitions",
                columns: new[] { "CarnivalId", "AgeGroup", "Category", "Gender" });

            migrationBuilder.CreateIndex(
                name: "IX_heats_RoundId",
                table: "heats",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_members_ClubId",
                table: "members",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_members_SurfguardId",
                table: "members",
                column: "SurfguardId",
                unique: true,
                filter: "\"SurfguardId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_points_tables_CarnivalId",
                table: "points_tables",
                column: "CarnivalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_protests_CarnivalId",
                table: "protests",
                column: "CarnivalId");

            migrationBuilder.CreateIndex(
                name: "IX_protests_EventId",
                table: "protests",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_regions_OrganisationId",
                table: "regions",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_results_EntryId",
                table: "results",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_results_HeatId",
                table: "results",
                column: "HeatId");

            migrationBuilder.CreateIndex(
                name: "IX_rounds_EventDefinitionId",
                table: "rounds",
                column: "EventDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "clubs");

            migrationBuilder.DropTable(
                name: "entries");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "organisations");

            migrationBuilder.DropTable(
                name: "points_tables");

            migrationBuilder.DropTable(
                name: "protests");

            migrationBuilder.DropTable(
                name: "regions");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "heats");

            migrationBuilder.DropTable(
                name: "rounds");

            migrationBuilder.DropTable(
                name: "event_definitions");

            migrationBuilder.DropTable(
                name: "carnivals");
        }
    }
}
