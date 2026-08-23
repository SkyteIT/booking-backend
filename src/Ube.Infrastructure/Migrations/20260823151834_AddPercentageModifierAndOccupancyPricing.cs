using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPercentageModifierAndOccupancyPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPercentageModifier",
                table: "ListingOptionValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BaseOccupancy",
                table: "HotelListingDetails",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "OccupancyPriceModifier",
                table: "HotelListingDetails",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPercentageModifier",
                table: "ListingOptionValues");

            migrationBuilder.DropColumn(
                name: "BaseOccupancy",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "OccupancyPriceModifier",
                table: "HotelListingDetails");
        }
    }
}
