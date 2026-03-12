using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEquipoZonaNoExcluyenteYRenombraZonaExcluyente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaActualId",
                table: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_Nombre_ZonaActualId",
                table: "Equipos");

            migrationBuilder.RenameColumn(
                name: "ZonaActualId",
                table: "Equipos",
                newName: "ZonaExcluyenteId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipos_ZonaActualId",
                table: "Equipos",
                newName: "IX_Equipos_ZonaExcluyenteId");

            migrationBuilder.CreateTable(
                name: "EquipoZonaNoExcluyente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    ZonaNoExcluyenteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoZonaNoExcluyente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipoZonaNoExcluyente_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipoZonaNoExcluyente_TorneoZonas_ZonaNoExcluyenteId",
                        column: x => x.ZonaNoExcluyenteId,
                        principalTable: "TorneoZonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Lmd2ZL1C/j46BihEQmFFF.HvpKSgWFrhas1r9wZ13n9/tYYvYI4im");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$ThFCdHrLFe5mVff5GdTy/O0zi9B4CeaBdcumBUCVXmEAAEfLa8oi6");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$RC2XGBMNR0Vf0u8fZoAkO.GBGN3XtTaEGLSR5T8jVEkZoeZbiViSK");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Nombre_ZonaExcluyenteId",
                table: "Equipos",
                columns: new[] { "Nombre", "ZonaExcluyenteId" },
                unique: true,
                filter: "[ZonaExcluyenteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquipoZonaNoExcluyente_EquipoId_ZonaNoExcluyenteId",
                table: "EquipoZonaNoExcluyente",
                columns: new[] { "EquipoId", "ZonaNoExcluyenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipoZonaNoExcluyente_ZonaNoExcluyenteId",
                table: "EquipoZonaNoExcluyente",
                column: "ZonaNoExcluyenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaExcluyenteId",
                table: "Equipos",
                column: "ZonaExcluyenteId",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaExcluyenteId",
                table: "Equipos");

            migrationBuilder.DropTable(
                name: "EquipoZonaNoExcluyente");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_Nombre_ZonaExcluyenteId",
                table: "Equipos");

            migrationBuilder.RenameColumn(
                name: "ZonaExcluyenteId",
                table: "Equipos",
                newName: "ZonaActualId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipos_ZonaExcluyenteId",
                table: "Equipos",
                newName: "IX_Equipos_ZonaActualId");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$L0Sv132jFICqArEVRPSL5uyJkdiafVTk0ZPebTxoB12Soi8GpAssG");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$MqwB98/5Djt1UvjQPuqs2uGi74cM4ZGUzIKIBNYZRhQZyX/Em9miC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$UM5xKG0RG2UbSu8r1eoas.UHEktVY7k/uYwPW8Mtl6HpxtfjIFAIu");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Nombre_ZonaActualId",
                table: "Equipos",
                columns: new[] { "Nombre", "ZonaActualId" },
                unique: true,
                filter: "[ZonaActualId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaActualId",
                table: "Equipos",
                column: "ZonaActualId",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
