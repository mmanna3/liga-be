using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class agregajugadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadoJugador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoJugador", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jugadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DNI = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jugadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JugadorEquipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JugadorId = table.Column<int>(type: "int", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    FechaFichaje = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoJugadorId = table.Column<int>(type: "int", nullable: false),
                    MotivoDeRechazoFichaje = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JugadorEquipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JugadorEquipo_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JugadorEquipo_EstadoJugador_EstadoJugadorId",
                        column: x => x.EstadoJugadorId,
                        principalTable: "EstadoJugador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JugadorEquipo_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EstadoJugador",
                columns: new[] { "Id", "Estado" },
                values: new object[,]
                {
                    { 1, "Fichaje pendiente de aprobación" },
                    { 2, "Fichaje rechazado" },
                    { 3, "Activo" },
                    { 4, "Suspendido" },
                    { 5, "Inhabilitado" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JugadorEquipo_EquipoId",
                table: "JugadorEquipo",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_JugadorEquipo_EstadoJugadorId",
                table: "JugadorEquipo",
                column: "EstadoJugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_JugadorEquipo_JugadorId",
                table: "JugadorEquipo",
                column: "JugadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JugadorEquipo");

            migrationBuilder.DropTable(
                name: "EstadoJugador");

            migrationBuilder.DropTable(
                name: "Jugadores");
        }
    }
}
