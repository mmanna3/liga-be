using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEstadoFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_EstadoFase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EstadoFase", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$hl5mtKHu3Z2g4WTdzR3CHugEQe7FcHWueXkfOLodLrblH49XAD2tu");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$9mlY9JSDrXLhIRieITJu6uJ7y2Hk3MNrnp9f2ZIUj9n0GBw6iVBbK");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$vJoMokueYhOCxmDORzTY1uxk8NNfrgNGPEVZR750ddYFQKXoj7mS.");

            migrationBuilder.InsertData(
                table: "_EstadoFase",
                columns: new[] { "Id", "Estado" },
                values: new object[,]
                {
                    { 100, "Inicio pendiente" },
                    { 200, "En curso" },
                    { 300, "Finalizada" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_EstadoFase");

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
        }
    }
}
