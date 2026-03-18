using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaLocalVisitante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_LocalVisitante",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LocalVisitante", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$lS5hpvCW24f0ack48cn78uYFOs1W9o4W9/otDs3ZA1jPcVYafEIha");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$tDrzfWVg1rjpYvDKbdUhduWixmU5mgJUWN11E80qCVXsikRel5e0i");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$L8.WDx.tke1iec6ZKEr/s.R.bZjcQdOMIxVZjMpz0LKniRi8D9y1i");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_LocalVisitante",
                columns: new[] { "Id", "Estado" },
                values: new object[,]
                {
                    { 1, "Local" },
                    { 2, "Visitante" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_LocalVisitante",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$fOCp/SJkXncZ8VYfZiU1WuMJb/5PXDHWgb.qeSmPk3ZsFbnKQMsCO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$MVz9iJiR9NVbOWSqAdpSGOe1aUSaQ72rHqdxY2ddf3MYeq2a127pu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$WztehcpYBYYKty0F7JzuS.NtRgbYdb/i/GHws0vvTVQwPx0HQ0TpO");
        }
    }
}
