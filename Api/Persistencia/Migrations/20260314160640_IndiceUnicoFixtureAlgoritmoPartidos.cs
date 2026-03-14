using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class IndiceUnicoFixtureAlgoritmoPartidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$3wTFUPnoEV8pqXaAx2.7seadx2qrdq1lQniyLltI2SeFfhAJwvwbG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$2HYmjpbUceChv1L9H2maX.EO0UMVGvlQBowbN1l1ePno9HsOFEuAe");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$fHiOiz/bQ3B9XNJ/AGrCMuvhgqmpvwyhwXfpRXuQWDwJtM0wc.oey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$0FZW58zjzhbkS/qlX0HUUuBE74sAbleG7XiMq6xicnvDI6mHtjzw6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$GAqi.Rb.OURsr6vKFeUEy.28caU2znlK5d0ppIeN.Du5uCbM8yr9.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$l0opGwFbACIzAZ36V2hzZerq5204hZZ5YfO59tqdbppbc1mvJ2m6K");
        }
    }
}
