using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoZonaTphConDiscriminador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoZona",
                schema: "dbo",
                table: "TorneoZonas",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "TodosContraTodos");

            // Por si quedara algún valor vacío, todas las zonas históricas son TCT.
            migrationBuilder.Sql(
                """
                UPDATE [dbo].[TorneoZonas]
                SET [TipoZona] = N'TodosContraTodos'
                WHERE [TipoZona] IS NULL OR [TipoZona] = N''
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoZona",
                schema: "dbo",
                table: "TorneoZonas");
        }
    }
}
