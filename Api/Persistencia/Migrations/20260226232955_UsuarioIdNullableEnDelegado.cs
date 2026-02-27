using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioIdNullableEnDelegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Delegados",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$cDxNAMhKOTqruLA5aBw7xeZQNiF/.DdEmvT6WoKrK3/AzlIdmaCqC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$fRu06.F9ooqrgzKY.sQ32uGXsmrZp8s10qtF5MU1OMXx4qJ.6Iv0W");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$tvFKYNAdjDFn.jKTHBcHDOL3tY8V4mRkFBcuemVDyjluHBM8Xq4we");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Delegados",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$4DdqZ/FSKg7Zx1BiVABhYe6/iFQnXDGnsbQreVWKln5UeUbYyMmp2");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$YBZzDtKZRCSwkAIihdYD9./g1UWpYcnxlbZdQxqoYguQnyYR20/V2");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$sE./k3e7SmI/jZ2y2w8nV.HHMUhChLak5eFoH52BEVVGciy.P7w8e");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegados_Usuarios_UsuarioId",
                table: "Delegados",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
