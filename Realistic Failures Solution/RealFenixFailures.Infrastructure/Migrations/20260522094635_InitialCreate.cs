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
                name: "FailurePresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    PresetType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailurePresets", x => x.Id);
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
                name: "FlightSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PresetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSessions_FailurePresets_PresetId",
                        column: x => x.PresetId,
                        principalTable: "FailurePresets",
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
                name: "FenixFailureDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FenixFailureId = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenixFailureDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FenixFailureDefinitions_FenixFailureGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "FenixFailureGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresetFailureDefinition",
                columns: table => new
                {
                    FailureDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FailurePresetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetFailureDefinition", x => new { x.FailureDefinitionId, x.FailurePresetId });
                    table.ForeignKey(
                        name: "FK_PresetFailureDefinition_FailurePresets_FailurePresetId",
                        column: x => x.FailurePresetId,
                        principalTable: "FailurePresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresetFailureDefinition_FenixFailureDefinitions_FailureDefinitionId",
                        column: x => x.FailureDefinitionId,
                        principalTable: "FenixFailureDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TriggeredFailures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FailureDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PresetId = table.Column<int>(type: "INTEGER", nullable: true),
                    TriggeredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FlightPhase = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggeredFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FailurePresets_PresetId",
                        column: x => x.PresetId,
                        principalTable: "FailurePresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FenixFailureDefinitions_FailureDefinitionId",
                        column: x => x.FailureDefinitionId,
                        principalTable: "FenixFailureDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FlightSessions_FlightSessionId",
                        column: x => x.FlightSessionId,
                        principalTable: "FlightSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FailurePresets",
                columns: new[] { "Id", "Description", "Name", "PresetType" },
                values: new object[,]
                {
                    { 1, "Fallas menores iniciales para forzar checklist real.", "Cold & Dark Immersion", 1 },
                    { 2, "Fallas aleatorias menores durante el vuelo.", "Random Non-Critical", 2 },
                    { 3, "Fallas aleatorias con posibilidad de fallas críticas.", "Random with Critical", 3 },
                    { 4, "Fallas críticas por fase para entrenamiento.", "Training Mode", 4 },
                    { 5, "Distribución cercana a operación real A320.", "Realistic Mode", 5 },
                    { 6, "Preset vacío para reglas del usuario.", "Custom", 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FenixFailureDefinitions_FenixFailureId",
                table: "FenixFailureDefinitions",
                column: "FenixFailureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FenixFailureDefinitions_GroupId",
                table: "FenixFailureDefinitions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FenixFailureGroups_SystemId",
                table: "FenixFailureGroups",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_PresetId",
                table: "FlightSessions",
                column: "PresetId");

            migrationBuilder.CreateIndex(
                name: "IX_PresetFailureDefinition_FailurePresetId",
                table: "PresetFailureDefinition",
                column: "FailurePresetId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredFailures_FailureDefinitionId",
                table: "TriggeredFailures",
                column: "FailureDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredFailures_FlightSessionId",
                table: "TriggeredFailures",
                column: "FlightSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredFailures_PresetId",
                table: "TriggeredFailures",
                column: "PresetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresetFailureDefinition");

            migrationBuilder.DropTable(
                name: "TriggeredFailures");

            migrationBuilder.DropTable(
                name: "FenixFailureDefinitions");

            migrationBuilder.DropTable(
                name: "FlightSessions");

            migrationBuilder.DropTable(
                name: "FenixFailureGroups");

            migrationBuilder.DropTable(
                name: "FailurePresets");

            migrationBuilder.DropTable(
                name: "FenixFailureSystems");
        }
    }
}
