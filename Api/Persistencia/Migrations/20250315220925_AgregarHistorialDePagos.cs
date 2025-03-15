using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialDePagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialDePagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JugadorEquipoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialDePagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialDePagos_JugadorEquipo_JugadorEquipoId",
                        column: x => x.JugadorEquipoId,
                        principalTable: "JugadorEquipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Hbsv.tb10.RYcR8DKKDTVOBR2Q5woQp78Dv4k/lY8K4PoPnNA.SPO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$s3KC0wpz4g8ux2UY58jeh.Shw.BJjy8qzet9.Z36duvXPxzH9jA9C");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialDePagos_JugadorEquipoId",
                table: "HistorialDePagos",
                column: "JugadorEquipoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialDePagos");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$MKXlbwDyyY1xdSE.IOWeN.37nQriH/sNF/9URVrPTmAxCPRkhInS.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$NLiGfekAI8AnecfbnQEQUeYmLwGFBDjPcJm4aFwgrK1Nr5ZN7ivX6");
        }
    }
}
