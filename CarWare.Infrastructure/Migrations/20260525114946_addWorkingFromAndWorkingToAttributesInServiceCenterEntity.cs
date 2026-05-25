using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addWorkingFromAndWorkingToAttributesInServiceCenterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkingFrom",
                table: "ServiceCenters",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkingTo",
                table: "ServiceCenters",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "ServiceCenters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "WorkingFrom", "WorkingTo" },
                values: new object[] { new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ServiceCenters",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "WorkingFrom", "WorkingTo" },
                values: new object[] { new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ServiceCenters",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "WorkingFrom", "WorkingTo" },
                values: new object[] { new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ServiceCenters",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "WorkingFrom", "WorkingTo" },
                values: new object[] { new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ServiceCenters",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "WorkingFrom", "WorkingTo" },
                values: new object[] { new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkingFrom",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "WorkingTo",
                table: "ServiceCenters");
        }
    }
}
