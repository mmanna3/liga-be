using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaKeyEnJugadorEquipoParaEquipoYJugador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JugadorEquipo_EquipoId",
                table: "JugadorEquipo");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nombre",
                value: "Usuario");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$KzRpuLNKIQmw6BzenQxaFekioVGg69CIOHCvW/cj3Tbz.5rxFUCUC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$okGG1dbFcddqKEN3uzZHueKD8G6vj6IZRNWJbCJwC6E5QaxoWR79m");

            migrationBuilder.CreateIndex(
                name: "IX_JugadorEquipo_EquipoId_JugadorId",
                table: "JugadorEquipo",
                columns: new[] { "EquipoId", "JugadorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JugadorEquipo_EquipoId_JugadorId",
                table: "JugadorEquipo");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nombre",
                value: "Delegado");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$FUAdnPbuTuQxaTIw4VsAYOPPAd88Cf8S1GUkPKMyH8kueewFeNE9e");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Hx/AI12bIeKpaPCxrgxVjOObfGkm0G0YP6IqzN5d..jrmfXa/.Ste");

            migrationBuilder.CreateIndex(
                name: "IX_JugadorEquipo_EquipoId",
                table: "JugadorEquipo",
                column: "EquipoId");
        }
    }
}
