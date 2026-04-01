using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoFechaTphConDiscriminador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropIndex(
                name: "IX_TorneoFechas_ZonaId_Numero",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.AlterColumn<int>(
                name: "Numero",
                schema: "dbo",
                table: "TorneoFechas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "TipoFecha",
                schema: "dbo",
                table: "TorneoFechas",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "TodosContraTodos");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFechas_ZonaId_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas",
                columns: new[] { "ZonaId", "InstanciaEliminacionDirectaId" },
                unique: true,
                filter: "[InstanciaEliminacionDirectaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFechas_ZonaId_Numero",
                schema: "dbo",
                table: "TorneoFechas",
                columns: new[] { "ZonaId", "Numero" },
                unique: true,
                filter: "[Numero] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropIndex(
                name: "IX_TorneoFechas_ZonaId_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropIndex(
                name: "IX_TorneoFechas_ZonaId_Numero",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropColumn(
                name: "TipoFecha",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.AlterColumn<int>(
                name: "Numero",
                schema: "dbo",
                table: "TorneoFechas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFechas_ZonaId_Numero",
                schema: "dbo",
                table: "TorneoFechas",
                columns: new[] { "ZonaId", "Numero" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id");
        }
    }
}
