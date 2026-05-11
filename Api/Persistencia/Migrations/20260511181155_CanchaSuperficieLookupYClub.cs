using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class CanchaSuperficieLookupYClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsTechado",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.AddColumn<int>(
                name: "CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "_CanchaSuperficie",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Superficie = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CanchaSuperficie", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$WtH0t9WLj3CVWUdRdeQTBeYjxWqHHLPXMpJJJlrh5CdUfHW92ffG2");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$gUYUTtr9OYvBhCEi/8ysV.JbTLzquDdeISbtcZUFcuyWfsvOIgMBO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$lFf9g50zbvqX600P4ixT0uBAqRN4J3wcQs/RKWRMXVYAgBMCa4HbW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$qTwKHUMI/2gjEWqaHuBtO.9zatYGx2NZ1Sl06T44QfUoH3uQTGvD6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$a6N9xSInKfIzWLIydD4SeOwddTkC3q2A.qNC.dBQOyMS8UOkc1gI.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$LLQR6reDZ0umRvb1mL1shO1OWNnSaA8t0O6M0diGH1JsgihKHb4Tu");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_CanchaSuperficie",
                columns: new[] { "Id", "Superficie" },
                values: new object[,]
                {
                    { 1, "PastoNatural" },
                    { 2, "PastoSintetico" },
                    { 3, "Consultar" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs",
                column: "CanchaSuperficieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs__CanchaSuperficie_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs",
                column: "CanchaSuperficieId",
                principalSchema: "dbo",
                principalTable: "_CanchaSuperficie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs__CanchaSuperficie_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropTable(
                name: "_CanchaSuperficie",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.AddColumn<bool>(
                name: "EsTechado",
                schema: "dbo",
                table: "Clubs",
                type: "bit",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$63nk8pCcvGZVb7056KbqXO0FF4uLBHbtyhcCl2gSsuFpyNqMvV4HG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$yW.2qvd3A2avGrz11cX4H.BPcBGeZj.sigl0PLBa9y0HCTUCIvBLq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$rEwvLog6vBILx/19vBnUGOM7NgikloGicxyS7qEqe/roDegT1ZQhi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$CyE4nlHXLRAchq5BvsqDQ.fRQjYDz9ktrMZok1ugdZdjsialkaxgG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$blFyMSphnmzAUB41jj6wAeBn0guL/4yZ/mifUfYzJCz7p2a0QknjS");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$Gm3xgnh7SOlNodUWvefKvuGk8A0szw1T.hqhr3StKmbU8Md9EmL/O");
        }
    }
}
