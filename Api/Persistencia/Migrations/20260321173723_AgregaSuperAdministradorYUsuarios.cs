using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSuperAdministradorYUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_Rol",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 0, "SuperAdministrador" });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RolId" },
                values: new object[] { "$2a$12$cySkpsz/jgZ4gF6EbHVpK.X6JoWSp55XMInjFXgJliBcs.R6b.LO.", 0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$sPHYdMQWhMGWHCgUG/irFuyTZrr2rqYSIlbHDWTcnl0kIF1eODi2y");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$8xq9kwxoH1k10a24umalFOHLEJageScjy8K2.2B6YBzXBSmfQjssi");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Usuarios",
                columns: new[] { "Id", "DelegadoId", "NombreUsuario", "Password", "RolId" },
                values: new object[,]
                {
                    { 1000, null, "eze", "$2a$12$Uh0CLF/GaOYogMZZER8A2.qSzgPyF7tVRkIAzBn1K7wvJ0tXGbetm", 1 },
                    { 1001, null, "lucas", "$2a$12$dwC/NXxq7pyFjDtfzmGLs.wKizMUsW3fBOMD71GXNeffV.QvUNI4K", 1 },
                    { 1002, null, "elias", "$2a$12$JDxEqjXekf2NBlTI4wfOaOCqhFN087bA1HtJlWBbDfbL6U6Rewspy", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RolId" },
                values: new object[] { "$2a$12$.GzqiTYWbLDSDjgqoPcmZeTRXYqecOMw8iLXd6FcOVAHB7cjl.NRS", 1 });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "_Rol",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$bgRU8l/M3ZtIvUO.Vcrv8uJ/w4S1UVbazyI565NbPUXMm7bSBc3ni");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$UR2pd2xfs4dIsK/r8cjAi.M2DgShOC9Uj6hB.oxUi0xl.jzMoccSW");
        }
    }
}
