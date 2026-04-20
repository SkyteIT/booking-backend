using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "CancellationPolicy",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "RoomTypes",
                table: "HotelListingDetails");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "HotelListingDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "HotelListingDetails");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "HotelListingDetails");

            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationPolicy",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckInTime",
                table: "HotelListingDetails",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckOutTime",
                table: "HotelListingDetails",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "HotelListingDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomTypes",
                table: "HotelListingDetails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
