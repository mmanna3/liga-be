using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoAgrupadorGeneralVisibleEnAppFalse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TorneoAgrupadores",
                keyColumn: "Id",
                keyValue: 1,
                column: "VisibleEnApp",
                value: false);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$tNKT2hqYy2Cn.3CUS1ztluzl70Tb3cSR3wXQSBbCckQtdon2jYToO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Gtz/.FrpKqEkyHGPC17xfer2fDia1McKuMZGhoT.dHh.gLGKtwbyS");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$tP2vED12CXZzcOF10gAgXuCGkCroNpFrZcSRru5IaMFlUP6i0c0rW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TorneoAgrupadores",
                keyColumn: "Id",
                keyValue: 1,
                column: "VisibleEnApp",
                value: true);

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
        }
    }
}
