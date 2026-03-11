using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class NombreRequeridoEnTorneoFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$L0Sv132jFICqArEVRPSL5uyJkdiafVTk0ZPebTxoB12Soi8GpAssG");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$MqwB98/5Djt1UvjQPuqs2uGi74cM4ZGUzIKIBNYZRhQZyX/Em9miC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$UM5xKG0RG2UbSu8r1eoas.UHEktVY7k/uYwPW8Mtl6HpxtfjIFAIu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$1SGg97hNSSj8aTzvQcJzlOwD/4fo/tZehVDj1ZIEFHoUS0Mk3bPnO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$kxaV0IJz7.P78B/zHe4uR.csT0Db/j16EtTukXbqdfOIDvnVIIBdq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$CbhGhBcoSj.6TYyKUiLFpeUaM0IrpGV09pORti4QzEe0jVazp94wC");
        }
    }
}
