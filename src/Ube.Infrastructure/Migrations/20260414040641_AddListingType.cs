using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotelListingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfRooms = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    RoomTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amenities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancellationPolicy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelListingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelListingDetails_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HotelListingDetails_ListingId",
                table: "HotelListingDetails",
                column: "ListingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotelListingDetails");
        }
    }
}
