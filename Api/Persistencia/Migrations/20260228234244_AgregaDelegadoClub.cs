using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaDelegadoClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DelegadoClub",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DelegadoId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegadoClub", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DelegadoClub_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DelegadoClub_Delegados_DelegadoId",
                        column: x => x.DelegadoId,
                        principalTable: "Delegados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Migrar datos existentes: copiar ClubId de cada Delegado a la nueva tabla DelegadoClub
            migrationBuilder.Sql(
                "INSERT INTO DelegadoClub (DelegadoId, ClubId) SELECT Id, ClubId FROM Delegados WHERE ClubId IS NOT NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Delegados_Clubs_ClubId",
                table: "Delegados");

            migrationBuilder.DropIndex(
                name: "IX_Delegados_ClubId",
                table: "Delegados");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Delegados");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$QB9FjCGu2zYdZFBCmB/16ulLqA2pId0VvxJ0VL5HN/Yiq9H/s/gJG");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$STQqmm8mykCfe.Zwz1rFiOanM6zGajTUaF.Qc.l6zhY3BBKAXTYBy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$VfvDEk1W8Moj5nCmltl/xe/pRok4vSc1mekwuXq0LJFNngXy4m9HG");

            migrationBuilder.CreateIndex(
                name: "IX_DelegadoClub_ClubId",
                table: "DelegadoClub",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegadoClub_DelegadoId_ClubId",
                table: "DelegadoClub",
                columns: new[] { "DelegadoId", "ClubId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar ClubId (nullable primero para poder migrar datos)
            migrationBuilder.AddColumn<int>(
                name: "ClubId",
                table: "Delegados",
                type: "int",
                nullable: true,
                defaultValue: null);

            // Restaurar datos: usar el primer club asociado a cada delegado
            migrationBuilder.Sql(
                "UPDATE d SET d.ClubId = (SELECT TOP 1 ClubId FROM DelegadoClub WHERE DelegadoId = d.Id ORDER BY Id) FROM Delegados d");

            migrationBuilder.DropTable(
                name: "DelegadoClub");

            // Convertir a NOT NULL con valor por defecto 0 para los que quedaron sin club
            migrationBuilder.AlterColumn<int>(
                name: "ClubId",
                table: "Delegados",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$AFyUWGa7g7/QDmet0PL5puH9Z9lhtyTecRJMfSWxY9F3jXeRWkqYq");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$qkkN7u9wuqvnL3f8HNyoX./.yQClXFc6K9fRnUOHDAuJle8T5vzAW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$OJxB/fK2SvQEz9..6D03OOD9QmwQ99nAKx5BlxUmtl.HRg0bkltzu");

            migrationBuilder.CreateIndex(
                name: "IX_Delegados_ClubId",
                table: "Delegados",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegados_Clubs_ClubId",
                table: "Delegados",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
