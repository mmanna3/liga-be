using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ArbitroEquipoProhibido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArbitroEquipoProhibido",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArbitroId = table.Column<int>(type: "int", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArbitroEquipoProhibido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArbitroEquipoProhibido_Arbitros_ArbitroId",
                        column: x => x.ArbitroId,
                        principalSchema: "dbo",
                        principalTable: "Arbitros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArbitroEquipoProhibido_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalSchema: "dbo",
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroEquipoProhibido_ArbitroId_EquipoId",
                schema: "dbo",
                table: "ArbitroEquipoProhibido",
                columns: new[] { "ArbitroId", "EquipoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroEquipoProhibido_EquipoId",
                schema: "dbo",
                table: "ArbitroEquipoProhibido",
                column: "EquipoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArbitroEquipoProhibido",
                schema: "dbo");
        }
    }
}
