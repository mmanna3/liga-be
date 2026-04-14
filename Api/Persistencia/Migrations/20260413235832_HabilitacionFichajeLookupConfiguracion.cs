using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class HabilitacionFichajeLookupConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_HabilitacionFichaje",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TipoHabilitacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HabilitacionFichaje", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_HabilitacionFichaje",
                columns: new[] { "Id", "TipoHabilitacion" },
                values: new object[,]
                {
                    { 1, "Habilitado" },
                    { 2, "Deshabilitado" },
                    { 3, "Programado" }
                });

            migrationBuilder.AddColumn<int>(
                name: "HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE dbo.Configuracion
                SET HabilitacionFichajeId = CASE
                    WHEN FichajeEstaHabilitado = 1 THEN 1
                    ELSE 2
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "FichajeEstaHabilitado",
                schema: "dbo",
                table: "Configuracion");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion",
                column: "HabilitacionFichajeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Configuracion__HabilitacionFichaje_HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion",
                column: "HabilitacionFichajeId",
                principalSchema: "dbo",
                principalTable: "_HabilitacionFichaje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Configuracion__HabilitacionFichaje_HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion");

            migrationBuilder.DropIndex(
                name: "IX_Configuracion_HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion");

            migrationBuilder.AddColumn<bool>(
                name: "FichajeEstaHabilitado",
                schema: "dbo",
                table: "Configuracion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE dbo.Configuracion
                SET FichajeEstaHabilitado = CASE
                    WHEN HabilitacionFichajeId = 1 THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END
                """);

            migrationBuilder.DropColumn(
                name: "HabilitacionFichajeId",
                schema: "dbo",
                table: "Configuracion");

            migrationBuilder.DropTable(
                name: "_HabilitacionFichaje",
                schema: "dbo");
        }
    }
}
