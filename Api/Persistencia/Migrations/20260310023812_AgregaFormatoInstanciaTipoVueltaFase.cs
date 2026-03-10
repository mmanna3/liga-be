using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaFormatoInstanciaTipoVueltaFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_FormatoDeLaFase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FormatoDeLaFase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_InstanciaEliminacionDirecta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__InstanciaEliminacionDirecta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_TipoVueltaDeLaFase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoVueltaDeLaFase", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$JfUmGnvnRJtecXzxPIYjZ.GDJW9zKVky3F2gQPVW1jJtEzv8ach0i");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$uxN6WuUI5lJRCAGI2jogXOfbTHVzkM0OTLJbHp8KAEz79J.D4.3o.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$38.AGy5HYdytLu10YBiEiu6nXuB49JujRt6GAS/4JtO5GUxunSXk2");

            migrationBuilder.InsertData(
                table: "_FormatoDeLaFase",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Todos contra todos" },
                    { 2, "Eliminación directa" }
                });

            migrationBuilder.InsertData(
                table: "_InstanciaEliminacionDirecta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 2, "Final" },
                    { 4, "Semifinal" },
                    { 8, "Cuartos de final" },
                    { 16, "Octavos de final" }
                });

            migrationBuilder.InsertData(
                table: "_TipoVueltaDeLaFase",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Solo ida" },
                    { 2, "Ida y vuelta" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_FormatoDeLaFase");

            migrationBuilder.DropTable(
                name: "_InstanciaEliminacionDirecta");

            migrationBuilder.DropTable(
                name: "_TipoVueltaDeLaFase");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Cwjj./OvbfGsnoa1BG6VDe9MVLcrCw6VIBA9B6fuNRpPHfPI2JNZC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$J8nZvQWhhXN00S/q66jYmuHnsWvEyalBpsqjUajuMCk7Aazb0oQq.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$yzdDhZE3kx8aAzm9yBxhOuRxQLx1x.LnW4/sDnHtvxxD5lNQeBCrW");
        }
    }
}
