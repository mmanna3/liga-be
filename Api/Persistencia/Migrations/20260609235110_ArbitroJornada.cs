using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ArbitroJornada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArbitroJornada",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArbitroId = table.Column<int>(type: "int", nullable: false),
                    JornadaId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArbitroJornada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArbitroJornada_Arbitros_ArbitroId",
                        column: x => x.ArbitroId,
                        principalSchema: "dbo",
                        principalTable: "Arbitros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArbitroJornada_Jornadas_JornadaId",
                        column: x => x.JornadaId,
                        principalSchema: "dbo",
                        principalTable: "Jornadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$5GsKNOUrqoBzhqTXSdc/DO7mJLp16.HScy0f/tux1lqooLmTEV.eG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$anD5B3.rgoUC64ODoX9tw.aUVzQHqc0UQlaDk/1l.GWdm0eO0fROa");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$J/j9ysZPY.e1UZ2l1CYiR.ourHWPvGj5d0dKK4UfeQzynYKmbhQAC");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$Sa/W9OGJa4KYsndp4iUmxuRjgfgDyJxMnwNjhaSZl0ntzvttIS1O6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$zgcuj/J62Sr.qSvLHaIoN.H3KOd5G6F0yD0I1Z4HUc5GZHk5YkUDu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$87vyIeIYSVnnYJi4iyc2UuvsIQ8A260JfZFrlWUFTk7sCVk0XAQzW");

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroJornada_ArbitroId_JornadaId",
                schema: "dbo",
                table: "ArbitroJornada",
                columns: new[] { "ArbitroId", "JornadaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroJornada_JornadaId",
                schema: "dbo",
                table: "ArbitroJornada",
                column: "JornadaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArbitroJornada",
                schema: "dbo");

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
        }
    }
}
