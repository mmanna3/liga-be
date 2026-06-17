using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ArbitroJornadaWhatsappMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsappCategoriasJson",
                schema: "dbo",
                table: "ArbitroJornada",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhatsappEnviadoEn",
                schema: "dbo",
                table: "ArbitroJornada",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappHorarioInicio",
                schema: "dbo",
                table: "ArbitroJornada",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappObservaciones",
                schema: "dbo",
                table: "ArbitroJornada",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsappCategoriasJson",
                schema: "dbo",
                table: "ArbitroJornada");

            migrationBuilder.DropColumn(
                name: "WhatsappEnviadoEn",
                schema: "dbo",
                table: "ArbitroJornada");

            migrationBuilder.DropColumn(
                name: "WhatsappHorarioInicio",
                schema: "dbo",
                table: "ArbitroJornada");

            migrationBuilder.DropColumn(
                name: "WhatsappObservaciones",
                schema: "dbo",
                table: "ArbitroJornada");
        }
    }
}
