using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ZonaEliminacionDirectaIndiceUnicoFaseCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Zonas_FaseId_CategoriaId",
                schema: "dbo",
                table: "Zonas",
                columns: new[] { "FaseId", "CategoriaId" },
                unique: true,
                filter: "[CategoriaId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Zonas_FaseId_CategoriaId",
                schema: "dbo",
                table: "Zonas");
        }
    }
}
