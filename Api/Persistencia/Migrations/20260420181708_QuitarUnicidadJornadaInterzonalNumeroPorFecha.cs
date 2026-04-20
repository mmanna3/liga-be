using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class QuitarUnicidadJornadaInterzonalNumeroPorFecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jornadas_FechaId_Numero",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_FechaId_Numero",
                schema: "dbo",
                table: "Jornadas",
                columns: new[] { "FechaId", "Numero" },
                filter: "[Tipo] = N'Interzonal'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jornadas_FechaId_Numero",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_FechaId_Numero",
                schema: "dbo",
                table: "Jornadas",
                columns: new[] { "FechaId", "Numero" },
                unique: true,
                filter: "[Tipo] = N'Interzonal'");
        }
    }
}
