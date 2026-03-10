using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTablasCatalogoConGuionBajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegadoClub_EstadoDelegado_EstadoDelegadoId",
                table: "DelegadoClub");

            migrationBuilder.DropForeignKey(
                name: "FK_JugadorEquipo_EstadoJugador_EstadoJugadorId",
                table: "JugadorEquipo");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstadoJugador",
                table: "EstadoJugador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstadoDelegado",
                table: "EstadoDelegado");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "_Rol");

            migrationBuilder.RenameTable(
                name: "EstadoJugador",
                newName: "_EstadoJugador");

            migrationBuilder.RenameTable(
                name: "EstadoDelegado",
                newName: "_EstadoDelegado");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Rol",
                table: "_Rol",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__EstadoJugador",
                table: "_EstadoJugador",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__EstadoDelegado",
                table: "_EstadoDelegado",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$9d2T29w2t.BW.CaI.HbdYueK1Fk2wYKzD.Tw4cVZby2BI3j6P8LIC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$ppkD5/fav5A4jbTifYXc5OAUZHLuUGKCWAn9dnwNrB07BE5R0Op1u");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$SQWdYq3dUKJLP3FhN0JiJucMkYI37x3f4DER4mUhyFYejwgt/9zXa");

            migrationBuilder.AddForeignKey(
                name: "FK_DelegadoClub__EstadoDelegado_EstadoDelegadoId",
                table: "DelegadoClub",
                column: "EstadoDelegadoId",
                principalTable: "_EstadoDelegado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JugadorEquipo__EstadoJugador_EstadoJugadorId",
                table: "JugadorEquipo",
                column: "EstadoJugadorId",
                principalTable: "_EstadoJugador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios__Rol_RolId",
                table: "Usuarios",
                column: "RolId",
                principalTable: "_Rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegadoClub__EstadoDelegado_EstadoDelegadoId",
                table: "DelegadoClub");

            migrationBuilder.DropForeignKey(
                name: "FK_JugadorEquipo__EstadoJugador_EstadoJugadorId",
                table: "JugadorEquipo");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios__Rol_RolId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Rol",
                table: "_Rol");

            migrationBuilder.DropPrimaryKey(
                name: "PK__EstadoJugador",
                table: "_EstadoJugador");

            migrationBuilder.DropPrimaryKey(
                name: "PK__EstadoDelegado",
                table: "_EstadoDelegado");

            migrationBuilder.RenameTable(
                name: "_Rol",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "_EstadoJugador",
                newName: "EstadoJugador");

            migrationBuilder.RenameTable(
                name: "_EstadoDelegado",
                newName: "EstadoDelegado");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstadoJugador",
                table: "EstadoJugador",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstadoDelegado",
                table: "EstadoDelegado",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$SH8SCCgHNyPt3ge2fp8TVufeiVHnQskMzfOcN7LVqNs0qk90ruai2");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$2csXhRtRTIyjBxZulqTz5u0Wc743AAV0jzH9riis2RtHvBnJJQnqy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$B2D9oGMFiqudjXVubAGWtui6P.R633Ynfswy0sEEapQdL0crqokuW");

            migrationBuilder.AddForeignKey(
                name: "FK_DelegadoClub_EstadoDelegado_EstadoDelegadoId",
                table: "DelegadoClub",
                column: "EstadoDelegadoId",
                principalTable: "EstadoDelegado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JugadorEquipo_EstadoJugador_EstadoJugadorId",
                table: "JugadorEquipo",
                column: "EstadoJugadorId",
                principalTable: "EstadoJugador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
