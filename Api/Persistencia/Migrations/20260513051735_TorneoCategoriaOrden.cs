using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoCategoriaOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TorneoCategorias_TorneoId",
                schema: "dbo",
                table: "TorneoCategorias");

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                schema: "dbo",
                table: "TorneoCategorias",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                WITH OrdenadoPorTorneo AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (PARTITION BY TorneoId ORDER BY Nombre, Id) AS NuevoOrden
                    FROM dbo.TorneoCategorias
                )
                UPDATE c
                SET c.Orden = o.NuevoOrden
                FROM dbo.TorneoCategorias c
                INNER JOIN OrdenadoPorTorneo o ON o.Id = c.Id;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                schema: "dbo",
                table: "TorneoCategorias",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TorneoCategorias_TorneoId_Orden",
                schema: "dbo",
                table: "TorneoCategorias",
                columns: new[] { "TorneoId", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TorneoCategorias_TorneoId_Orden",
                schema: "dbo",
                table: "TorneoCategorias");

            migrationBuilder.DropColumn(
                name: "Orden",
                schema: "dbo",
                table: "TorneoCategorias");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoCategorias_TorneoId",
                schema: "dbo",
                table: "TorneoCategorias",
                column: "TorneoId");
        }
    }
}
