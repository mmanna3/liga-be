using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaRolYUsuarioConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 3, "Consulta" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$lAav8qI16d9AMiGjVckv8.HmEZMr5tdwsJ.EFPWyVAevehHB5VRwK");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$tOw.o3UPY3rByQfcR32SZeB99Q4ZO.ExImSBKenx0TS65HWuvv.hG");

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "NombreUsuario", "Password", "RolId" },
                values: new object[] { 101, "consulta", "$2a$12$wjAmPCPMAa/shrlArH.4lOEQa52G8qcG2bJNRDyBJv0UvhpskhU/a", 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

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
        }
    }
}
