using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ZonaOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Zonas_FaseId",
                schema: "dbo",
                table: "Zonas");

            // Agregamos la columna como nullable para poder poblar los registros
            // existentes con un ROW_NUMBER() antes de marcarla NOT NULL.
            migrationBuilder.AddColumn<int>(
                name: "Orden",
                schema: "dbo",
                table: "Zonas",
                type: "int",
                nullable: true);

            // Asignamos un orden 1..N por FaseId, ordenando alfabéticamente por Nombre
            // (con el Id como desempate determinístico para nombres repetidos).
            migrationBuilder.Sql(@"
                WITH OrdenadoPorFase AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (PARTITION BY FaseId ORDER BY Nombre, Id) AS NuevoOrden
                    FROM dbo.Zonas
                )
                UPDATE z
                SET z.Orden = o.NuevoOrden
                FROM dbo.Zonas z
                INNER JOIN OrdenadoPorFase o ON o.Id = z.Id;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                schema: "dbo",
                table: "Zonas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$6HArK8GrVmbwjMuYQo3iSODO86TPRCkkAAaAR3Q2g4q3PyfUgvHmi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$F/lTeQ4Mb2yu6CaMCf6LbeBVuxcBYIvGOCu779uUIrXZ3AgGt06nS");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$GdpkRWawLRCx9PRpvo5wV.6/8CFiYvmRbezIVI1GCn5pYOmZSZ2XG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$VRnkyJelLVT8OUK.M3ZJkO2hXnverZiIJNxPifo./NtuQUPL0cwcK");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$r3GdoerRavWFKCGjGkIE3OxcN7bjeoYLKmCCqVYL4EBGFTI3RukN2");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$UpdAba2tKTVXCll3fQ1.B.ZomlcNw3Us6qdcTmx91cOoF.NB4NJZu");

            migrationBuilder.CreateIndex(
                name: "IX_Zonas_FaseId_Orden",
                schema: "dbo",
                table: "Zonas",
                columns: new[] { "FaseId", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Zonas_FaseId_Orden",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.DropColumn(
                name: "Orden",
                schema: "dbo",
                table: "Zonas");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$WtH0t9WLj3CVWUdRdeQTBeYjxWqHHLPXMpJJJlrh5CdUfHW92ffG2");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$gUYUTtr9OYvBhCEi/8ysV.JbTLzquDdeISbtcZUFcuyWfsvOIgMBO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$lFf9g50zbvqX600P4ixT0uBAqRN4J3wcQs/RKWRMXVYAgBMCa4HbW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$qTwKHUMI/2gjEWqaHuBtO.9zatYGx2NZ1Sl06T44QfUoH3uQTGvD6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$a6N9xSInKfIzWLIydD4SeOwddTkC3q2A.qNC.dBQOyMS8UOkc1gI.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$LLQR6reDZ0umRvb1mL1shO1OWNnSaA8t0O6M0diGH1JsgihKHb4Tu");

            migrationBuilder.CreateIndex(
                name: "IX_Zonas_FaseId",
                schema: "dbo",
                table: "Zonas",
                column: "FaseId");
        }
    }
}
