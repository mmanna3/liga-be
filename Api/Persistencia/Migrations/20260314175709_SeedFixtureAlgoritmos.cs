using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedFixtureAlgoritmos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                columns: new[] { "Id", "CantidadDeEquipos" },
                values: new object[,]
                {
                    { 1, 4 },
                    { 2, 8 },
                    { 3, 10 },
                    { 4, 12 },
                    { 5, 14 },
                    { 6, 16 }
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$MNom3K7gxNWhMir55Gr2heSuaqoHsJpajHgth59YsGafXi47w/e2W");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Cp4bwQa5FkNOuYpnrP2qZONFh8zhO6J0Byqz6EqNaC/3LKvPuG6OW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Gn48yQFpijemW104K9DqC.rDmjPfCv5Virf4YRwHMfYbqNajIRApa");
        }
    }
}
