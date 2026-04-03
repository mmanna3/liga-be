using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class JornadaSinEquipos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas",
                sql: "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [EquipoLocalId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [EquipoLocalId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoLocalId] IS NULL)\n    OR\n    ([Tipo] = N'SinEquipos' AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [EquipoLocalId] IS NULL AND [LocalOVisitanteId] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jornada_Tipo_Valido",
                schema: "dbo",
                table: "Jornadas",
                sql: "([Tipo] = N'Normal' AND [LocalEquipoId] IS NOT NULL AND [VisitanteEquipoId] IS NOT NULL AND [LocalEquipoId] <> [VisitanteEquipoId] AND [EquipoId] IS NULL AND [EquipoLocalId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Libre' AND [EquipoLocalId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoId] IS NULL AND [LocalOVisitanteId] IS NULL)\n    OR\n    ([Tipo] = N'Interzonal' AND [EquipoId] IS NOT NULL AND [LocalOVisitanteId] IS NOT NULL AND [LocalEquipoId] IS NULL AND [VisitanteEquipoId] IS NULL AND [EquipoLocalId] IS NULL)");
        }
    }
}
