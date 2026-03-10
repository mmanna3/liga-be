using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaAnioEnTorneo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Anio",
                table: "Torneos",
                type: "int",
                nullable: false,
                defaultValue: 2026);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Cwjj./OvbfGsnoa1BG6VDe9MVLcrCw6VIBA9B6fuNRpPHfPI2JNZC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$J8nZvQWhhXN00S/q66jYmuHnsWvEyalBpsqjUajuMCk7Aazb0oQq.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$yzdDhZE3kx8aAzm9yBxhOuRxQLx1x.LnW4/sDnHtvxxD5lNQeBCrW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anio",
                table: "Torneos");

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
        }
    }
}
