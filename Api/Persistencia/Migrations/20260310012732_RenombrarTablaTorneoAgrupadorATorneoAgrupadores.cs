using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTablaTorneoAgrupadorATorneoAgrupadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Torneos_TorneoAgrupador_TorneoAgrupadorId",
                table: "Torneos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TorneoAgrupador",
                table: "TorneoAgrupador");

            migrationBuilder.RenameTable(
                name: "TorneoAgrupador",
                newName: "TorneoAgrupadores");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TorneoAgrupadores",
                table: "TorneoAgrupadores",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$NQV8vGcuAyINWjMQ.UUI9OxhrUy18DPA7oeZkAMZ0IkeTy1vI/bmu");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$h7T95Ae/1EZSev6.GOuN/ehQ7iTGMHE0Tk8RBX1C3slQftDCX2Ol.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Uwd8v1T/V7ywukf0tQ0cmOnarFY8kM5FwnQnOucoCaPwcBCncrAYm");

            migrationBuilder.AddForeignKey(
                name: "FK_Torneos_TorneoAgrupadores_TorneoAgrupadorId",
                table: "Torneos",
                column: "TorneoAgrupadorId",
                principalTable: "TorneoAgrupadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Torneos_TorneoAgrupadores_TorneoAgrupadorId",
                table: "Torneos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TorneoAgrupadores",
                table: "TorneoAgrupadores");

            migrationBuilder.RenameTable(
                name: "TorneoAgrupadores",
                newName: "TorneoAgrupador");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TorneoAgrupador",
                table: "TorneoAgrupador",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Torneos_TorneoAgrupador_TorneoAgrupadorId",
                table: "Torneos",
                column: "TorneoAgrupadorId",
                principalTable: "TorneoAgrupador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
