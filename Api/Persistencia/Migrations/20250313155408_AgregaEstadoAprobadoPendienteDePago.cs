using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEstadoAprobadoPendienteDePago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EstadoJugador",
                columns: new[] { "Id", "Estado" },
                values: new object[] { 6, "Aprobado pendiente de pago" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EstadoJugador",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
