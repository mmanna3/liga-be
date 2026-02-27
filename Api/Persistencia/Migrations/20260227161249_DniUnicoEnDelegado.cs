using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class DniUnicoEnDelegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$AFyUWGa7g7/QDmet0PL5puH9Z9lhtyTecRJMfSWxY9F3jXeRWkqYq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$qkkN7u9wuqvnL3f8HNyoX./.yQClXFc6K9fRnUOHDAuJle8T5vzAW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$OJxB/fK2SvQEz9..6D03OOD9QmwQ99nAKx5BlxUmtl.HRg0bkltzu");

            migrationBuilder.CreateIndex(
                name: "IX_Delegados_DNI",
                table: "Delegados",
                column: "DNI",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Delegados_DNI",
                table: "Delegados");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$hSHCz9NjNbgnIT.x7EJHAu7DDRrLORD04Q/m2e8iX5q4jObskwuXO");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$3B4fsrg0MfTR73dvHD/wQeE81E1ewSkMRHNAGrTiasALCD2ARXgkW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$BGHqFLIR6jGrcOfBH4ITquXDw/jv8rBScmatUFrPYVQB.4LV3cDW2");
        }
    }
}
