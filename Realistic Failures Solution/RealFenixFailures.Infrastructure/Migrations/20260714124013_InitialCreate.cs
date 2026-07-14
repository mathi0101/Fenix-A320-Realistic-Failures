using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RealFenixFailures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AircraftWearableSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftWearableSystems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailurePresetTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailurePresetTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FenixFailureSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenixFailureSystems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAircrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Registration = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IcaoTypeCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TotalFlightHours = table.Column<double>(type: "REAL", nullable: false),
                    TotalFlights = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAircrafts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailurePresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    TriggerDescription = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    PresetType = table.Column<int>(type: "INTEGER", nullable: false),
                    PresetType1 = table.Column<int>(type: "INTEGER", nullable: false),
                    FlightPhase = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailurePresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailurePresets_FailurePresetTypes_PresetType",
                        column: x => x.PresetType,
                        principalTable: "FailurePresetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FenixFailureGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SystemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenixFailureGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FenixFailureGroups_FenixFailureSystems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "FenixFailureSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AircraftSystemWears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAircraftId = table.Column<int>(type: "INTEGER", nullable: false),
                    WearableSystemId = table.Column<int>(type: "INTEGER", nullable: false),
                    WearPercentage = table.Column<double>(type: "REAL", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftSystemWears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AircraftSystemWears_AircraftWearableSystems_WearableSystemId",
                        column: x => x.WearableSystemId,
                        principalTable: "AircraftWearableSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftSystemWears_UserAircrafts_UserAircraftId",
                        column: x => x.UserAircraftId,
                        principalTable: "UserAircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlightSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RiskLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    UserAircraftId = table.Column<int>(type: "INTEGER", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSessions_UserAircrafts_UserAircraftId",
                        column: x => x.UserAircraftId,
                        principalTable: "UserAircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FenixFailureDefinitions",
                columns: table => new
                {
                    FenixFailureId = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenixFailureDefinitions", x => x.FenixFailureId);
                    table.ForeignKey(
                        name: "FK_FenixFailureDefinitions_FenixFailureGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "FenixFailureGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresetFailureDefinitions",
                columns: table => new
                {
                    PresetId = table.Column<int>(type: "INTEGER", nullable: false),
                    FenixFailureId = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    ProbabilityGroup = table.Column<int>(type: "INTEGER", nullable: true),
                    Probability = table.Column<double>(type: "REAL", nullable: false),
                    Ias = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Above_Altitude = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Below_Altitude = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Time = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AfterEvent = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AfterEventSeconds = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetFailureDefinitions", x => new { x.PresetId, x.FenixFailureId });
                    table.ForeignKey(
                        name: "FK_PresetFailureDefinitions_FailurePresets_PresetId",
                        column: x => x.PresetId,
                        principalTable: "FailurePresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresetFailureDefinitions_FenixFailureDefinitions_FenixFailureId",
                        column: x => x.FenixFailureId,
                        principalTable: "FenixFailureDefinitions",
                        principalColumn: "FenixFailureId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TriggeredFailures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FenixFailureId = table.Column<string>(type: "TEXT", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FlightPhase = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggeredFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FenixFailureDefinitions_FenixFailureId",
                        column: x => x.FenixFailureId,
                        principalTable: "FenixFailureDefinitions",
                        principalColumn: "FenixFailureId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FlightSessions_FlightSessionId",
                        column: x => x.FlightSessionId,
                        principalTable: "FlightSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AircraftWearableSystems",
                columns: new[] { "Id", "DisplayOrder", "Name", "ShortName" },
                values: new object[,]
                {
                    { 1, 1, "Engine 1", "ENG1" },
                    { 2, 2, "Engine 2", "ENG2" },
                    { 3, 3, "Hydraulic System", "HYD" },
                    { 4, 4, "Landing Gear", "GEAR" },
                    { 5, 5, "Navigation Systems", "NAV" },
                    { 6, 6, "Pneumatic System", "PNEU" },
                    { 7, 7, "APU", "APU" },
                    { 8, 8, "Doors", "DOOR" }
                });

            migrationBuilder.InsertData(
                table: "FailurePresetTypes",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 1, "RealisticMode" },
                    { 2, "TrainingMode" },
                    { 3, "Custom" },
                    { 4, "UserPreset" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftSystemWears_UserAircraftId_WearableSystemId",
                table: "AircraftSystemWears",
                columns: new[] { "UserAircraftId", "WearableSystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AircraftSystemWears_WearableSystemId",
                table: "AircraftSystemWears",
                column: "WearableSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_FailurePresets_PresetType",
                table: "FailurePresets",
                column: "PresetType");

            migrationBuilder.CreateIndex(
                name: "IX_FenixFailureDefinitions_GroupId",
                table: "FenixFailureDefinitions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FenixFailureGroups_SystemId",
                table: "FenixFailureGroups",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_UserAircraftId",
                table: "FlightSessions",
                column: "UserAircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_PresetFailureDefinitions_FenixFailureId",
                table: "PresetFailureDefinitions",
                column: "FenixFailureId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredFailures_FenixFailureId",
                table: "TriggeredFailures",
                column: "FenixFailureId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredFailures_FlightSessionId",
                table: "TriggeredFailures",
                column: "FlightSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAircrafts_Registration",
                table: "UserAircrafts",
                column: "Registration",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftSystemWears");

            migrationBuilder.DropTable(
                name: "PresetFailureDefinitions");

            migrationBuilder.DropTable(
                name: "TriggeredFailures");

            migrationBuilder.DropTable(
                name: "AircraftWearableSystems");

            migrationBuilder.DropTable(
                name: "FailurePresets");

            migrationBuilder.DropTable(
                name: "FenixFailureDefinitions");

            migrationBuilder.DropTable(
                name: "FlightSessions");

            migrationBuilder.DropTable(
                name: "FailurePresetTypes");

            migrationBuilder.DropTable(
                name: "FenixFailureGroups");

            migrationBuilder.DropTable(
                name: "UserAircrafts");

            migrationBuilder.DropTable(
                name: "FenixFailureSystems");
        }
    }
}
