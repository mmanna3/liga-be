using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioDelegadoCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelegadoId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            // Migrar: los Usuarios que eran referenciados por Delegados.UsuarioId ahora tienen DelegadoId
            migrationBuilder.Sql(
                "UPDATE Usuarios SET DelegadoId = d.Id FROM Delegados d WHERE d.UsuarioId = Usuarios.Id");

            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados");

            migrationBuilder.DropIndex(
                name: "IX_Delegados_UsuarioId",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Delegados");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DelegadoId", "Password" },
                values: new object[] { null, "$2a$12$Ur/a7aStH/qkw6QDyWVQsOjXa94.olzgDPnXy4qLCBvDAC6P0IDUC" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DelegadoId", "Password" },
                values: new object[] { null, "$2a$12$SGi.IWRHc95SQWGdhxplr.hoS5CEbBYKxDA0ulS5erqlMe7TnHtrK" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "DelegadoId", "Password" },
                values: new object[] { null, "$2a$12$IPZt7gh0Y5pxLYsFhHsWreE4zR4zOkFMHFamkNvJzxBcYpoCiuX.W" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DelegadoId",
                table: "Usuarios",
                column: "DelegadoId",
                unique: true,
                filter: "[DelegadoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Delegados_DelegadoId",
                table: "Usuarios",
                column: "DelegadoId",
                principalTable: "Delegados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Delegados_DelegadoId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_DelegadoId",
                table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Delegados",
                type: "int",
                nullable: true);

            // Restaurar: Delegados.UsuarioId = Id del Usuario que tiene DelegadoId
            migrationBuilder.Sql(
                "UPDATE Delegados SET UsuarioId = u.Id FROM Usuarios u WHERE u.DelegadoId = Delegados.Id");

            migrationBuilder.DropColumn(
                name: "DelegadoId",
                table: "Usuarios");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$QVfJTIzwGHFZq9JsYGxBseg7soPF0DuLSfxFB5A03VvSaMb1cUemS");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$urZ//SoEDQcoM38JrNkJLu2x1EJQ9jKOR2T9.6ejECUo9TBmg0HVq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$lImcMT3Hxkr5A/WxpQxqx.51z1x7V8lnyCGFShsKqZGh2mjGtJgcG");

            migrationBuilder.CreateIndex(
                name: "IX_Delegados_UsuarioId",
                table: "Delegados",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
