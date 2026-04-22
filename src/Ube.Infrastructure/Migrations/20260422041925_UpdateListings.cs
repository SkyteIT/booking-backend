using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "Listings",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "HotelListingDetails",
                newName: "PricePerNight");

            migrationBuilder.RenameColumn(
                name: "EventDate",
                table: "EventListingDetails",
                newName: "DateAndTime");

            migrationBuilder.RenameColumn(
                name: "DurationHours",
                table: "EventListingDetails",
                newName: "SeatCount");

            migrationBuilder.RenameColumn(
                name: "PricePerPerson",
                table: "ActivityListingDetails",
                newName: "Price");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "RestaurantListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AvailableRooms",
                table: "HotelListingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckInTime",
                table: "HotelListingDetails",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckOutTime",
                table: "HotelListingDetails",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "EventListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "EventListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityStatus",
                table: "CarRentalListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "CarRentalListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "ActivityListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "RestaurantListingDetails");

            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "AvailableRooms",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "EventListingDetails");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "EventListingDetails");

            migrationBuilder.DropColumn(
                name: "AvailabilityStatus",
                table: "CarRentalListingDetails");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "CarRentalListingDetails");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "ActivityListingDetails");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Listings",
                newName: "BasePrice");

            migrationBuilder.RenameColumn(
                name: "PricePerNight",
                table: "HotelListingDetails",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "SeatCount",
                table: "EventListingDetails",
                newName: "DurationHours");

            migrationBuilder.RenameColumn(
                name: "DateAndTime",
                table: "EventListingDetails",
                newName: "EventDate");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ActivityListingDetails",
                newName: "PricePerPerson");
        }
    }
}
