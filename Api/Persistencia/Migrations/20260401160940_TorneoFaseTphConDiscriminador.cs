using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class TorneoFaseTphConDiscriminador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoFase",
                schema: "dbo",
                table: "TorneoFases",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[TorneoFases]
                SET [TipoFase] = CASE [FaseFormatoId]
                    WHEN 1 THEN N'TodosContraTodos'
                    WHEN 2 THEN N'EliminacionDirecta'
                    ELSE N'TodosContraTodos'
                END
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_TorneoFases__FaseFormato_FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.DropIndex(
                name: "IX_TorneoFases_FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.DropColumn(
                name: "FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.AlterColumn<string>(
                name: "TipoFase",
                schema: "dbo",
                table: "TorneoFases",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[TorneoFases]
                SET [FaseFormatoId] = CASE [TipoFase]
                    WHEN N'TodosContraTodos' THEN 1
                    WHEN N'EliminacionDirecta' THEN 2
                    ELSE 1
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "TipoFase",
                schema: "dbo",
                table: "TorneoFases");

            migrationBuilder.CreateIndex(
                name: "IX_TorneoFases_FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases",
                column: "FaseFormatoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TorneoFases__FaseFormato_FaseFormatoId",
                schema: "dbo",
                table: "TorneoFases",
                column: "FaseFormatoId",
                principalSchema: "dbo",
                principalTable: "_FaseFormato",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
