using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoSeVenLosGolesEnTablaDePosiciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SeVenLosGolesEnTablaDePosiciones",
                schema: "dbo",
                table: "Torneos",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeVenLosGolesEnTablaDePosiciones",
                schema: "dbo",
                table: "Torneos");
        }
    }
}
