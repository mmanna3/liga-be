using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AlgoritmosFixtureDe6Equipos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                columns: new[] { "Id", "CantidadDeEquipos", "Nombre" },
                values: new object[,]
                {
                    { 13, 6, "Apertura" },
                    { 14, 6, "Clausura" }
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$IE3LIvilp./YqY2hQF2IGeySoStnXS6McYJHFvRxgNqMCa3l3dE1u");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$hr4Ioa8sY0vcMuyOVjFJZeHcVDtRBrGoy83s9IVipuanojMxkbld6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$9dtHRwGCMIZ.JBh6kW1IB.eYvTV4q6/SjFqw78Z/bTNIRXcOsT.Qm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$B8UfiiZYLRoZKzGcbhVOd.Hv2m.kGLumF3kouaqgJMcxbHHp3Mapy");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$kFxhuy5qvlV2I5./YOmJEuLJkQeplFbLtezN/TZsh/bCOR1mvejFO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$W59/2XgFNGJ42O9HcEv3p.7AyRw1hQL3sqE6qNyTirRq7u4IRMAMK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "FixtureAlgoritmos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$cySkpsz/jgZ4gF6EbHVpK.X6JoWSp55XMInjFXgJliBcs.R6b.LO.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$sPHYdMQWhMGWHCgUG/irFuyTZrr2rqYSIlbHDWTcnl0kIF1eODi2y");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$8xq9kwxoH1k10a24umalFOHLEJageScjy8K2.2B6YBzXBSmfQjssi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$Uh0CLF/GaOYogMZZER8A2.qSzgPyF7tVRkIAzBn1K7wvJ0tXGbetm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$dwC/NXxq7pyFjDtfzmGLs.wKizMUsW3fBOMD71GXNeffV.QvUNI4K");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$JDxEqjXekf2NBlTI4wfOaOCqhFN087bA1HtJlWBbDfbL6U6Rewspy");
        }
    }
}
