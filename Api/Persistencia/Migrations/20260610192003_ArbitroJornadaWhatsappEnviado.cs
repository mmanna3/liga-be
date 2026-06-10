using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ArbitroJornadaWhatsappEnviado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WhatsappEnviado",
                schema: "dbo",
                table: "ArbitroJornada",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$QV3OLmIbqy0Wcd4zPXKJTe0Q8zWy4TbE0W0I7Q/LO8AlQQ4aHj7Ae");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$ZMbD5q6GVXbHap5JmJ3gre/ng3I8zbwO2P..XOKUaBtr4C6gtrJHe");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$TLXDTbKx6Jdpzn5bHv1AiO6EELGs1I2XxQco6fdKY1kK3yO8liCg.");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$AiloM86gWe.b4B3K5fPq9OFn9hZ9DWjIadnLNAtJJZCEd1EH.T48e");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$h6wsS3svhO8CyM5GIlU.kemdPmR/gAycr1MfBONGT/0Jfw9njJLLa");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$05DIwUsCf3RG9YqjjZVhHOsxaIvwH2Y3ax4zgEU.udnZkYab6bw1O");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsappEnviado",
                schema: "dbo",
                table: "ArbitroJornada");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$5GsKNOUrqoBzhqTXSdc/DO7mJLp16.HScy0f/tux1lqooLmTEV.eG");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$12$anD5B3.rgoUC64ODoX9tw.aUVzQHqc0UQlaDk/1l.GWdm0eO0fROa");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 101,
                column: "Password",
                value: "$2a$12$J/j9ysZPY.e1UZ2l1CYiR.ourHWPvGj5d0dKK4UfeQzynYKmbhQAC");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Password",
                value: "$2a$12$Sa/W9OGJa4KYsndp4iUmxuRjgfgDyJxMnwNjhaSZl0ntzvttIS1O6");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password",
                value: "$2a$12$zgcuj/J62Sr.qSvLHaIoN.H3KOd5G6F0yD0I1Z4HUc5GZHk5YkUDu");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1002,
                column: "Password",
                value: "$2a$12$87vyIeIYSVnnYJi4iyc2UuvsIQ8A260JfZFrlWUFTk7sCVk0XAQzW");
        }
    }
}
