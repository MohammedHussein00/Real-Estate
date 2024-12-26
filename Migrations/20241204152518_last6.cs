using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Real_State.Migrations
{
    /// <inheritdoc />
    public partial class last6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BroadbandInternet",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CCTVSecurity",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CleaningServices",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ElectricityBackup",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Garden",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "GymOrHealthClub",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "MaintenanceStaff",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ParkingSpace",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Reception",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SecurityStaff",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SwimmingPool",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "WasteDisposal",
                table: "Properties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BroadbandInternet",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CCTVSecurity",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CleaningServices",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ElectricityBackup",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Garden",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GymOrHealthClub",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MaintenanceStaff",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParkingSpace",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Reception",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SecurityStaff",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SwimmingPool",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WasteDisposal",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
