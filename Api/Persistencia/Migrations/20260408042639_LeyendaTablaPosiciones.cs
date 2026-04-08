using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class LeyendaTablaPosiciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeyendaTablaPosiciones",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Leyenda = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: true),
                    ZonaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeyendaTablaPosiciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeyendaTablaPosiciones_TorneoCategorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "dbo",
                        principalTable: "TorneoCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeyendaTablaPosiciones_Zonas_ZonaId",
                        column: x => x.ZonaId,
                        principalSchema: "dbo",
                        principalTable: "Zonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                column: "ZonaId",
                unique: true,
                filter: "[CategoriaId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeyendaTablaPosiciones_ZonaId_CategoriaId",
                schema: "dbo",
                table: "LeyendaTablaPosiciones",
                columns: new[] { "ZonaId", "CategoriaId" },
                unique: true,
                filter: "[CategoriaId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeyendaTablaPosiciones",
                schema: "dbo");
        }
    }
}
