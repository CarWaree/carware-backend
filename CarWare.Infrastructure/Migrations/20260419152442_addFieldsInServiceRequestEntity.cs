using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addFieldsInServiceRequestEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequest_AspNetUsers_UserId",
                table: "ServiceRequest");

            migrationBuilder.DropTable(
                name: "ServiceRequestService");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequest_AppointmentId",
                table: "ServiceRequest");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "ServiceRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ServiceRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedCompletion",
                table: "ServiceRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "ServiceRequest",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "ServiceRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ServiceRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianId",
                table: "ServiceRequest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianNotes",
                table: "ServiceRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceRequestItem",
                columns: table => new
                {
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestItem", x => new { x.ServiceRequestId, x.MaintenanceTypeId });
                    table.ForeignKey(
                        name: "FK_ServiceRequestItem_ServiceRequest_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequestItem_maintenanceTypes_MaintenanceTypeId",
                        column: x => x.MaintenanceTypeId,
                        principalTable: "maintenanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequest_AppointmentId",
                table: "ServiceRequest",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequest_TechnicianId",
                table: "ServiceRequest",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestItem_MaintenanceTypeId",
                table: "ServiceRequestItem",
                column: "MaintenanceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequest_AspNetUsers_TechnicianId",
                table: "ServiceRequest",
                column: "TechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequest_AspNetUsers_UserId",
                table: "ServiceRequest",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequest_AspNetUsers_TechnicianId",
                table: "ServiceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequest_AspNetUsers_UserId",
                table: "ServiceRequest");

            migrationBuilder.DropTable(
                name: "ServiceRequestItem");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequest_AppointmentId",
                table: "ServiceRequest");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequest_TechnicianId",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "EstimatedCompletion",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "TechnicianNotes",
                table: "ServiceRequest");

            migrationBuilder.CreateTable(
                name: "ServiceRequestService",
                columns: table => new
                {
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestService", x => new { x.ServiceRequestId, x.MaintenanceTypeId });
                    table.ForeignKey(
                        name: "FK_ServiceRequestService_ServiceRequest_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequestService_maintenanceTypes_MaintenanceTypeId",
                        column: x => x.MaintenanceTypeId,
                        principalTable: "maintenanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequest_AppointmentId",
                table: "ServiceRequest",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestService_MaintenanceTypeId",
                table: "ServiceRequestService",
                column: "MaintenanceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequest_AspNetUsers_UserId",
                table: "ServiceRequest",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
