using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EliminarExcluyenteYRenombrarEquipoZona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_TorneoZonas_ZonaExcluyenteId",
                table: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_Nombre_ZonaExcluyenteId",
                table: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_ZonaExcluyenteId",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "EsExcluyente",
                table: "TorneoFases");

            migrationBuilder.DropColumn(
                name: "ZonaExcluyenteId",
                table: "Equipos");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipoZonaNoExcluyente_Equipos_EquipoId",
                table: "EquipoZonaNoExcluyente");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipoZonaNoExcluyente_TorneoZonas_ZonaNoExcluyenteId",
                table: "EquipoZonaNoExcluyente");

            migrationBuilder.DropIndex(
                name: "IX_EquipoZonaNoExcluyente_EquipoId_ZonaNoExcluyenteId",
                table: "EquipoZonaNoExcluyente");

            migrationBuilder.DropIndex(
                name: "IX_EquipoZonaNoExcluyente_ZonaNoExcluyenteId",
                table: "EquipoZonaNoExcluyente");

            migrationBuilder.RenameTable(
                name: "EquipoZonaNoExcluyente",
                newName: "EquipoZona");

            migrationBuilder.RenameColumn(
                name: "ZonaNoExcluyenteId",
                table: "EquipoZona",
                newName: "ZonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipoZona_Equipos_EquipoId",
                table: "EquipoZona",
                column: "EquipoId",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipoZona_TorneoZonas_ZonaId",
                table: "EquipoZona",
                column: "ZonaId",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_EquipoZona_EquipoId_ZonaId",
                table: "EquipoZona",
                columns: new[] { "EquipoId", "ZonaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipoZona_ZonaId",
                table: "EquipoZona",
                column: "ZonaId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "dbo",
                table: "Equipos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$0FZW58zjzhbkS/qlX0HUUuBE74sAbleG7XiMq6xicnvDI6mHtjzw6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$GAqi.Rb.OURsr6vKFeUEy.28caU2znlK5d0ppIeN.Du5uCbM8yr9.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$l0opGwFbACIzAZ36V2hzZerq5204hZZ5YfO59tqdbppbc1mvJ2m6K");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipoZona",
                schema: "dbo");

            migrationBuilder.AddColumn<bool>(
                name: "EsExcluyente",
                table: "TorneoFases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Equipos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ZonaExcluyenteId",
                table: "Equipos",
                type: "int",
                nullable: true);

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
                name: "IX_Equipos_ZonaExcluyenteId",
                table: "Equipos",
                column: "ZonaExcluyenteId");

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
    }
}
