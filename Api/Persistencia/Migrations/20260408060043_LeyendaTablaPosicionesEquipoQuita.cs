using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class LeyendaTablaPosicionesEquipoQuita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.AddColumn<int>(
                name: "EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuitaDePuntos",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                columns: new[] { "ZonaId", "CategoriaId" },
                unique: true,
                filter: "[EquipoId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId_EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                columns: new[] { "ZonaId", "CategoriaId", "EquipoId" },
                unique: true,
                filter: "[EquipoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_LeyendaTablaPosiciones_Equipos_EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "EquipoId",
                principalSchema: "dbo",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeyendaTablaPosiciones_Equipos_EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId_EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropColumn(
                name: "EquipoId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropColumn(
                name: "QuitaDePuntos",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "ZonaId",
                unique: true,
                filter: "[CategoriaId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                columns: new[] { "ZonaId", "CategoriaId" },
                unique: true,
                filter: "[CategoriaId] IS NOT NULL");
        }
    }
}
