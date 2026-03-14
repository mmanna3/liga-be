using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarFixtureAlgoritmoPartidosAPartido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FixtureAlgoritmos",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CantidadDeFechas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureAlgoritmos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FixtureAlgoritmoPartido",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureAlgoritmoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<int>(type: "int", nullable: false),
                    EquipoLocal = table.Column<int>(type: "int", nullable: false),
                    EquipoVisitante = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureAlgoritmoPartido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureAlgoritmoPartido_FixtureAlgoritmos_FixtureAlgoritmoId",
                        column: x => x.FixtureAlgoritmoId,
                        principalSchema: "dbo",
                        principalTable: "FixtureAlgoritmos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$WY0LE5yf8vM8fmCiWiBJuO4s8DHDAykq2PSf3kscCeNc83qkF7bQG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$FlQmVHPcwcS25DK8rHpLDOFzV7HdVjllCHN4AZRyAzQkqXCDPaV4O");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$7oO5xAV3amXSwobdEYX7vuHhZAQjDuB6Y0JmzaZIQHA8WI3dGWZ2q");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureAlgoritmoPartido_FixtureAlgoritmoId_Fecha_EquipoLocal_EquipoVisitante",
                schema: "dbo",
                table: "FixtureAlgoritmoPartido",
                columns: new[] { "FixtureAlgoritmoId", "Fecha", "EquipoLocal", "EquipoVisitante" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixtureAlgoritmoPartido",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FixtureAlgoritmos",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$3wTFUPnoEV8pqXaAx2.7seadx2qrdq1lQniyLltI2SeFfhAJwvwbG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$2HYmjpbUceChv1L9H2maX.EO0UMVGvlQBowbN1l1ePno9HsOFEuAe");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$fHiOiz/bQ3B9XNJ/AGrCMuvhgqmpvwyhwXfpRXuQWDwJtM0wc.oey");
        }
    }
}
