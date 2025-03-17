using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class DelegadoTieneUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "Usuario",
                table: "Delegados");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Delegados",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Delegados",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Delegados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$eApHtnPNGVPdlZXTQe7A5uN4eZ9zHjfZcoCKKACdOcQbiGWrLXZI.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$miPry4RRyPtzE7k1gnmj2Oc/RZxJjsgYk2s9AiqkohOhRjniLiyCG");

            migrationBuilder.CreateIndex(
                name: "IX_Delegados_UsuarioId",
                table: "Delegados",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados");

            migrationBuilder.DropIndex(
                name: "IX_Delegados_UsuarioId",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Delegados");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Delegados",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Delegados",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Delegados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Usuario",
                table: "Delegados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
        }
    }
}
