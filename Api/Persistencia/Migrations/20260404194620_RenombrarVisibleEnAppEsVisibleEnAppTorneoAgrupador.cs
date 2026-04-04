using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarVisibleEnAppEsVisibleEnAppTorneoAgrupador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VisibleEnApp",
                schema: "dbo",
                table: "TorneoAgrupadores",
                newName: "EsVisibleEnApp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EsVisibleEnApp",
                schema: "dbo",
                table: "TorneoAgrupadores",
                newName: "VisibleEnApp");
        }
    }
}
