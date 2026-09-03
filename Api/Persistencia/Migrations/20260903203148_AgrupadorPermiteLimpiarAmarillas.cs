using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgrupadorPermiteLimpiarAmarillas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PermiteLimpiarAmarillas",
                schema: "dbo",
                table: "TorneoAgrupadores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "TorneoAgrupadores",
                keyColumn: "Id",
                keyValue: 1,
                column: "PermiteLimpiarAmarillas",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermiteLimpiarAmarillas",
                schema: "dbo",
                table: "TorneoAgrupadores");
        }
    }
}
