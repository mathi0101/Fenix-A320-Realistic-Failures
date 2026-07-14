using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealFenixFailures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Sessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TriggeredFailures_FailurePresets_PresetId",
                table: "TriggeredFailures");

            migrationBuilder.DropIndex(
                name: "IX_TriggeredFailures_PresetId",
                table: "TriggeredFailures");

            migrationBuilder.DropColumn(
                name: "PresetId",
                table: "TriggeredFailures");

            migrationBuilder.RenameColumn(
                name: "TriggeredAtUtc",
                table: "TriggeredFailures",
                newName: "TriggeredAt");

            migrationBuilder.RenameColumn(
                name: "StartedAtUtc",
                table: "FlightSessions",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedAtUtc",
                table: "AircraftSystemWears",
                newName: "LastUpdatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "UserAircraftId",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RiskLevel",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TriggeredAt",
                table: "TriggeredFailures",
                newName: "TriggeredAtUtc");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "FlightSessions",
                newName: "StartedAtUtc");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedAt",
                table: "AircraftSystemWears",
                newName: "LastUpdatedAtUtc");

            migrationBuilder.AddColumn<int>(
                name: "PresetId",
                table: "TriggeredFailures",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserAircraftId",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "RiskLevel",
                table: "FlightSessions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_TriggeredFailures_PresetId",
                table: "TriggeredFailures",
                column: "PresetId");

            migrationBuilder.AddForeignKey(
                name: "FK_TriggeredFailures_FailurePresets_PresetId",
                table: "TriggeredFailures",
                column: "PresetId",
                principalTable: "FailurePresets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
