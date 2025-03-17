using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class PasswordDelDelegadoPuedeSerNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HistorialDePagos_JugadorEquipoId",
                table: "HistorialDePagos");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Delegados",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$j5FSczH0wjmaydQHI8jkLuY4pgdUw.50sRzE6NE52kBKucQRAnrxi");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$/358f1/4/wp0Q1UCBlL0gestqCJwgt35SQ4OOtpgF3r.G2FPk858i");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialDePagos_JugadorEquipoId",
                table: "HistorialDePagos",
                column: "JugadorEquipoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HistorialDePagos_JugadorEquipoId",
                table: "HistorialDePagos");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Delegados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Hbsv.tb10.RYcR8DKKDTVOBR2Q5woQp78Dv4k/lY8K4PoPnNA.SPO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$s3KC0wpz4g8ux2UY58jeh.Shw.BJjy8qzet9.Z36duvXPxzH9jA9C");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialDePagos_JugadorEquipoId",
                table: "HistorialDePagos",
                column: "JugadorEquipoId");
        }
    }
}
