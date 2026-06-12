using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class FaseCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeyendaTablaPosiciones_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Zonas_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.CreateTable(
                name: "FaseCategorias",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnioDesde = table.Column<int>(type: "int", nullable: false),
                    AnioHasta = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaseCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaseCategorias_Fases_FaseId",
                        column: x => x.FaseId,
                        principalSchema: "dbo",
                        principalTable: "Fases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$l4c3aTHmjB8NeIwaMTcbheU8DXwrmWfXjndbr.RAj3B/enNptxx.e");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$YNxfmxF8arFkf2AWWS5p7eA5ZtqUwp4ibZ1KBDPHS851nIO1zBTcy");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$5VxWOjC1Svm/jlks55HsAegWLiEJP2lt2jZU/71sFfx0UDx4pYsC6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$KhRbnkn3A5gKqcrL8/VOX.RfJxlCS1/471tVk0R39l1Y7c4sxRGPu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$yuYcvjEoSwFgNXyeJAn6u.1hqLlBXS32VTQdqA88wx0FKx/U8HqoW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$gS3YQzEtFzkgG8mxto7Bb.xKHHzxYytcroy4AkblTmXNUmxpWQE.i");

            migrationBuilder.CreateIndex(
                name: "IX_FaseCategorias_FaseId_Orden",
                schema: "dbo",
                table: "FaseCategorias",
                columns: new[] { "FaseId", "Orden" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO [dbo].[FaseCategorias] ([Nombre], [AnioDesde], [AnioHasta], [Orden], [FaseId])
                SELECT tc.[Nombre], tc.[AnioDesde], tc.[AnioHasta], tc.[Orden], f.[Id]
                FROM [dbo].[Fases] f
                INNER JOIN [dbo].[TorneoCategorias] tc ON tc.[TorneoId] = f.[TorneoId];

                UPDATE p
                SET p.[CategoriaId] = fc.[Id]
                FROM [dbo].[Partidos] p
                INNER JOIN [dbo].[Jornadas] j ON j.[Id] = p.[JornadaId]
                INNER JOIN [dbo].[Fechas] fe ON fe.[Id] = j.[FechaId]
                INNER JOIN [dbo].[Zonas] z ON z.[Id] = fe.[ZonaId]
                INNER JOIN [dbo].[TorneoCategorias] tc ON tc.[Id] = p.[CategoriaId]
                INNER JOIN [dbo].[FaseCategorias] fc ON fc.[FaseId] = z.[FaseId]
                    AND fc.[Orden] = tc.[Orden]
                    AND fc.[Nombre] = tc.[Nombre];

                UPDATE z
                SET z.[CategoriaId] = fc.[Id]
                FROM [dbo].[Zonas] z
                INNER JOIN [dbo].[TorneoCategorias] tc ON tc.[Id] = z.[CategoriaId]
                INNER JOIN [dbo].[FaseCategorias] fc ON fc.[FaseId] = z.[FaseId]
                    AND fc.[Orden] = tc.[Orden]
                    AND fc.[Nombre] = tc.[Nombre]
                WHERE z.[TipoZona] = N'EliminacionDirecta';

                UPDATE l
                SET l.[CategoriaId] = fc.[Id]
                FROM [dbo].[LeyendaTablaPosiciones] l
                INNER JOIN [dbo].[Zonas] z ON z.[Id] = l.[ZonaId]
                INNER JOIN [dbo].[TorneoCategorias] tc ON tc.[Id] = l.[CategoriaId]
                INNER JOIN [dbo].[FaseCategorias] fc ON fc.[FaseId] = z.[FaseId]
                    AND fc.[Orden] = tc.[Orden]
                    AND fc.[Nombre] = tc.[Nombre]
                WHERE l.[CategoriaId] IS NOT NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_LeyendaTablaPosiciones_FaseCategorias_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "FaseCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_FaseCategorias_CategoriaId",
                schema: "dbo",
                table: "Partidos",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "FaseCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zonas_FaseCategorias_CategoriaId",
                schema: "dbo",
                table: "Zonas",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "FaseCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeyendaTablaPosiciones_FaseCategorias_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_FaseCategorias_CategoriaId",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Zonas_FaseCategorias_CategoriaId",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropTable(
                name: "FaseCategorias",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$dwZ74xBlZBMHd7k1wQnU0etGT6U470AmPpEzdAE9ygo8nvbHploDm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Wyf9wWYtbCaUbr7edI.7PeaYWFuowQzVezacLZiBBPX07BheECwmO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Etgqads6kGNBGxDiVs7BpezOVX2knzTCbb8eKlAYz81vsjtAAfpRm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$XpVq6T66ivihFMrvt6wg5ejqSrmKuTmx14pdnQwG5nqLT0yAXwhbK");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$nsPWq5uK7cq.QvczWNah5OhFc9rHN1DTtAYjf4TeLWHQ67EELZs4i");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$aTYrmaTln35NjAkLwsUhd.RHF/eKh3zdWyw.iwlv.5AkNEseNFu56");

            migrationBuilder.AddForeignKey(
                name: "FK_LeyendaTablaPosiciones_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "TorneoCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "Partidos",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "TorneoCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zonas_TorneoCategorias_CategoriaId",
                schema: "dbo",
                table: "Zonas",
                column: "CategoriaId",
                principalSchema: "dbo",
                principalTable: "TorneoCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
