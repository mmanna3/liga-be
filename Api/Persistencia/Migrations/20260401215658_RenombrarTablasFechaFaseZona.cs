using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTablasFechaFaseZona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipoZona_TorneoZonas_ZonaId",
                schema: "dbo",
                table: "EquipoZona");

            migrationBuilder.DropForeignKey(
                name: "FK_Jornadas_TorneoFechas_FechaId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFases_Torneos_TorneoId",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFases__EstadoFase_EstadoFaseId",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFechas_TorneoZonas_ZonaId",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoZonas_TorneoFases_TorneoFaseId",
                schema: "dbo",
                table: "TorneoZonas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TorneoZonas",
                schema: "dbo",
                table: "TorneoZonas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TorneoFechas",
                schema: "dbo",
                table: "TorneoFechas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TorneoFases",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.RenameTable(
                name: "TorneoZonas",
                schema: "dbo",
                newName: "Zonas",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TorneoFechas",
                schema: "dbo",
                newName: "Fechas",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TorneoFases",
                schema: "dbo",
                newName: "Fases",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "TorneoFaseId",
                schema: "dbo",
                table: "Zonas",
                newName: "FaseId");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoZonas_TorneoFaseId",
                schema: "dbo",
                table: "Zonas",
                newName: "IX_Zonas_FaseId");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoFechas_ZonaId_Numero",
                schema: "dbo",
                table: "Fechas",
                newName: "IX_Fechas_ZonaId_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoFechas_ZonaId_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                newName: "IX_Fechas_ZonaId_InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoFechas_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                newName: "IX_Fechas_InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoFases_TorneoId_Numero",
                schema: "dbo",
                table: "Fases",
                newName: "IX_Fases_TorneoId_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoFases_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases",
                newName: "IX_Fases_InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_TorneoFases_EstadoFaseId",
                schema: "dbo",
                table: "Fases",
                newName: "IX_Fases_EstadoFaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zonas",
                schema: "dbo",
                table: "Zonas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fechas",
                schema: "dbo",
                table: "Fechas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fases",
                schema: "dbo",
                table: "Fases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipoZona_Zonas_ZonaId",
                schema: "dbo",
                table: "EquipoZona",
                column: "ZonaId",
                principalSchema: "dbo",
                principalTable: "Zonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fases_Torneos_TorneoId",
                schema: "dbo",
                table: "Fases",
                column: "TorneoId",
                principalSchema: "dbo",
                principalTable: "Torneos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fases__EstadoFase_EstadoFaseId",
                schema: "dbo",
                table: "Fases",
                column: "EstadoFaseId",
                principalSchema: "dbo",
                principalTable: "_EstadoFase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Fechas_Zonas_ZonaId",
                schema: "dbo",
                table: "Fechas",
                column: "ZonaId",
                principalSchema: "dbo",
                principalTable: "Zonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jornadas_Fechas_FechaId",
                schema: "dbo",
                table: "Jornadas",
                column: "FechaId",
                principalSchema: "dbo",
                principalTable: "Fechas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zonas_Fases_FaseId",
                schema: "dbo",
                table: "Zonas",
                column: "FaseId",
                principalSchema: "dbo",
                principalTable: "Fases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipoZona_Zonas_ZonaId",
                schema: "dbo",
                table: "EquipoZona");

            migrationBuilder.DropForeignKey(
                name: "FK_Fases_Torneos_TorneoId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropForeignKey(
                name: "FK_Fases__EstadoFase_EstadoFaseId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropForeignKey(
                name: "FK_Fases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropForeignKey(
                name: "FK_Fechas_Zonas_ZonaId",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.DropForeignKey(
                name: "FK_Fechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.DropForeignKey(
                name: "FK_Jornadas_Fechas_FechaId",
                schema: "dbo",
                table: "Jornadas");

            migrationBuilder.DropForeignKey(
                name: "FK_Zonas_Fases_FaseId",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zonas",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fechas",
                schema: "dbo",
                table: "Fechas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fases",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.RenameTable(
                name: "Zonas",
                schema: "dbo",
                newName: "TorneoZonas",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Fechas",
                schema: "dbo",
                newName: "TorneoFechas",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Fases",
                schema: "dbo",
                newName: "TorneoFases",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "FaseId",
                schema: "dbo",
                table: "TorneoZonas",
                newName: "TorneoFaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Zonas_FaseId",
                schema: "dbo",
                table: "TorneoZonas",
                newName: "IX_TorneoZonas_TorneoFaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Fechas_ZonaId_Numero",
                schema: "dbo",
                table: "TorneoFechas",
                newName: "IX_TorneoFechas_ZonaId_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_Fechas_ZonaId_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas",
                newName: "IX_TorneoFechas_ZonaId_InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_Fechas_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas",
                newName: "IX_TorneoFechas_InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_Fases_TorneoId_Numero",
                schema: "dbo",
                table: "TorneoFases",
                newName: "IX_TorneoFases_TorneoId_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_Fases_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFases",
                newName: "IX_TorneoFases_InstanciaEliminacionDirectaId");

            migrationBuilder.RenameIndex(
                name: "IX_Fases_EstadoFaseId",
                schema: "dbo",
                table: "TorneoFases",
                newName: "IX_TorneoFases_EstadoFaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TorneoZonas",
                schema: "dbo",
                table: "TorneoZonas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TorneoFechas",
                schema: "dbo",
                table: "TorneoFechas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TorneoFases",
                schema: "dbo",
                table: "TorneoFases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipoZona_TorneoZonas_ZonaId",
                schema: "dbo",
                table: "EquipoZona",
                column: "ZonaId",
                principalSchema: "dbo",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jornadas_TorneoFechas_FechaId",
                schema: "dbo",
                table: "Jornadas",
                column: "FechaId",
                principalSchema: "dbo",
                principalTable: "TorneoFechas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFases_Torneos_TorneoId",
                schema: "dbo",
                table: "TorneoFases",
                column: "TorneoId",
                principalSchema: "dbo",
                principalTable: "Torneos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFases__EstadoFase_EstadoFaseId",
                schema: "dbo",
                table: "TorneoFases",
                column: "EstadoFaseId",
                principalSchema: "dbo",
                principalTable: "_EstadoFase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFases",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFechas_TorneoZonas_ZonaId",
                schema: "dbo",
                table: "TorneoFechas",
                column: "ZonaId",
                principalSchema: "dbo",
                principalTable: "TorneoZonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFechas__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                schema: "dbo",
                table: "TorneoFechas",
                column: "InstanciaEliminacionDirectaId",
                principalSchema: "dbo",
                principalTable: "_InstanciaEliminacionDirecta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoZonas_TorneoFases_TorneoFaseId",
                schema: "dbo",
                table: "TorneoZonas",
                column: "TorneoFaseId",
                principalSchema: "dbo",
                principalTable: "TorneoFases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
