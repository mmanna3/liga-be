using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class CreaTorneoCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TorneoCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnioDesde = table.Column<int>(type: "int", nullable: false),
                    AnioHasta = table.Column<int>(type: "int", nullable: false),
                    TorneoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneoCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorneoCategorias_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$KSdlepA.vs.xkCc6P/F4o.MQGjI.waawgSx2iwkGZBtsvS0tE..KW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$NP0ZI3s.Z9z0S/6cCdGXteTuBlbkgdEJgZkkmQukuSh5Qeh6wjiC2");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$WatEPott.gQ7IDnjH4IHVuJVumZlQ3iEQXsA8zOp6.vY3Vpde/niy");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoCategorias_TorneoId",
                table: "TorneoCategorias",
                column: "TorneoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TorneoCategorias");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$tNKT2hqYy2Cn.3CUS1ztluzl70Tb3cSR3wXQSBbCckQtdon2jYToO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Gtz/.FrpKqEkyHGPC17xfer2fDia1McKuMZGhoT.dHh.gLGKtwbyS");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$tP2vED12CXZzcOF10gAgXuCGkCroNpFrZcSRru5IaMFlUP6i0c0rW");
        }
    }
}
