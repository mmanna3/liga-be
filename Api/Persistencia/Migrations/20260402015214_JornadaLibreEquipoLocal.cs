using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class JornadaLibreEquipoLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jornadas_Equipos_JornadaLibre_EquipoId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.RenameColumn(
                name: "JornadaLibre_EquipoId",
                schema: "dbo",
                table: "Jornadas",
                newName: "EquipoLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_Jornadas_JornadaLibre_EquipoId",
                schema: "dbo",
                table: "Jornadas",
                newName: "IX_Jornadas_EquipoLocalId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas",
                sql: "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [EquipoLocalId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [EquipoLocalId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoLocalId] IS NULL)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jornadas_Equipos_EquipoLocalId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.RenameColumn(
                name: "EquipoLocalId",
                schema: "dbo",
                table: "Jornadas",
                newName: "JornadaLibre_EquipoId");

            migrationBuilder.RenameIndex(
                name: "IX_Jornadas_EquipoLocalId",
                schema: "dbo",
                table: "Jornadas",
                newName: "IX_Jornadas_JornadaLibre_EquipoId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas",
                sql: "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [JornadaLibre_EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [JornadaLibre_EquipoId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [JornadaLibre_EquipoId] IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Jornadas_Equipos_JornadaLibre_EquipoId",
                schema: "dbo",
                table: "Jornadas",
                column: "JornadaLibre_EquipoId",
                principalSchema: "dbo",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
