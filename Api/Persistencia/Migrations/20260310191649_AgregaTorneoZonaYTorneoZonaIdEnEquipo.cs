using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTorneoZonaYTorneoZonaIdEnEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TorneoZonaId",
                table: "Equipos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TorneoZonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TorneoFaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneoZonas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorneoZonas_TorneoFases_TorneoFaseId",
                        column: x => x.TorneoFaseId,
                        principalTable: "TorneoFases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$NQAWlihrAaH9Yt.QWwgdLOD3oyD/IOxCvdV7TqNyXp1xzgKaSQHOy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$bZwvHSy3io.YmZQ71a7MYOKtHaMCHYQAEFUELuGoSeuD8XesZfzrC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Ha.Wj5ldIOD/hblxNQG.Be0.Pc9QH2uAuHpuO2aIwnMFhUZOIULGa");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_TorneoZonaId",
                table: "Equipos",
                column: "TorneoZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoZonas_TorneoFaseId",
                table: "TorneoZonas",
                column: "TorneoFaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_TorneoZonas_TorneoZonaId",
                table: "Equipos",
                column: "TorneoZonaId",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_TorneoZonas_TorneoZonaId",
                table: "Equipos");

            migrationBuilder.DropTable(
                name: "TorneoZonas");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_TorneoZonaId",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "TorneoZonaId",
                table: "Equipos");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$82oocSSw89JfE.e7v3YCOeFJjZEDgWmA/LD9Ckagi3uEe3R0xG5fy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$mo7htk0Aa3iO3OuiZm/rKux6dB7kjaf1c/fmjAiJS7ZeGNtfyLY1W");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$XPeMJkoncFrX0QvN1UtjrurQ62Ds8Ortjbl5l7BgNopWR2ZgDQaHm");
        }
    }
}
