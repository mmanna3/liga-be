using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaDatosAlClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Clubs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Clubs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsTechado",
                table: "Clubs",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                table: "Clubs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$SH8SCCgHNyPt3ge2fp8TVufeiVHnQskMzfOcN7LVqNs0qk90ruai2");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$2csXhRtRTIyjBxZulqTz5u0Wc743AAV0jzH9riis2RtHvBnJJQnqy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$B2D9oGMFiqudjXVubAGWtui6P.R633Ynfswy0sEEapQdL0crqokuW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "EsTechado",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "Localidad",
                table: "Clubs");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Ur/a7aStH/qkw6QDyWVQsOjXa94.olzgDPnXy4qLCBvDAC6P0IDUC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$SGi.IWRHc95SQWGdhxplr.hoS5CEbBYKxDA0ulS5erqlMe7TnHtrK");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$IPZt7gh0Y5pxLYsFhHsWreE4zR4zOkFMHFamkNvJzxBcYpoCiuX.W");
        }
    }
}
