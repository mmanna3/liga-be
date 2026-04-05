using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ColorLookupTorneoAgrupador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                schema: "dbo",
                table: "TorneoAgrupadores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "_Color",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Color", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "TorneoAgrupadores",
                keyColumn: "Id",
                keyValue: 1,
                column: "ColorId",
                value: 0);

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_Color",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 0, "Negro" },
                    { 1, "Azul" },
                    { 2, "Rojo" },
                    { 3, "Verde" },
                    { 4, "Amarillo" },
                    { 5, "Naranja" },
                    { 6, "Violeta" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TorneoAgrupadores_ColorId",
                schema: "dbo",
                table: "TorneoAgrupadores",
                column: "ColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoAgrupadores__Color_ColorId",
                schema: "dbo",
                table: "TorneoAgrupadores",
                column: "ColorId",
                principalSchema: "dbo",
                principalTable: "_Color",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TorneoAgrupadores__Color_ColorId",
                schema: "dbo",
                table: "TorneoAgrupadores");

            migrationBuilder.DropTable(
                name: "_Color",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_TorneoAgrupadores_ColorId",
                schema: "dbo",
                table: "TorneoAgrupadores");

            migrationBuilder.DropColumn(
                name: "ColorId",
                schema: "dbo",
                table: "TorneoAgrupadores");
        }
    }
}
