using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class PartidoPenalesOpcionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PenalesLocal",
                schema: "dbo",
                table: "Partidos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PenalesVisitante",
                schema: "dbo",
                table: "Partidos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$P2ZmsPE3t1I1hymA61lF4ez1HjwUCPl27P41WS8FqrhOP46Pnqpnq");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$LEtbQilEoENi3yPbhFmtBOiDNCFt9ZtU8a7zEPVkqB2SOEmpqcg3S");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$mC9VvOVzMH.jBo8V7Y2FaOiTWCgK8tJRkCqq318OJSdqeSI2oU71W");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$iw566E588rjNiUqUdnyMp./DNlQJdLQxscxMA/akRlPUtGKSWmkmi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$RIwk.Joizgj4a9meUt8Jreg/we2VhulnjBC3YdOl6GKuvLU1UFysS");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$KyFSR7/wCT.J..6CP2cEuO2VGodpy00vLzkU2Cg5xY0ixcXdeL1qG");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Partido_PenalesLocal_Valido",
                schema: "dbo",
                table: "Partidos",
                sql: "([PenalesLocal] IS NULL OR ([PenalesLocal] NOT LIKE '%[^0-9]%' AND LEN([PenalesLocal]) > 0))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Partido_PenalesVisitante_Valido",
                schema: "dbo",
                table: "Partidos",
                sql: "([PenalesVisitante] IS NULL OR ([PenalesVisitante] NOT LIKE '%[^0-9]%' AND LEN([PenalesVisitante]) > 0))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Partido_PenalesLocal_Valido",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Partido_PenalesVisitante_Valido",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.DropColumn(
                name: "PenalesLocal",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.DropColumn(
                name: "PenalesVisitante",
                schema: "dbo",
                table: "Partidos");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$LmIfqL1fa3bfL5Qy6fNkNeXb3bPLF7z4yHG5KvBdQ2gHxlMM6BcfS");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$zYrSKf3UwnbClzohampU9u5rh.tD911ar4bdY8RMqzUxZWWhlGLLu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$vn.IRhq/1jXgHmCFMlYVT.guSWjtF8S3QsHrRv2Gnxd9EEpaX/dyi");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$PW2L5TUOZmv83dBA/5xI8O0vv9CoLBAJbB/a4cPfJhOmH0vgvefR.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$S1IgDY/RALm1BkaZzUpy2OZ3o85YodJn/18umhzWomrOyZ..0faRG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$O9iwfliX.8o8VcTYAIu37uvoAewACrZG5THdRhA5A8eFXNBWook6q");
        }
    }
}
