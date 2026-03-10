using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EliminaTorneoIdDeEquipoYAgregaFaseZonaUnica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrar datos: para cada torneo con equipos, crear Fase única + Zona única y asignar ZonaActualId
            migrationBuilder.Sql(@"
                DECLARE @TorneoId INT;
                DECLARE @FaseId INT;
                DECLARE @ZonaId INT;

                DECLARE torneo_cursor CURSOR FOR SELECT Id FROM Torneos;
                OPEN torneo_cursor;

                FETCH NEXT FROM torneo_cursor INTO @TorneoId;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM TorneoFases WHERE TorneoId = @TorneoId AND Numero = 1)
                    BEGIN
                        INSERT INTO TorneoFases (Numero, TorneoId, FaseFormatoId, InstanciaEliminacionDirectaId, FaseTipoDeVueltaId, EstadoFaseId, EsVisibleEnApp)
                        VALUES (1, @TorneoId, 1, NULL, 1, 100, 1);
                        SET @FaseId = SCOPE_IDENTITY();

                        INSERT INTO TorneoZonas (TorneoFaseId, Nombre)
                        VALUES (@FaseId, N'Zona única');
                        SET @ZonaId = SCOPE_IDENTITY();

                        UPDATE Equipos SET ZonaActualId = @ZonaId WHERE TorneoId = @TorneoId;
                    END
                    ELSE
                    BEGIN
                        SELECT TOP 1 @ZonaId = tz.Id FROM TorneoZonas tz
                        INNER JOIN TorneoFases tf ON tz.TorneoFaseId = tf.Id
                        WHERE tf.TorneoId = @TorneoId AND tf.Numero = 1 AND tz.Nombre = N'Zona única';

                        UPDATE Equipos SET ZonaActualId = @ZonaId WHERE TorneoId = @TorneoId;
                    END

                    FETCH NEXT FROM torneo_cursor INTO @TorneoId;
                END

                CLOSE torneo_cursor;
                DEALLOCATE torneo_cursor;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_Torneos_TorneoId",
                table: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Equipo_Nombre_TorneoId",
                table: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Equipos_TorneoId",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "TorneoId",
                table: "Equipos");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$5OGoNTPwcMexYcIHwMv/cu2qUB0A7tQ9jV7LTIbjx6eS7ma2Ax7ua");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$1LA6XfVy4Ee3xj6opM0c8.QlpM5u8qtM3oeR4FIcz1ILZ4mg/EhvW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$vqC1rI8R4koJ1PCq.NUbd.bmRKWMx8wim4dPHVka0gZ2I1UaYbqee");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Nombre_ZonaActualId",
                table: "Equipos",
                columns: new[] { "Nombre", "ZonaActualId" },
                unique: true,
                filter: "[ZonaActualId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equipos_Nombre_ZonaActualId",
                table: "Equipos");

            migrationBuilder.AddColumn<int>(
                name: "TorneoId",
                table: "Equipos",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$ELlmZj3FFROmsnkzARQSjO5XIGCCckjrDowtD7NAUjoYM279VfahW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Xtf43yi52WenUiUBtaasWuuYDdYfCWNbLZVBtfYyDxMQFyg04pfRy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$ziEGftzpDCuZZgKQ4iRuJuSpAzL1SYUZItf8mw4jH2CjERkIc7/zO");

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_Nombre_TorneoId",
                table: "Equipos",
                columns: new[] { "Nombre", "TorneoId" },
                unique: true,
                filter: "[TorneoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_TorneoId",
                table: "Equipos",
                column: "TorneoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_Torneos_TorneoId",
                table: "Equipos",
                column: "TorneoId",
                principalTable: "Torneos",
                principalColumn: "Id");
        }
    }
}
