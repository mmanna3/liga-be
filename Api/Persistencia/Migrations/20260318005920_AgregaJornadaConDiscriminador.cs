using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaJornadaConDiscriminador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jornadas",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaId = table.Column<int>(type: "int", nullable: false),
                    ResultadosVerificados = table.Column<bool>(type: "bit", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: true),
                    LocalOVisitanteId = table.Column<int>(type: "int", nullable: true),
                    JornadaLibre_EquipoId = table.Column<int>(type: "int", nullable: true),
                    LocalEquipoId = table.Column<int>(type: "int", nullable: true),
                    VisitanteEquipoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jornadas", x => x.Id);
                    table.CheckConstraint("CK_Jornada_Tipo_Valido", "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [JornadaLibre_EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [JornadaLibre_EquipoId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [JornadaLibre_EquipoId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_Jornadas_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalSchema: "dbo",
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jornadas_Equipos_JornadaLibre_EquipoId",
                        column: x => x.JornadaLibre_EquipoId,
                        principalSchema: "dbo",
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jornadas_Equipos_LocalEquipoId",
                        column: x => x.LocalEquipoId,
                        principalSchema: "dbo",
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jornadas_Equipos_VisitanteEquipoId",
                        column: x => x.VisitanteEquipoId,
                        principalSchema: "dbo",
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jornadas_TorneoFechas_FechaId",
                        column: x => x.FechaId,
                        principalSchema: "dbo",
                        principalTable: "TorneoFechas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jornadas__LocalVisitante_LocalOVisitanteId",
                        column: x => x.LocalOVisitanteId,
                        principalSchema: "dbo",
                        principalTable: "_LocalVisitante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_EquipoId",
                schema: "dbo",
                table: "Jornadas",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_FechaId",
                schema: "dbo",
                table: "Jornadas",
                column: "FechaId");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_JornadaLibre_EquipoId",
                schema: "dbo",
                table: "Jornadas",
                column: "JornadaLibre_EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_LocalEquipoId",
                schema: "dbo",
                table: "Jornadas",
                column: "LocalEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_LocalOVisitanteId",
                schema: "dbo",
                table: "Jornadas",
                column: "LocalOVisitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_VisitanteEquipoId",
                schema: "dbo",
                table: "Jornadas",
                column: "VisitanteEquipoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jornadas",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$lS5hpvCW24f0ack48cn78uYFOs1W9o4W9/otDs3ZA1jPcVYafEIha");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$tDrzfWVg1rjpYvDKbdUhduWixmU5mgJUWN11E80qCVXsikRel5e0i");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$L8.WDx.tke1iec6ZKEr/s.R.bZjcQdOMIxVZjMpz0LKniRi8D9y1i");
        }
    }
}
