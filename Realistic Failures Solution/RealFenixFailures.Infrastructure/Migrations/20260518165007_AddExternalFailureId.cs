using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealFenixFailures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalFailureId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalFailureId",
                table: "FailureDefinitions",
                type: "TEXT",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1001"),
                column: "ExternalFailureId",
                value: "F_PNEUMATIC_CPC_1");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1002"),
                column: "ExternalFailureId",
                value: "F_PNEUMATIC_PACK_1_OVERHEAT");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1003"),
                column: "ExternalFailureId",
                value: "F_ELEC_AC_ESS_FEED_1");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1004"),
                column: "ExternalFailureId",
                value: "F_FMGC_1");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1005"),
                column: "ExternalFailureId",
                value: "F_PNEUMATIC_TRIM_AIR");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1006"),
                column: "ExternalFailureId",
                value: "F_HYD_LOW_GREEN");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1007"),
                column: "ExternalFailureId",
                value: "F_OH_FIRE_ENG_1");

            migrationBuilder.UpdateData(
                table: "FailureDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("c13afc03-95e4-439f-b464-95ce3a7e1008"),
                column: "ExternalFailureId",
                value: "F_PNEUMATIC_WAI_1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalFailureId",
                table: "FailureDefinitions");
        }
    }
}
