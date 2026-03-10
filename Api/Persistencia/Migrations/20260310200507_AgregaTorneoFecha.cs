using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTorneoFecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TorneoFechas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dia = table.Column<DateOnly>(type: "date", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    ZonaId = table.Column<int>(type: "int", nullable: false),
                    InstanciaEliminacionDirectaId = table.Column<int>(type: "int", nullable: true),
                    EsVisibleEnApp = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneoFechas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorneoFechas_TorneoZonas_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "TorneoZonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                        column: x => x.InstanciaEliminacionDirectaId,
                        principalTable: "_InstanciaEliminacionDirecta",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$psIAxd8oaHn/KfAMuwZl0eJsq8.aeTaf7vA8phKuXTozz5UGSKSnW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$i3nQFBfJt.3KAH8XIPVhY.FoHu2j5blzFsYnjgSuR6SMtkmWGQqH6");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$sQ4IDCja57YApUfFIf47febds0YsVDDGs0cAsBUv8NgzQ.3TW4Jya");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFechas_InstanciaEliminacionDirectaId",
                table: "TorneoFechas",
                column: "InstanciaEliminacionDirectaId");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFechas_ZonaId_Numero",
                table: "TorneoFechas",
                columns: new[] { "ZonaId", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TorneoFechas");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$5OGoNTPwcMexYcIHwMv/cu2qUB0A7tQ9jV7LTIbjx6eS7ma2Ax7ua");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$1LA6XfVy4Ee3xj6opM0c8.QlpM5u8qtM3oeR4FIcz1ILZ4mg/EhvW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$vqC1rI8R4koJ1PCq.NUbd.bmRKWMx8wim4dPHVka0gZ2I1UaYbqee");
        }
    }
}
