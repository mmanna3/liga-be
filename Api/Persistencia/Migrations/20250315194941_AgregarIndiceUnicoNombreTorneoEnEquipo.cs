using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndiceUnicoNombreTorneoEnEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Equipos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$MKXlbwDyyY1xdSE.IOWeN.37nQriH/sNF/9URVrPTmAxCPRkhInS.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$NLiGfekAI8AnecfbnQEQUeYmLwGFBDjPcJm4aFwgrK1Nr5ZN7ivX6");

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_Nombre_TorneoId",
                table: "Equipos",
                columns: new[] { "Nombre", "TorneoId" },
                unique: true,
                filter: "[TorneoId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equipo_Nombre_TorneoId",
                table: "Equipos");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Equipos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
        }
    }
}
