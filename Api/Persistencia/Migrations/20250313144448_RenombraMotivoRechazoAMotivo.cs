using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombraMotivoRechazoAMotivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MotivoDeRechazoFichaje",
                table: "JugadorEquipo",
                newName: "Motivo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "JugadorEquipo",
                newName: "MotivoDeRechazoFichaje");
        }
    }
}
