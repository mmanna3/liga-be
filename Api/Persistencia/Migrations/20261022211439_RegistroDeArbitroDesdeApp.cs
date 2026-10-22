using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RegistroDeArbitroDesdeApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                schema: "dbo",
                table: "Arbitros",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoArbitroId",
                schema: "dbo",
                table: "Arbitros",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                schema: "dbo",
                table: "Arbitros",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Institucion",
                schema: "dbo",
                table: "Arbitros",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                schema: "dbo",
                table: "Arbitros",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneRelacionInstitucional",
                schema: "dbo",
                table: "Arbitros",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoMovilidadArbitroId",
                schema: "dbo",
                table: "Arbitros",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "_EstadoArbitro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EstadoArbitro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_FranjaHorariaArbitro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Franja = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FranjaHorariaArbitro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_TipoMovilidadArbitro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoMovilidadArbitro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArbitroFranjaHoraria",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArbitroId = table.Column<int>(type: "int", nullable: false),
                    FranjaHorariaArbitroId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArbitroFranjaHoraria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArbitroFranjaHoraria_Arbitros_ArbitroId",
                        column: x => x.ArbitroId,
                        principalSchema: "dbo",
                        principalTable: "Arbitros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArbitroFranjaHoraria__FranjaHorariaArbitro_FranjaHorariaArbitroId",
                        column: x => x.FranjaHorariaArbitroId,
                        principalSchema: "dbo",
                        principalTable: "_FranjaHorariaArbitro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_EstadoArbitro",
                columns: new[] { "Id", "Estado" },
                values: new object[,]
                {
                    { 1, "Pendiente de aprobación" },
                    { 2, "Activo" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_FranjaHorariaArbitro",
                columns: new[] { "Id", "Franja" },
                values: new object[,]
                {
                    { 1, "Sabados8A14" },
                    { 2, "Sabados13A20" },
                    { 3, "Domingos9A15" },
                    { 4, "Domingos15A22" },
                    { 5, "MartesAViernes20A2330" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "_TipoMovilidadArbitro",
                columns: new[] { "Id", "Tipo" },
                values: new object[,]
                {
                    { 1, "Moto" },
                    { 2, "Auto" },
                    { 3, "Bicicleta" },
                    { 4, "TransportePublico" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arbitros_EstadoArbitroId",
                schema: "dbo",
                table: "Arbitros",
                column: "EstadoArbitroId");

            migrationBuilder.CreateIndex(
                name: "IX_Arbitros_TipoMovilidadArbitroId",
                schema: "dbo",
                table: "Arbitros",
                column: "TipoMovilidadArbitroId");

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroFranjaHoraria_ArbitroId_FranjaHorariaArbitroId",
                schema: "dbo",
                table: "ArbitroFranjaHoraria",
                columns: new[] { "ArbitroId", "FranjaHorariaArbitroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArbitroFranjaHoraria_FranjaHorariaArbitroId",
                schema: "dbo",
                table: "ArbitroFranjaHoraria",
                column: "FranjaHorariaArbitroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arbitros__EstadoArbitro_EstadoArbitroId",
                schema: "dbo",
                table: "Arbitros",
                column: "EstadoArbitroId",
                principalSchema: "dbo",
                principalTable: "_EstadoArbitro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Arbitros__TipoMovilidadArbitro_TipoMovilidadArbitroId",
                schema: "dbo",
                table: "Arbitros",
                column: "TipoMovilidadArbitroId",
                principalSchema: "dbo",
                principalTable: "_TipoMovilidadArbitro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arbitros__EstadoArbitro_EstadoArbitroId",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropForeignKey(
                name: "FK_Arbitros__TipoMovilidadArbitro_TipoMovilidadArbitroId",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropTable(
                name: "_EstadoArbitro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "_TipoMovilidadArbitro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ArbitroFranjaHoraria",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "_FranjaHorariaArbitro",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Arbitros_EstadoArbitroId",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropIndex(
                name: "IX_Arbitros_TipoMovilidadArbitroId",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "Direccion",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "EstadoArbitroId",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "Institucion",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "Localidad",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "TieneRelacionInstitucional",
                schema: "dbo",
                table: "Arbitros");

            migrationBuilder.DropColumn(
                name: "TipoMovilidadArbitroId",
                schema: "dbo",
                table: "Arbitros");
        }
    }
}
