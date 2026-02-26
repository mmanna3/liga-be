using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEstadoDelegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DNI",
                table: "Delegados",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EstadoDelegadoId",
                table: "Delegados",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Delegados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "EstadoDelegado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoDelegado", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EstadoDelegado",
                columns: new[] { "Id", "Estado" },
                values: new object[,]
                {
                    { 1, "Pendiente de aprobación" },
                    { 2, "Rechazado" },
                    { 3, "Activo" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Delegados_EstadoDelegadoId",
                table: "Delegados",
                column: "EstadoDelegadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegados_EstadoDelegado_EstadoDelegadoId",
                table: "Delegados",
                column: "EstadoDelegadoId",
                principalTable: "EstadoDelegado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_EstadoDelegado_EstadoDelegadoId",
                table: "Delegados");

            migrationBuilder.DropTable(
                name: "EstadoDelegado");

            migrationBuilder.DropIndex(
                name: "IX_Delegados_EstadoDelegadoId",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "DNI",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "EstadoDelegadoId",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Delegados");

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

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$wjAmPCPMAa/shrlArH.4lOEQa52G8qcG2bJNRDyBJv0UvhpskhU/a");
        }
    }
}
