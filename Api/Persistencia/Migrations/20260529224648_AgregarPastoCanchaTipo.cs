using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPastoCanchaTipo : Migration
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

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_CanchaTipo",
                columns: new[] { "Id", "Tipo" },
                values: new object[,]
                {
                    { 5, "PastoSintetico" },
                    { 6, "PastoNatural" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "_CanchaTipo",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "_CanchaTipo",
                keyColumn: "Id",
                keyValue: 6);

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
    }
}
