using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderRecurrenceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextDueDate",
                table: "maintenances");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "maintenances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatCount",
                table: "maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatInterval",
                table: "maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatUnit",
                table: "maintenances",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "maintenances");

            migrationBuilder.DropColumn(
                name: "RepeatCount",
                table: "maintenances");

            migrationBuilder.DropColumn(
                name: "RepeatInterval",
                table: "maintenances");

            migrationBuilder.DropColumn(
                name: "RepeatUnit",
                table: "maintenances");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextDueDate",
                table: "maintenances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
