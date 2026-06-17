using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioAccesoModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioAccesoModulo",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Modulo = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioAccesoModulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioAccesoModulo_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAccesoModulo_UsuarioId_Modulo",
                schema: "dbo",
                table: "UsuarioAccesoModulo",
                columns: new[] { "UsuarioId", "Modulo" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO dbo.UsuarioAccesoModulo (UsuarioId, Modulo, Nivel)
                SELECT u.Id, m.Modulo, 2
                FROM dbo.Usuarios u
                CROSS JOIN (VALUES (1), (2), (3), (4), (5), (6), (7), (8)) AS m(Modulo)
                WHERE u.RolId = 1 AND u.DelegadoId IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioAccesoModulo",
                schema: "dbo");
        }
    }
}
