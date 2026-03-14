using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarFixtureAlgoritmoACantidadDeEquiposYFechas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixtureAlgoritmoPartido",
                schema: "dbo");

            migrationBuilder.RenameColumn(
                name: "CantidadDeFechas",
                schema: "dbo",
                table: "FixtureAlgoritmos",
                newName: "CantidadDeEquipos");

            migrationBuilder.CreateTable(
                name: "FixtureAlgoritmoFecha",
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
                    table.PrimaryKey("PK_FixtureAlgoritmoFecha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureAlgoritmoFecha_FixtureAlgoritmos_FixtureAlgoritmoId",
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
                value: "$2a$12$MNom3K7gxNWhMir55Gr2heSuaqoHsJpajHgth59YsGafXi47w/e2W");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Cp4bwQa5FkNOuYpnrP2qZONFh8zhO6J0Byqz6EqNaC/3LKvPuG6OW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Gn48yQFpijemW104K9DqC.rDmjPfCv5Virf4YRwHMfYbqNajIRApa");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureAlgoritmoFecha_FixtureAlgoritmoId_Fecha_EquipoLocal_EquipoVisitante",
                schema: "dbo",
                table: "FixtureAlgoritmoFecha",
                columns: new[] { "FixtureAlgoritmoId", "Fecha", "EquipoLocal", "EquipoVisitante" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixtureAlgoritmoFecha",
                schema: "dbo");

            migrationBuilder.RenameColumn(
                name: "CantidadDeEquipos",
                schema: "dbo",
                table: "FixtureAlgoritmos",
                newName: "CantidadDeFechas");

            migrationBuilder.CreateTable(
                name: "FixtureAlgoritmoPartido",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureAlgoritmoId = table.Column<int>(type: "int", nullable: false),
                    EquipoLocal = table.Column<int>(type: "int", nullable: false),
                    EquipoVisitante = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<int>(type: "int", nullable: false)
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
    }
}
