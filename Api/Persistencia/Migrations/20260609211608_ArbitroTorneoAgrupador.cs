using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ArbitroTorneoAgrupador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArbitroTorneoAgrupador",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArbitroId = table.Column<int>(type: "int", nullable: false),
                    TorneoAgrupadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArbitroTorneoAgrupador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArbitroTorneoAgrupador_Arbitros_ArbitroId",
                        column: x => x.ArbitroId,
                        principalSchema: "dbo",
                        principalTable: "Arbitros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArbitroTorneoAgrupador_TorneoAgrupadores_TorneoAgrupadorId",
                        column: x => x.TorneoAgrupadorId,
                        principalSchema: "dbo",
                        principalTable: "TorneoAgrupadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$CT2VjSO6SgKFdGS0MFxXte5LdtR8ovWvNg5h8mmTUMzWW0DGnOKpG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$YVGDNYZDIzsMcj5NxekKFuHwdYaSGEneH/J9nb0P4mG6oQ9MxrSVi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$XDnunaG0Jx5GiS1CAVXH6OCLFzJWCSHBCUZI4gcCG7eUc6WCLLio6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$9sm69kqjb8Lcfw12Abn3TeR068lBkINO.Wn9CBCnpSOfPAPzXuG7S");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$vXIK2uIvAfKmB.8MKlr/LeOAf4G/HKFjxUWntJ/1gOtAhB.fNsksq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$Bsvj/iwKUKQX6iwbvNlKGuikTemI.4FqJzSP0wgKZwGEF4cA8aKgy");

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroTorneoAgrupador_ArbitroId_TorneoAgrupadorId",
                schema: "dbo",
                table: "ArbitroTorneoAgrupador",
                columns: new[] { "ArbitroId", "TorneoAgrupadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroTorneoAgrupador_TorneoAgrupadorId",
                schema: "dbo",
                table: "ArbitroTorneoAgrupador",
                column: "TorneoAgrupadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArbitroTorneoAgrupador",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$DSylm9JD/zIo.EE9rzgZcetUB7kyI.E7sbMVDixvXD/rG1FjMq3Gm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$77TVQrpZyjsEmzNOPOXtxe2SO2wayA9RzseUI/ZEAmhTtsvjVxH3S");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$sK/fBFtoPtXLcy5KjtqWoeo2vxwkLSjkNVVFlirYp05V7oWIM0sKa");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$dSBSgbktoM3LNFJPVsQE0e7NqeCDfcda8ILFtgBTCIEMY0A46hx4e");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$6eAYsvOzbiuAyVnpnZQrn.6lvHlo9Ulv2HCPaKkOT30AHaqYGgcoG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$KtQUiD2JZbQ7TBDQnH/v3.BM6eOgAtKXr2mh9BTtO7AgSuGmw69oC");
        }
    }
}
