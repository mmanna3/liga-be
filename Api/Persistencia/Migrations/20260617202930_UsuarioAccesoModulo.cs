using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioAccesoModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioAccesoModulo",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Modulo = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioAccesoModulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioAccesoModulo_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$pjP1sWcoA/byZv4zK07T6un8oTFXkQrHXTK9siarlWeY5ervVn2mu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$LmuNMxACH9xm31iVc3/Yce7FtUw6xrPDgJGq/3LVXqU9edSHemU..");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$LNYxt8cBTWB3c8tIZFbz5OjJlrZissIi2HMvKMJ.NyHALIj.FL0Nm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$VcnBw4cdXXS/Tp46Rd.kZuFPPCLPsur9WowtccC34FnBoyLrLoWXK");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$Sd4MuFJZcjuOn7nDVinokO7OBVhGWzkU1bZjMW59ls8hCa7HscU/S");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$PyjqT4rIANDPYLVwtxLPr.ZGIq1BXNUSeB2m46WYSv9/Zofp4eJxG");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAccesoModulo_UsuarioId_Modulo",
                schema: "dbo",
                table: "UsuarioAccesoModulo",
                columns: new[] { "UsuarioId", "Modulo" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO dbo.UsuarioAccesoModulo (UsuarioId, Modulo, Nivel)
                SELECT u.Id, m.Modulo, 2
                FROM dbo.Usuarios u
                CROSS JOIN (VALUES (1), (2), (3), (4), (5), (6), (7), (8)) AS m(Modulo)
                WHERE u.RolId = 1 AND u.DelegadoId IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioAccesoModulo",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$l4c3aTHmjB8NeIwaMTcbheU8DXwrmWfXjndbr.RAj3B/enNptxx.e");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$YNxfmxF8arFkf2AWWS5p7eA5ZtqUwp4ibZ1KBDPHS851nIO1zBTcy");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$5VxWOjC1Svm/jlks55HsAegWLiEJP2lt2jZU/71sFfx0UDx4pYsC6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$KhRbnkn3A5gKqcrL8/VOX.RfJxlCS1/471tVk0R39l1Y7c4sxRGPu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$yuYcvjEoSwFgNXyeJAn6u.1hqLlBXS32VTQdqA88wx0FKx/U8HqoW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$gS3YQzEtFzkgG8mxto7Bb.xKHHzxYytcroy4AkblTmXNUmxpWQE.i");
        }
    }
}
