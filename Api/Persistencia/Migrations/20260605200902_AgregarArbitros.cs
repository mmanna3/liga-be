using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarArbitros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arbitros",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DNI = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    TelefonoCelular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arbitros", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$DSylm9JD/zIo.EE9rzgZcetUB7kyI.E7sbMVDixvXD/rG1FjMq3Gm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$77TVQrpZyjsEmzNOPOXtxe2SO2wayA9RzseUI/ZEAmhTtsvjVxH3S");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$sK/fBFtoPtXLcy5KjtqWoeo2vxwkLSjkNVVFlirYp05V7oWIM0sKa");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$dSBSgbktoM3LNFJPVsQE0e7NqeCDfcda8ILFtgBTCIEMY0A46hx4e");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$6eAYsvOzbiuAyVnpnZQrn.6lvHlo9Ulv2HCPaKkOT30AHaqYGgcoG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$KtQUiD2JZbQ7TBDQnH/v3.BM6eOgAtKXr2mh9BTtO7AgSuGmw69oC");

            migrationBuilder.CreateIndex(
                name: "IX_Arbitros_DNI",
                schema: "dbo",
                table: "Arbitros",
                column: "DNI",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arbitros",
                schema: "dbo");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$6hasIZb574IpswLA/SqxnOpA7zzAWvK.oLgePBLq1Lt6838RFABBS");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$6itwklGbgS0Ka6bXPMvFpO8XLOsVhR08sstu6Tj3NuHMyh3X3Buxq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$fhp8BmEbLYK0d/V/oO174usLvmaFLvQL1AAhunc7J6nVeTlyaIktG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$5J7/2DbIaaI3Z1I9lNoLM.fbeqmXadi/HMSBQ0.6ZJQShQOrrmeSu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$Hw11J3sqBndcn6dfFam13.mVdF0318QVZuEh3c/k3CQB5Z3W6utH6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$tU8YAzU/nQ0q89h0N1/AKu8qamMF2d/mSD8uqQkhRAcm/8MPAJQsm");
        }
    }
}
