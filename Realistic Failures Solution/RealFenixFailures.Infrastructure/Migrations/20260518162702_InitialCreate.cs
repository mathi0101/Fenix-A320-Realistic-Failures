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
                name: "FailureDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    AffectedSystem = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Probability = table.Column<double>(type: "REAL", nullable: false),
                    ApplicableFlightPhase = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailurePresets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    PresetType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailurePresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PresetId = table.Column<Guid>(type: "TEXT", nullable: false)
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
                name: "PresetFailureDefinition",
                columns: table => new
                {
                    FailureDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FailurePresetId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetFailureDefinition", x => new { x.FailureDefinitionId, x.FailurePresetId });
                    table.ForeignKey(
                        name: "FK_PresetFailureDefinition_FailureDefinitions_FailureDefinitionId",
                        column: x => x.FailureDefinitionId,
                        principalTable: "FailureDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresetFailureDefinition_FailurePresets_FailurePresetId",
                        column: x => x.FailurePresetId,
                        principalTable: "FailurePresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TriggeredFailures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlightSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FailureDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PresetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TriggeredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FlightPhase = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggeredFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FailureDefinitions_FailureDefinitionId",
                        column: x => x.FailureDefinitionId,
                        principalTable: "FailureDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TriggeredFailures_FailurePresets_PresetId",
                        column: x => x.PresetId,
                        principalTable: "FailurePresets",
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
                table: "FailureDefinitions",
                columns: new[] { "Id", "AffectedSystem", "ApplicableFlightPhase", "Name", "Probability", "Severity" },
                values: new object[,]
                {
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1001"), "Cabin", 1, "Cabin Light Burnout", 0.23000000000000001, 1 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1002"), "AirConditioning", 1, "Cabin Temp Sensor Out of Range", 0.17999999999999999, 1 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1003"), "Electrical", 5, "Galley Bus Intermittent", 0.14000000000000001, 1 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1004"), "Avionics", 5, "Secondary Avionics Degraded", 0.10000000000000001, 2 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1005"), "Pneumatics", 4, "PACK Fault", 0.080000000000000002, 2 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1006"), "Hydraulics", 6, "Hydraulic Low Pressure", 0.040000000000000001, 3 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1007"), "Engine", 3, "Engine Fire Warning", 0.02, 3 },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1008"), "IceProtection", 7, "Wing Anti-Ice Valve Fault", 0.059999999999999998, 2 }
                });

            migrationBuilder.InsertData(
                table: "FailurePresets",
                columns: new[] { "Id", "Description", "Name", "PresetType" },
                values: new object[,]
                {
                    { new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc001"), "Fallas menores iniciales para forzar checklist real.", "Cold & Dark Immersion", 1 },
                    { new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc002"), "Fallas aleatorias menores durante el vuelo.", "Random Non-Critical", 2 },
                    { new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc003"), "Fallas aleatorias con posibilidad de fallas críticas.", "Random with Critical", 3 },
                    { new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc004"), "Fallas críticas por fase para entrenamiento.", "Training Mode", 4 },
                    { new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005"), "Distribución cercana a operación real A320.", "Realistic Mode", 5 },
                    { new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc006"), "Preset vacío para reglas del usuario.", "Custom", 6 }
                });

            migrationBuilder.InsertData(
                table: "PresetFailureDefinition",
                columns: new[] { "FailureDefinitionId", "FailurePresetId" },
                values: new object[,]
                {
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1001"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc001") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1001"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc002") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1001"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1002"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc001") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1002"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc002") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1002"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1003"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc002") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1003"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc003") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1003"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1004"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc002") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1004"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc003") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1004"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1005"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc003") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1005"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc004") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1005"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1006"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc003") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1006"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc004") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1006"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1007"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc003") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1007"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc004") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1007"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1008"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc004") },
                    { new Guid("c13afc03-95e4-439f-b464-95ce3a7e1008"), new Guid("08fcd6c9-0e35-4a28-86e5-4ab78a4cc005") }
                });

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
                name: "FailureDefinitions");

            migrationBuilder.DropTable(
                name: "FlightSessions");

            migrationBuilder.DropTable(
                name: "FailurePresets");
        }
    }
}
