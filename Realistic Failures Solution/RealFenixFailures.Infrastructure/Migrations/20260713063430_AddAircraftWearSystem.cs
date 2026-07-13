using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RealFenixFailures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftWearSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RiskLevel",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserAircraftId",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: true);

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
                name: "UserAircrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Registration = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IcaoTypeCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TotalFlightHours = table.Column<double>(type: "REAL", nullable: false),
                    TotalFlights = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAircrafts", x => x.Id);
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
                    LastUpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_UserAircraftId",
                table: "FlightSessions",
                column: "UserAircraftId");

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
                name: "IX_UserAircrafts_Registration",
                table: "UserAircrafts",
                column: "Registration",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FlightSessions_UserAircrafts_UserAircraftId",
                table: "FlightSessions",
                column: "UserAircraftId",
                principalTable: "UserAircrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlightSessions_UserAircrafts_UserAircraftId",
                table: "FlightSessions");

            migrationBuilder.DropTable(
                name: "AircraftSystemWears");

            migrationBuilder.DropTable(
                name: "AircraftWearableSystems");

            migrationBuilder.DropTable(
                name: "UserAircrafts");

            migrationBuilder.DropIndex(
                name: "IX_FlightSessions_UserAircraftId",
                table: "FlightSessions");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "FlightSessions");

            migrationBuilder.DropColumn(
                name: "UserAircraftId",
                table: "FlightSessions");
        }
    }
}
