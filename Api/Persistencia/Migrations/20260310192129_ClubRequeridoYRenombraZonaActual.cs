using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ClubRequeridoYRenombraZonaActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_TorneoZonas_TorneoZonaId",
                table: "Equipos");

            migrationBuilder.RenameColumn(
                name: "TorneoZonaId",
                table: "Equipos",
                newName: "ZonaActualId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipos_TorneoZonaId",
                table: "Equipos",
                newName: "IX_Equipos_ZonaActualId");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$ELlmZj3FFROmsnkzARQSjO5XIGCCckjrDowtD7NAUjoYM279VfahW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Xtf43yi52WenUiUBtaasWuuYDdYfCWNbLZVBtfYyDxMQFyg04pfRy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$ziEGftzpDCuZZgKQ4iRuJuSpAzL1SYUZItf8mw4jH2CjERkIc7/zO");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaActualId",
                table: "Equipos",
                column: "ZonaActualId",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaActualId",
                table: "Equipos");

            migrationBuilder.RenameColumn(
                name: "ZonaActualId",
                table: "Equipos",
                newName: "TorneoZonaId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipos_ZonaActualId",
                table: "Equipos",
                newName: "IX_Equipos_TorneoZonaId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_TorneoZonas_TorneoZonaId",
                table: "Equipos",
                column: "TorneoZonaId",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
