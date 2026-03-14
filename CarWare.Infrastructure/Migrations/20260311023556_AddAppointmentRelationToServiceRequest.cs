using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentRelationToServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "ServiceRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceDate",
                table: "ServiceRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequest_AppointmentId",
                table: "ServiceRequest",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequest_Appointments_AppointmentId",
                table: "ServiceRequest",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequest_Appointments_AppointmentId",
                table: "ServiceRequest");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequest_AppointmentId",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "ServiceRequest");

            migrationBuilder.DropColumn(
                name: "ServiceDate",
                table: "ServiceRequest");
        }
    }
}
