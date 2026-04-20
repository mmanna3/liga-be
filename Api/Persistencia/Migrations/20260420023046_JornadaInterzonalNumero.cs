using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class JornadaInterzonalNumero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Numero",
                schema: "dbo",
                table: "Jornadas",
                type: "int",
                nullable: true);

            // Numeración por fecha: 1..n según Id ascendente (estable para datos ya existentes).
            migrationBuilder.Sql(
                """
                ;WITH ranked AS (
                    SELECT j.Id,
                           ROW_NUMBER() OVER (PARTITION BY j.FechaId ORDER BY j.Id) AS rn
                    FROM dbo.Jornadas AS j
                    WHERE j.Tipo = N'Interzonal'
                )
                UPDATE j
                SET j.Numero = ranked.rn
                FROM dbo.Jornadas AS j
                INNER JOIN ranked ON j.Id = ranked.Id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_FechaId_Numero",
                schema: "dbo",
                table: "Jornadas",
                columns: new[] { "FechaId", "Numero" },
                unique: true,
                filter: "[Tipo] = N'Interzonal'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jornadas_FechaId_Numero",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropColumn(
                name: "Numero",
                schema: "dbo",
                table: "Jornadas");
        }
    }
}
