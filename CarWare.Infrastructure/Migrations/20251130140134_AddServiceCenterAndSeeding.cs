using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterAndSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderServices",
                columns: table => new
                {
                    ServiceCenterId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderServices", x => new { x.ServiceCenterId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_ProviderServices_ServiceCenters_ServiceCenterId",
                        column: x => x.ServiceCenterId,
                        principalTable: "ServiceCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderServices_maintenanceTypes_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "maintenanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ServiceCenters",
                columns: new[] { "Id", "Location", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "Nasr City", "Provider 1", "0123456789" },
                    { 2, "Maadi", "Provider 2", "0112233445" },
                    { 3, "Heliopolis", "Provider 3", "01011223344" },
                    { 4, "6th October", "Provider 4", "01233445566" },
                    { 5, "Dokki", "Provider 5", "01099887766" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderServices_ServiceId",
                table: "ProviderServices",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderServices");

            migrationBuilder.DropTable(
                name: "ServiceCenters");
        }
    }
}
