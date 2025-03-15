using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTorneoYRelacionConEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TorneoId",
                table: "Equipos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Torneos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torneos", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$I678wGX0OX1N0fh0Q7E0uex6TimajpOkEDdbdTodVLzyCF/L60QDu");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$qXtm5ZBl6o3l8NwJLJhziO7gomOiHrdQhkANekNbc94DRDinlc3Be");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_TorneoId",
                table: "Equipos",
                column: "TorneoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_Torneos_TorneoId",
                table: "Equipos",
                column: "TorneoId",
                principalTable: "Torneos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_Torneos_TorneoId",
                table: "Equipos");

            migrationBuilder.DropTable(
                name: "Torneos");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_TorneoId",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "TorneoId",
                table: "Equipos");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$.fcQkJ5oIxYYp5LZSXfCuuSEqVxqL8rU2vHa7lVbCspbw6zLLlQ/q");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$9nHVVAo7FvgXwaGnnsUYDOrFQPdjuaP5R58RTL9TiOAkGHbnp6S0C");
        }
    }
}
