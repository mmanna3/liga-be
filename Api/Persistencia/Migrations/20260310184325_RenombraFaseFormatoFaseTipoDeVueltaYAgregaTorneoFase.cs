using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombraFaseFormatoFaseTipoDeVueltaYAgregaTorneoFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_FormatoDeLaFase");

            migrationBuilder.DropTable(
                name: "_TipoVueltaDeLaFase");

            migrationBuilder.CreateTable(
                name: "_FaseFormato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FaseFormato", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_FaseTipoDeVuelta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FaseTipoDeVuelta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TorneoFases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    TorneoId = table.Column<int>(type: "int", nullable: false),
                    FaseFormatoId = table.Column<int>(type: "int", nullable: false),
                    InstanciaEliminacionDirectaId = table.Column<int>(type: "int", nullable: true),
                    FaseTipoDeVueltaId = table.Column<int>(type: "int", nullable: false),
                    EstadoFaseId = table.Column<int>(type: "int", nullable: false),
                    EsVisibleEnApp = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneoFases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorneoFases_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorneoFases__EstadoFase_EstadoFaseId",
                        column: x => x.EstadoFaseId,
                        principalTable: "_EstadoFase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorneoFases__FaseFormato_FaseFormatoId",
                        column: x => x.FaseFormatoId,
                        principalTable: "_FaseFormato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorneoFases__FaseTipoDeVuelta_FaseTipoDeVueltaId",
                        column: x => x.FaseTipoDeVueltaId,
                        principalTable: "_FaseTipoDeVuelta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorneoFases__InstanciaEliminacionDirecta_InstanciaEliminacionDirectaId",
                        column: x => x.InstanciaEliminacionDirectaId,
                        principalTable: "_InstanciaEliminacionDirecta",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$82oocSSw89JfE.e7v3YCOeFJjZEDgWmA/LD9Ckagi3uEe3R0xG5fy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$mo7htk0Aa3iO3OuiZm/rKux6dB7kjaf1c/fmjAiJS7ZeGNtfyLY1W");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$XPeMJkoncFrX0QvN1UtjrurQ62Ds8Ortjbl5l7BgNopWR2ZgDQaHm");

            migrationBuilder.InsertData(
                table: "_FaseFormato",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Todos contra todos" },
                    { 2, "Eliminación directa" }
                });

            migrationBuilder.InsertData(
                table: "_FaseTipoDeVuelta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Solo ida" },
                    { 2, "Ida y vuelta" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_EstadoFaseId",
                table: "TorneoFases",
                column: "EstadoFaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_FaseFormatoId",
                table: "TorneoFases",
                column: "FaseFormatoId");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_FaseTipoDeVueltaId",
                table: "TorneoFases",
                column: "FaseTipoDeVueltaId");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_InstanciaEliminacionDirectaId",
                table: "TorneoFases",
                column: "InstanciaEliminacionDirectaId");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_TorneoId_Numero",
                table: "TorneoFases",
                columns: new[] { "TorneoId", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TorneoFases");

            migrationBuilder.DropTable(
                name: "_FaseFormato");

            migrationBuilder.DropTable(
                name: "_FaseTipoDeVuelta");

            migrationBuilder.CreateTable(
                name: "_FormatoDeLaFase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FormatoDeLaFase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_TipoVueltaDeLaFase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoVueltaDeLaFase", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$hl5mtKHu3Z2g4WTdzR3CHugEQe7FcHWueXkfOLodLrblH49XAD2tu");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$9mlY9JSDrXLhIRieITJu6uJ7y2Hk3MNrnp9f2ZIUj9n0GBw6iVBbK");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$vJoMokueYhOCxmDORzTY1uxk8NNfrgNGPEVZR750ddYFQKXoj7mS.");

            migrationBuilder.InsertData(
                table: "_FormatoDeLaFase",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Todos contra todos" },
                    { 2, "Eliminación directa" }
                });

            migrationBuilder.InsertData(
                table: "_TipoVueltaDeLaFase",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Solo ida" },
                    { 2, "Ida y vuelta" }
                });
        }
    }
}
