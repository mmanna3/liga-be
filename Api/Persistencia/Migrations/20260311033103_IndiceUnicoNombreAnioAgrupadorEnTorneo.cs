using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class IndiceUnicoNombreAnioAgrupadorEnTorneo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Torneos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$1SGg97hNSSj8aTzvQcJzlOwD/4fo/tZehVDj1ZIEFHoUS0Mk3bPnO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$kxaV0IJz7.P78B/zHe4uR.csT0Db/j16EtTukXbqdfOIDvnVIIBdq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$CbhGhBcoSj.6TYyKUiLFpeUaM0IrpGV09pORti4QzEe0jVazp94wC");

            migrationBuilder.CreateIndex(
                name: "IX_Torneos_Nombre_Anio_TorneoAgrupadorId",
                table: "Torneos",
                columns: new[] { "Nombre", "Anio", "TorneoAgrupadorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Torneos_Nombre_Anio_TorneoAgrupadorId",
                table: "Torneos");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Torneos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$svOlKAf5TcDFSCGLxdMj5eE4iXjabjBL7du4VezmzFNNPEiFZ7ybW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$8FRaWJV.muw2BiUm3p.vge96QWgXZp8egSdoxZ2eGbT4.HiIBhTxC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Y5J84.HmhCcySD1pn6EunueCtczQFbB8jWmHxTlNhVLR2fJEOWSxO");
        }
    }
}
