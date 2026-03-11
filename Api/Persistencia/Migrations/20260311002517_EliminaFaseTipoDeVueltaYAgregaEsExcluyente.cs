using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EliminaFaseTipoDeVueltaYAgregaEsExcluyente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFases__FaseTipoDeVuelta_FaseTipoDeVueltaId",
                table: "TorneoFases");

            migrationBuilder.DropTable(
                name: "_FaseTipoDeVuelta");

            migrationBuilder.DropIndex(
                name: "IX_TorneoFases_FaseTipoDeVueltaId",
                table: "TorneoFases");

            migrationBuilder.DropColumn(
                name: "FaseTipoDeVueltaId",
                table: "TorneoFases");

            migrationBuilder.AddColumn<bool>(
                name: "EsExcluyente",
                table: "TorneoFases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$KymdCy6WDkHG.ZyfKOXKZO8SWdCdECFl4ElEO7B84crwJuZDTMGhq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$lzGHHCLql6qmX0otQaVu4.I2gOowGWuaGkYIkqFOqXTywywhlLBwe");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$5zocefZyMxD7jEyvLd2EhutIep5aTrt3Wi7Ci66O6kdeARrccKCNG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsExcluyente",
                table: "TorneoFases");

            migrationBuilder.AddColumn<int>(
                name: "FaseTipoDeVueltaId",
                table: "TorneoFases",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$psIAxd8oaHn/KfAMuwZl0eJsq8.aeTaf7vA8phKuXTozz5UGSKSnW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$i3nQFBfJt.3KAH8XIPVhY.FoHu2j5blzFsYnjgSuR6SMtkmWGQqH6");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$sQ4IDCja57YApUfFIf47febds0YsVDDGs0cAsBUv8NgzQ.3TW4Jya");

            migrationBuilder.InsertData(
                table: "_FaseTipoDeVuelta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Solo ida" },
                    { 2, "Ida y vuelta" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_FaseTipoDeVueltaId",
                table: "TorneoFases",
                column: "FaseTipoDeVueltaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFases__FaseTipoDeVuelta_FaseTipoDeVueltaId",
                table: "TorneoFases",
                column: "FaseTipoDeVueltaId",
                principalTable: "_FaseTipoDeVuelta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
