using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarFechaInstanciaColumnas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.DropIndex(
                name: "IX_Fechas_ZonaId_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.RenameColumn(
                name: "InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                newName: "InstanciaId");

            migrationBuilder.RenameIndex(
                name: "IX_Fechas_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                newName: "IX_Fechas_InstanciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Fechas_ZonaId_InstanciaId",
                schema: "dbo",
                table: "Fechas",
                columns: new[] { "ZonaId", "InstanciaId" },
                unique: true,
                filter: "[InstanciaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Fechas__InstanciaEliminacionDirecta_InstanciaId",
                schema: "dbo",
                table: "Fechas",
                column: "InstanciaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fechas__InstanciaEliminacionDirecta_InstanciaId",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.DropIndex(
                name: "IX_Fechas_ZonaId_InstanciaId",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.RenameColumn(
                name: "InstanciaId",
                schema: "dbo",
                table: "Fechas",
                newName: "InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_Fechas_InstanciaId",
                schema: "dbo",
                table: "Fechas",
                newName: "IX_Fechas_InstanciaEliminacionDirectaId");

            migrationBuilder.CreateIndex(
                name: "IX_Fechas_ZonaId_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                columns: new[] { "ZonaId", "InstanciaEliminacionDirectaId" },
                unique: true,
                filter: "[InstanciaEliminacionDirectaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Fechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
