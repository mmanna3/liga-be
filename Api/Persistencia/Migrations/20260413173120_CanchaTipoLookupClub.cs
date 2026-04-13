using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class CanchaTipoLookupClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_CanchaTipo",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CanchaTipo", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_CanchaTipo",
                columns: new[] { "Id", "Tipo" },
                values: new object[,]
                {
                    { 1, "Cubierta" },
                    { 2, "Descubierta" },
                    { 3, "Semidescubierta" },
                    { 4, "Consultar" }
                });

            migrationBuilder.AddColumn<int>(
                name: "CanchaTipoId",
                schema: "dbo",
                table: "Clubs",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE dbo.Clubs
                SET CanchaTipoId = CASE
                    WHEN EsTechado = 1 THEN 1
                    WHEN EsTechado = 0 THEN 2
                    ELSE 4
                END
                """);

            migrationBuilder.Sql(
                "UPDATE dbo.Clubs SET CanchaTipoId = 4 WHERE CanchaTipoId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "CanchaTipoId",
                schema: "dbo",
                table: "Clubs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CanchaTipoId",
                schema: "dbo",
                table: "Clubs",
                column: "CanchaTipoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs__CanchaTipo_CanchaTipoId",
                schema: "dbo",
                table: "Clubs",
                column: "CanchaTipoId",
                principalSchema: "dbo",
                principalTable: "_CanchaTipo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs__CanchaTipo_CanchaTipoId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_CanchaTipoId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "CanchaTipoId",
                schema: "dbo",
                table: "Clubs");

            migrationBuilder.DropTable(
                name: "_CanchaTipo",
                schema: "dbo");
        }
    }
}
