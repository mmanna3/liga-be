using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class DnisExpulsadosDeLaLiga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DnisExpulsadosDeLaLiga",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Explicacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DNI = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnisExpulsadosDeLaLiga", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DnisExpulsadosDeLaLiga_DNI",
                schema: "dbo",
                table: "DnisExpulsadosDeLaLiga",
                column: "DNI",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DnisExpulsadosDeLaLiga",
                schema: "dbo");
        }
    }
}
