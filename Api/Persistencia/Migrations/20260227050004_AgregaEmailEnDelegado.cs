using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEmailEnDelegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Delegados",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$ajvcshOF22ogZLqMYtcHNeyzo5yUJNu5CrKH.1hDLC3ij6ZWpvjX6");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$oO2P8.wOCZjT.yVShd1rxulG1AJyllFJT1qn3oFx6VAckahAB7rIy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$UghhqG.nSXkbOziu.X62wuX9pMhqSPUAqhNw3KvyPBcAQBO4zXHLG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Delegados");

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
    }
}
