using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class CreaTorneoAgrupador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TorneoAgrupador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisibleEnApp = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneoAgrupador", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO TorneoAgrupador (Nombre, VisibleEnApp) VALUES (N'General', 1);");

            migrationBuilder.AddColumn<int>(
                name: "TorneoAgrupadorId",
                table: "Torneos",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$JjkN5jTonpUs9MNBgHdC5ezQ.obcpIFA39fHcsDGUk1XEdnyisx76");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$f0ERzHyYRq998zIEZXuVIO49m/iQN6VTwJTro6Ig/w8XBJjxKv2Da");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$WvpGQYXYxU/051RTuERjrOZFM/vjNIslMRmfGQZEsd6gtbYXt1zUa");

            migrationBuilder.CreateIndex(
                name: "IX_Torneos_TorneoAgrupadorId",
                table: "Torneos",
                column: "TorneoAgrupadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Torneos_TorneoAgrupador_TorneoAgrupadorId",
                table: "Torneos",
                column: "TorneoAgrupadorId",
                principalTable: "TorneoAgrupador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Torneos_TorneoAgrupador_TorneoAgrupadorId",
                table: "Torneos");

            migrationBuilder.DropTable(
                name: "TorneoAgrupador");

            migrationBuilder.DropIndex(
                name: "IX_Torneos_TorneoAgrupadorId",
                table: "Torneos");

            migrationBuilder.DropColumn(
                name: "TorneoAgrupadorId",
                table: "Torneos");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$M.NmFcElfUp9yM8TTVGOsukI89N1rZMquL3lrL0SnxvFZYCfRR7RW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$/WNxVyo4PqTpIkjFI5D.NOniDpHGQBVKlpXc8OuVpmRpA.Ypxy8VG");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$HS43iRyR7TZjuQkTuuBTnOwQMMd6bEtTmmpmpsHjWdDdaCeEZdAiq");
        }
    }
}
