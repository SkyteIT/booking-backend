using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Listings: Rename Price to BasePrice
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Listings",
                newName: "BasePrice");

            // 2. HotelListingDetails: Add missing columns and drop old ones
            migrationBuilder.AddColumn<decimal>(name: "PricePerNight", table: "HotelListingDetails", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<int>(name: "AvailableRooms", table: "HotelListingDetails", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "Amenities", table: "HotelListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "RoomTypes", table: "HotelListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "CheckInTime", table: "HotelListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "CheckOutTime", table: "HotelListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");
            
            migrationBuilder.DropColumn(name: "Location", table: "HotelListingDetails");
            migrationBuilder.DropColumn(name: "Price", table: "HotelListingDetails");

            // 3. ActivityListingDetails: Rename PricePerPerson to Price
            migrationBuilder.RenameColumn(
                name: "PricePerPerson",
                table: "ActivityListingDetails",
                newName: "Price");

            // 4. CarRentalListingDetails: Add missing columns
            migrationBuilder.AddColumn<string>(name: "FuelType", table: "CarRentalListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "AvailabilityStatus", table: "CarRentalListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");

            // 5. EventListingDetails: Rename and add
            migrationBuilder.RenameColumn(
                name: "EventDate",
                table: "EventListingDetails",
                newName: "DateAndTime");
            
            migrationBuilder.AddColumn<string>(name: "EventName", table: "EventListingDetails", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<int>(name: "SeatCount", table: "EventListingDetails", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.DropColumn(name: "DurationHours", table: "EventListingDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
