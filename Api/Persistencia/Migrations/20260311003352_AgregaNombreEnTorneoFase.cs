using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaNombreEnTorneoFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "TorneoFases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$svOlKAf5TcDFSCGLxdMj5eE4iXjabjBL7du4VezmzFNNPEiFZ7ybW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$8FRaWJV.muw2BiUm3p.vge96QWgXZp8egSdoxZ2eGbT4.HiIBhTxC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Y5J84.HmhCcySD1pn6EunueCtczQFbB8jWmHxTlNhVLR2fJEOWSxO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "TorneoFases");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$KymdCy6WDkHG.ZyfKOXKZO8SWdCdECFl4ElEO7B84crwJuZDTMGhq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$lzGHHCLql6qmX0otQaVu4.I2gOowGWuaGkYIkqFOqXTywywhlLBwe");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$5zocefZyMxD7jEyvLd2EhutIep5aTrt3Wi7Ci66O6kdeARrccKCNG");
        }
    }
}
