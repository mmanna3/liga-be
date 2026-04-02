using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ZonaEliminacionDirectaCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                schema: "dbo",
                table: "Zonas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zonas_CategoriaId",
                schema: "dbo",
                table: "Zonas",
                column: "CategoriaId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Zonas_EliminacionDirecta_TieneCategoria",
                schema: "dbo",
                table: "Zonas",
                sql: "([TipoZona] <> N'EliminacionDirecta') OR ([CategoriaId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Zonas_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "Zonas",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "TorneoCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zonas_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropIndex(
                name: "IX_Zonas_CategoriaId",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Zonas_EliminacionDirecta_TieneCategoria",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                schema: "dbo",
                table: "Zonas");
        }
    }
}
