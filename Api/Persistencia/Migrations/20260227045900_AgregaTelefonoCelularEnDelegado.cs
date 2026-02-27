using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTelefonoCelularEnDelegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelefonoCelular",
                table: "Delegados",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$2hehmBciBV4Tnu5wi.fgXOhOqNHtVjFJWDJGDg9xAeA2UQa48Ip6y");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$5Tbva3oE/nWo5J.8hOZeres6a6mS1GxBiV7R0WU70PBuYBKL.mQ3W");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$LroM9cU5.nsg2jZVge05teQ3rIBModG9xme.p.G0R/IEkIg6zMUbG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelefonoCelular",
                table: "Delegados");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$cDxNAMhKOTqruLA5aBw7xeZQNiF/.DdEmvT6WoKrK3/AzlIdmaCqC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$fRu06.F9ooqrgzKY.sQ32uGXsmrZp8s10qtF5MU1OMXx4qJ.6Iv0W");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$tvFKYNAdjDFn.jKTHBcHDOL3tY8V4mRkFBcuemVDyjluHBM8Xq4we");
        }
    }
}
