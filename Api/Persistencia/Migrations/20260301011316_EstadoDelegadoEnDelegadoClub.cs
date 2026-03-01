using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EstadoDelegadoEnDelegadoClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegadoClub_Clubs_ClubId",
                table: "DelegadoClub");

            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_EstadoDelegado_EstadoDelegadoId",
                table: "Delegados");

            // Agregar columna nullable para poder migrar datos
            migrationBuilder.AddColumn<int>(
                name: "EstadoDelegadoId",
                table: "DelegadoClub",
                type: "int",
                nullable: true);

            // Migrar EstadoDelegadoId de Delegados a cada DelegadoClub
            migrationBuilder.Sql(
                "UPDATE dc SET dc.EstadoDelegadoId = d.EstadoDelegadoId FROM DelegadoClub dc INNER JOIN Delegados d ON dc.DelegadoId = d.Id");

            // Valor por defecto para filas sin datos (no debería haber)
            migrationBuilder.Sql(
                "UPDATE DelegadoClub SET EstadoDelegadoId = 3 WHERE EstadoDelegadoId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "EstadoDelegadoId",
                table: "DelegadoClub",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Eliminar columna y FK de Delegados
            migrationBuilder.DropIndex(
                name: "IX_Delegados_EstadoDelegadoId",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "EstadoDelegadoId",
                table: "Delegados");

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
                name: "IX_DelegadoClub_EstadoDelegadoId",
                table: "DelegadoClub",
                column: "EstadoDelegadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DelegadoClub_Clubs_ClubId",
                table: "DelegadoClub",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DelegadoClub_EstadoDelegado_EstadoDelegadoId",
                table: "DelegadoClub",
                column: "EstadoDelegadoId",
                principalTable: "EstadoDelegado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegadoClub_Clubs_ClubId",
                table: "DelegadoClub");

            migrationBuilder.DropForeignKey(
                name: "FK_DelegadoClub_EstadoDelegado_EstadoDelegadoId",
                table: "DelegadoClub");

            migrationBuilder.DropIndex(
                name: "IX_DelegadoClub_EstadoDelegadoId",
                table: "DelegadoClub");

            // Restaurar columna en Delegados
            migrationBuilder.AddColumn<int>(
                name: "EstadoDelegadoId",
                table: "Delegados",
                type: "int",
                nullable: false,
                defaultValue: 3);

            // Copiar estado desde DelegadoClub (tomar el primero si hay varios)
            migrationBuilder.Sql(
                "UPDATE Delegados SET EstadoDelegadoId = (SELECT TOP 1 EstadoDelegadoId FROM DelegadoClub WHERE DelegadoId = Delegados.Id)");

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

            migrationBuilder.DropColumn(
                name: "EstadoDelegadoId",
                table: "DelegadoClub");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$QB9FjCGu2zYdZFBCmB/16ulLqA2pId0VvxJ0VL5HN/Yiq9H/s/gJG");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$STQqmm8mykCfe.Zwz1rFiOanM6zGajTUaF.Qc.l6zhY3BBKAXTYBy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$VfvDEk1W8Moj5nCmltl/xe/pRok4vSc1mekwuXq0LJFNngXy4m9HG");

            migrationBuilder.AddForeignKey(
                name: "FK_DelegadoClub_Clubs_ClubId",
                table: "DelegadoClub",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
