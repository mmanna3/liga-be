using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class FixtureAlgoritmoFechaOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Orden",
                schema: "dbo",
                table: "FixtureAlgoritmoFecha",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                WITH OrdenadoPorFecha AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY FixtureAlgoritmoId, Fecha
                            ORDER BY Id
                        ) AS NuevoOrden
                    FROM dbo.FixtureAlgoritmoFecha
                )
                UPDATE f
                SET f.Orden = o.NuevoOrden
                FROM dbo.FixtureAlgoritmoFecha f
                INNER JOIN OrdenadoPorFecha o ON o.Id = f.Id;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                schema: "dbo",
                table: "FixtureAlgoritmoFecha",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixtureAlgoritmoFecha_FixtureAlgoritmoId_Fecha_Orden",
                schema: "dbo",
                table: "FixtureAlgoritmoFecha",
                columns: new[] { "FixtureAlgoritmoId", "Fecha", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FixtureAlgoritmoFecha_FixtureAlgoritmoId_Fecha_Orden",
                schema: "dbo",
                table: "FixtureAlgoritmoFecha");

            migrationBuilder.DropColumn(
                name: "Orden",
                schema: "dbo",
                table: "FixtureAlgoritmoFecha");
        }
    }
}
