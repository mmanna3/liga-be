using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaRolDelegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 4, "Delegado" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$hSHCz9NjNbgnIT.x7EJHAu7DDRrLORD04Q/m2e8iX5q4jObskwuXO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$3B4fsrg0MfTR73dvHD/wQeE81E1ewSkMRHNAGrTiasALCD2ARXgkW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$BGHqFLIR6jGrcOfBH4ITquXDw/jv8rBScmatUFrPYVQB.4LV3cDW2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

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
    }
}
