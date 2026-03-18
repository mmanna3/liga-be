using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class DiaNullableEnTorneoFecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "Dia",
                schema: "dbo",
                table: "TorneoFechas",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$gxVTKJfRt4NaZWMTCqkcles37mczI168hsH3xHoQpX8YfuAlgxk36");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$0zRNCU1pgd1cirTLHmDgWOc0i/ua2HZr/C3J0KJlLMWT8HarfM9l.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$lAeKfYy86RP0sEjbPMb/P.l31U0dK0EZzQDtaipjPsM01Tru5m4nK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "Dia",
                schema: "dbo",
                table: "TorneoFechas",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$JpmfYMr1LLfqAhXzNVrR5.CX9Eh/6PGzCcZEjwVndoqtyZGL4Du9q");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$gcs3rzh.lWpxhHf7H63.PuFeax.3lCsC8gOcdiU1MItpZPZ5cfZva");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$lOb5LQAqdW.NognKCrpdUO.ryRknxxB5HAlj8r4PXmN9.sP5Wgybi");
        }
    }
}
