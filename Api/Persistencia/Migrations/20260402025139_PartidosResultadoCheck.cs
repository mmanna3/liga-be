using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class PartidosResultadoCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partidos",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    JornadaId = table.Column<int>(type: "int", nullable: false),
                    ResultadoLocal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultadoVisitante = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidos", x => x.Id);
                    table.CheckConstraint("CK_Partido_ResultadoLocal_Valido", "([ResultadoLocal] NOT LIKE '%[^0-9]%' OR [ResultadoLocal] IN (N'NP', N'S', N'P', N'GP', N'PP'))");
                    table.CheckConstraint("CK_Partido_ResultadoVisitante_Valido", "([ResultadoVisitante] NOT LIKE '%[^0-9]%' OR [ResultadoVisitante] IN (N'NP', N'S', N'P', N'GP', N'PP'))");
                    table.ForeignKey(
                        name: "FK_Partidos_Jornadas_JornadaId",
                        column: x => x.JornadaId,
                        principalSchema: "dbo",
                        principalTable: "Jornadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Partidos_TorneoCategorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "dbo",
                        principalTable: "TorneoCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_CategoriaId",
                schema: "dbo",
                table: "Partidos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_JornadaId",
                schema: "dbo",
                table: "Partidos",
                column: "JornadaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partidos",
                schema: "dbo");
        }
    }
}
