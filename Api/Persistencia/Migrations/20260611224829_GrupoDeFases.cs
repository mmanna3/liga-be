using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class GrupoDeFases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fases_TorneoId_Numero",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.AddColumn<int>(
                name: "GrupoDeFasesId",
                schema: "dbo",
                table: "Fases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GruposDeFases",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    TorneoId = table.Column<int>(type: "int", nullable: false),
                    GrupoDeFasesPadreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposDeFases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposDeFases_GruposDeFases_GrupoDeFasesPadreId",
                        column: x => x.GrupoDeFasesPadreId,
                        principalSchema: "dbo",
                        principalTable: "GruposDeFases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GruposDeFases_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalSchema: "dbo",
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$dwZ74xBlZBMHd7k1wQnU0etGT6U470AmPpEzdAE9ygo8nvbHploDm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Wyf9wWYtbCaUbr7edI.7PeaYWFuowQzVezacLZiBBPX07BheECwmO");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$Etgqads6kGNBGxDiVs7BpezOVX2knzTCbb8eKlAYz81vsjtAAfpRm");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$XpVq6T66ivihFMrvt6wg5ejqSrmKuTmx14pdnQwG5nqLT0yAXwhbK");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$nsPWq5uK7cq.QvczWNah5OhFc9rHN1DTtAYjf4TeLWHQ67EELZs4i");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$aTYrmaTln35NjAkLwsUhd.RHF/eKh3zdWyw.iwlv.5AkNEseNFu56");

            migrationBuilder.CreateIndex(
                name: "IX_Fases_GrupoDeFasesId",
                schema: "dbo",
                table: "Fases",
                column: "GrupoDeFasesId");

            migrationBuilder.CreateIndex(
                name: "IX_Fases_TorneoId",
                schema: "dbo",
                table: "Fases",
                column: "TorneoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposDeFases_GrupoDeFasesPadreId",
                schema: "dbo",
                table: "GruposDeFases",
                column: "GrupoDeFasesPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposDeFases_TorneoId_GrupoDeFasesPadreId_Numero",
                schema: "dbo",
                table: "GruposDeFases",
                columns: new[] { "TorneoId", "GrupoDeFasesPadreId", "Numero" });

            migrationBuilder.AddForeignKey(
                name: "FK_Fases_GruposDeFases_GrupoDeFasesId",
                schema: "dbo",
                table: "Fases",
                column: "GrupoDeFasesId",
                principalSchema: "dbo",
                principalTable: "GruposDeFases",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fases_GruposDeFases_GrupoDeFasesId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropTable(
                name: "GruposDeFases",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Fases_GrupoDeFasesId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropIndex(
                name: "IX_Fases_TorneoId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.DropColumn(
                name: "GrupoDeFasesId",
                schema: "dbo",
                table: "Fases");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$lrOofukIE4t7qh/64EUtMuLI4CsW0jxXn4CwzaksXgJgxtqdd/UYW");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$Vmd1pR0kWHxZZqe2AO130uZQJq/yTfUMXN7q8UiaRUgcaXfA.Az..");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$FmCV2cVhVc5vnj3Qgd2ayeA.BeUKGpYI5JoquZrzx1QYLfwr3/NSq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$0CYhmjc9ih67MDFODXAvQujoFAc0DC9qjcGttOWUtt4Zu0s7fJdrq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$JkpRKdEANG//JIul8CHPY.NoI7bIeirqKE9uqoykpFOi3oZsnAyfy");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$1zCunL/999BlC6KRcv4Wze9xSQSML5ijOiAjVIz/AyYjzsSza1Zjm");

            migrationBuilder.CreateIndex(
                name: "IX_Fases_TorneoId_Numero",
                schema: "dbo",
                table: "Fases",
                columns: new[] { "TorneoId", "Numero" },
                unique: true);
        }
    }
}
