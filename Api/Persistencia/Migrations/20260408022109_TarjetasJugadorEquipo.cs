using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TarjetasJugadorEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TarjetasAmarillas",
                schema: "dbo",
                table: "JugadorEquipo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TarjetasRojas",
                schema: "dbo",
                table: "JugadorEquipo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TarjetasAmarillas",
                schema: "dbo",
                table: "JugadorEquipo");

            migrationBuilder.DropColumn(
                name: "TarjetasRojas",
                schema: "dbo",
                table: "JugadorEquipo");
        }
    }
}
