using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class LeyendaDeTablaAnual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.AddColumn<bool>(
                name: "EsDeTablaAnual",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId_EsDeTablaAnual",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                columns: new[] { "ZonaId", "CategoriaId", "EsDeTablaAnual" },
                unique: true,
                filter: "[EquipoId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId_EsDeTablaAnual",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropColumn(
                name: "EsDeTablaAnual",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                columns: new[] { "ZonaId", "CategoriaId" },
                unique: true,
                filter: "[EquipoId] IS NULL");
        }
    }
}
