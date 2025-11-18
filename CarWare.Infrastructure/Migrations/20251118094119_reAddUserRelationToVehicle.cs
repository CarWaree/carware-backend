using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class reAddUserRelationToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "vehicles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_UserId",
                table: "vehicles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicles_AspNetUsers_UserId",
                table: "vehicles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vehicles_AspNetUsers_UserId",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_UserId",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "vehicles");
        }
    }
}
