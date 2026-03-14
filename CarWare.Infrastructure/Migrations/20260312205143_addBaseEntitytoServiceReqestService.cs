using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addBaseEntitytoServiceReqestService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ServiceRequestService",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "ServiceRequestService");
        }
    }
}
