using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class EliminarCanchaSuperficie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs__CanchaSuperficie_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropTable(
                name: "_CanchaSuperficie",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$U9E3yVbk.uwmqx829.LXX.XTxq5dug2oQ/YWsp.2XR8hxweeSPwpi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Mo89qLb95c47.UXaBqHtHOIXwQMxNYAODoyHLzkThoA2tDWScUGBq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$LVRqMsMn9CxEfjQYeGTm2ewIIw7i5h7WNSMRqBF/tPSCP..1HLeRO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$4.b.8FH6Z1TdorsR5UDace9FqQfw9RWG90/QJrVvOIkWk.i3fRXEe");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$zGzIXMbFNbPxFJuQTrIkMO1AKwsonN3Y68K3IUbiMkQ/d.evXgjQ2");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$hh5qFMkeNVyIL.KyIg06IObiAgl2/jI7GiPHPShpIGiXOQcRkNFIW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "_CanchaSuperficie",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Superficie = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CanchaSuperficie", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$jiQ/8FndoKXxdKSbqBDVceZVK50503YzuHXn1pxkmIwNkVumHjN5i");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$eF9YvI4QIPdySzbzqC2OReiOi76OD/l1CKBrXsaDwpVY8GZOusU9e");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$V76xkjcN9uYy1smhqnZoh.PldhSFFjO.Gir2rUv4dQXffSYN5FsZ6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$ausyn33SOWl9m54rluyzSucfhbzRoB1YLAI5RaWM7lQeFFV9dAiKW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$Iio/iKNZwnENA71U5t5P0efpGY5t7P5e/ObEcUY4l.iPMKJNv6A92");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$6z7aoWb4wBMiI7STnJjJru0nA1kf1NgyOOAFKOvL06VGUbTWeGwRe");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_CanchaSuperficie",
                columns: new[] { "Id", "Superficie" },
                values: new object[,]
                {
                    { 1, "PastoNatural" },
                    { 2, "PastoSintetico" },
                    { 3, "Consultar" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs",
                column: "CanchaSuperficieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs__CanchaSuperficie_CanchaSuperficieId",
                schema: "dbo",
                table: "Clubs",
                column: "CanchaSuperficieId",
                principalSchema: "dbo",
                principalTable: "_CanchaSuperficie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
