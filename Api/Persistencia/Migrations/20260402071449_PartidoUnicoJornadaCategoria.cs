using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class PartidoUnicoJornadaCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partidos_JornadaId",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_JornadaId_CategoriaId",
                schema: "dbo",
                table: "Partidos",
                columns: new[] { "JornadaId", "CategoriaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partidos_JornadaId_CategoriaId",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_JornadaId",
                schema: "dbo",
                table: "Partidos",
                column: "JornadaId");
        }
    }
}
