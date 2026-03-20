using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaNombreEnFixtureAlgoritmo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                schema: "dbo",
                table: "FixtureAlgoritmos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nombre",
                value: "Clásico");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nombre",
                value: "Apertura");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Nombre",
                value: "Apertura");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 4,
                column: "Nombre",
                value: "Apertura");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 5,
                column: "Nombre",
                value: "Apertura");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nombre",
                value: "Apertura");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                columns: new[] { "Id", "CantidadDeEquipos", "Nombre" },
                values: new object[,]
                {
                    { 7, 4, "Champions" },
                    { 8, 8, "Clausura" },
                    { 9, 10, "Clausura" },
                    { 10, 12, "Clausura" },
                    { 11, 14, "Clausura" },
                    { 12, 16, "Clausura" }
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$.GzqiTYWbLDSDjgqoPcmZeTRXYqecOMw8iLXd6FcOVAHB7cjl.NRS");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$bgRU8l/M3ZtIvUO.Vcrv8uJ/w4S1UVbazyI565NbPUXMm7bSBc3ni");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$UR2pd2xfs4dIsK/r8cjAi.M2DgShOC9Uj6hB.oxUi0xl.jzMoccSW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "Nombre",
                schema: "dbo",
                table: "FixtureAlgoritmos");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$gxVTKJfRt4NaZWMTCqkcles37mczI168hsH3xHoQpX8YfuAlgxk36");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$0zRNCU1pgd1cirTLHmDgWOc0i/ua2HZr/C3J0KJlLMWT8HarfM9l.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$lAeKfYy86RP0sEjbPMb/P.l31U0dK0EZzQDtaipjPsM01Tru5m4nK");
        }
    }
}
