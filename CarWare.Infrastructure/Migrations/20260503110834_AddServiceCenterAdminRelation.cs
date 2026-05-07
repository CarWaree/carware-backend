using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterAdminRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceCenterId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ServiceCenterId",
                table: "AspNetUsers",
                column: "ServiceCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ServiceCenters_ServiceCenterId",
                table: "AspNetUsers",
                column: "ServiceCenterId",
                principalTable: "ServiceCenters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ServiceCenters_ServiceCenterId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ServiceCenterId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ServiceCenterId",
                table: "AspNetUsers");
        }
    }
}
