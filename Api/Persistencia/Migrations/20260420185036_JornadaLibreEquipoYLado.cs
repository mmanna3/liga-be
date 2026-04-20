using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class JornadaLibreEquipoYLado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jornadas_Equipos_EquipoLocalId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Jornadas]
                SET [EquipoId] = [EquipoLocalId],
                    [LocalOVisitanteId] = 1
                WHERE [Tipo] = N'Libre';
                """);

            migrationBuilder.DropIndex(
                name: "IX_Jornadas_EquipoLocalId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropColumn(
                name: "EquipoLocalId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas",
                sql: "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL)\n    OR\n    ([Tipo] = N'SinEquipos' AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.AddColumn<int>(
                name: "EquipoLocalId",
                schema: "dbo",
                table: "Jornadas",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Jornadas]
                SET [EquipoLocalId] = [EquipoId],
                    [EquipoId] = NULL,
                    [LocalOVisitanteId] = NULL
                WHERE [Tipo] = N'Libre';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_EquipoLocalId",
                schema: "dbo",
                table: "Jornadas",
                column: "EquipoLocalId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas",
                sql: "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [EquipoLocalId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [EquipoLocalId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoLocalId] IS NULL)\n    OR\n    ([Tipo] = N'SinEquipos' AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [EquipoLocalId] IS NULL AND [LocalOVisitanteId] IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Jornadas_Equipos_EquipoLocalId",
                schema: "dbo",
                table: "Jornadas",
                column: "EquipoLocalId",
                principalSchema: "dbo",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
