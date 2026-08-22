using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAllListingFieldsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReservationRules",
                table: "RestaurantListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableTypes",
                table: "RestaurantListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryRoomType",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "EventListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketTypesJson",
                table: "EventListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VenueAddress",
                table: "EventListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VenueName",
                table: "EventListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "CarRentalListingDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceOptions",
                table: "CarRentalListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupLocation",
                table: "CarRentalListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnLocation",
                table: "CarRentalListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "CarRentalListingDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationRules",
                table: "RestaurantListingDetails");

            migrationBuilder.DropColumn(
                name: "TableTypes",
                table: "RestaurantListingDetails");

            migrationBuilder.DropColumn(
                name: "PrimaryRoomType",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "EventListingDetails");

            migrationBuilder.DropColumn(
                name: "TicketTypesJson",
                table: "EventListingDetails");

            migrationBuilder.DropColumn(
                name: "VenueAddress",
                table: "EventListingDetails");

            migrationBuilder.DropColumn(
                name: "VenueName",
                table: "EventListingDetails");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "CarRentalListingDetails");

            migrationBuilder.DropColumn(
                name: "InsuranceOptions",
                table: "CarRentalListingDetails");

            migrationBuilder.DropColumn(
                name: "PickupLocation",
                table: "CarRentalListingDetails");

            migrationBuilder.DropColumn(
                name: "ReturnLocation",
                table: "CarRentalListingDetails");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CarRentalListingDetails");
        }
    }
}
