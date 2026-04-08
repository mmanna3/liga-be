using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoFaseAperturaClausuraTablaAnual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FaseAperturaId",
                schema: "dbo",
                table: "Torneos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaseClausuraId",
                schema: "dbo",
                table: "Torneos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Torneos_FaseAperturaId",
                schema: "dbo",
                table: "Torneos",
                column: "FaseAperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Torneos_FaseClausuraId",
                schema: "dbo",
                table: "Torneos",
                column: "FaseClausuraId");

            migrationBuilder.AddForeignKey(
                name: "FK_Torneos_Fases_FaseAperturaId",
                schema: "dbo",
                table: "Torneos",
                column: "FaseAperturaId",
                principalSchema: "dbo",
                principalTable: "Fases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Torneos_Fases_FaseClausuraId",
                schema: "dbo",
                table: "Torneos",
                column: "FaseClausuraId",
                principalSchema: "dbo",
                principalTable: "Fases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Torneos_Fases_FaseAperturaId",
                schema: "dbo",
                table: "Torneos");

            migrationBuilder.DropForeignKey(
                name: "FK_Torneos_Fases_FaseClausuraId",
                schema: "dbo",
                table: "Torneos");

            migrationBuilder.DropIndex(
                name: "IX_Torneos_FaseAperturaId",
                schema: "dbo",
                table: "Torneos");

            migrationBuilder.DropIndex(
                name: "IX_Torneos_FaseClausuraId",
                schema: "dbo",
                table: "Torneos");

            migrationBuilder.DropColumn(
                name: "FaseAperturaId",
                schema: "dbo",
                table: "Torneos");

            migrationBuilder.DropColumn(
                name: "FaseClausuraId",
                schema: "dbo",
                table: "Torneos");
        }
    }
}
