using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EliminaSeedUsuariosConsultaYBlanqueaClaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.Usuarios WHERE RolId = 3");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "_Rol",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.Sql("UPDATE dbo.Usuarios SET Password = NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_Rol",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 3, "Consulta" });
        }
    }
}
