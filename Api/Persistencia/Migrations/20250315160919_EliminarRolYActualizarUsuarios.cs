using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EliminarRolYActualizarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Usuarios");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NombreUsuario", "Password" },
                values: new object[] { "mati", "$2a$12$Z448AmN.U41zJ4unBdlWS.1JpoGdVzzWI5K0ptdigtzQNVKNvwdhu" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NombreUsuario", "Password" },
                values: new object[] { "pipa", "$2a$12$uBBTnnXXC/5ATjsTubwSKOmiC7zqe/JORoTTrWqdjhzN5hsHYnoKy" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NombreUsuario", "Password", "Rol" },
                values: new object[] { "admin", "admin123", "" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NombreUsuario", "Password", "Rol" },
                values: new object[] { "usuario", "usuario123", "" });
        }
    }
}
