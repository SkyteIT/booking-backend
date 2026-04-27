using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityDetailsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ActivityListingDetails",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "DifficultyLevel",
                table: "ActivityListingDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilitySchedule",
                table: "ActivityListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncludedServices",
                table: "ActivityListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAge",
                table: "ActivityListingDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxGroupSize",
                table: "ActivityListingDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAge",
                table: "ActivityListingDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinGroupSize",
                table: "ActivityListingDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyRequirements",
                table: "ActivityListingDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilitySchedule",
                table: "ActivityListingDetails");

            migrationBuilder.DropColumn(
                name: "IncludedServices",
                table: "ActivityListingDetails");

            migrationBuilder.DropColumn(
                name: "MaxAge",
                table: "ActivityListingDetails");

            migrationBuilder.DropColumn(
                name: "MaxGroupSize",
                table: "ActivityListingDetails");

            migrationBuilder.DropColumn(
                name: "MinAge",
                table: "ActivityListingDetails");

            migrationBuilder.DropColumn(
                name: "MinGroupSize",
                table: "ActivityListingDetails");

            migrationBuilder.DropColumn(
                name: "SafetyRequirements",
                table: "ActivityListingDetails");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ActivityListingDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DifficultyLevel",
                table: "ActivityListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
