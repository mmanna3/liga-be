using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class QuitarInstanciaDeFaseEliminacionDirecta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropIndex(
                name: "IX_Fases_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropColumn(
                name: "InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fases_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases",
                column: "InstanciaEliminacionDirectaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id");
        }
    }
}
