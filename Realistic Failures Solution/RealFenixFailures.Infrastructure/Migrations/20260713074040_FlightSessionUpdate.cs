using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealFenixFailures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FlightSessionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlightSessions_FailurePresets_PresetId",
                table: "FlightSessions");

            migrationBuilder.DropIndex(
                name: "IX_FlightSessions_PresetId",
                table: "FlightSessions");

            migrationBuilder.DropColumn(
                name: "PresetId",
                table: "FlightSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PresetId",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_PresetId",
                table: "FlightSessions",
                column: "PresetId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlightSessions_FailurePresets_PresetId",
                table: "FlightSessions",
                column: "PresetId",
                principalTable: "FailurePresets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
