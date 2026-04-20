using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllListingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityListingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricePerPerson = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityListingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityListingDetails_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarRentalListingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatCount = table.Column<int>(type: "int", nullable: false),
                    Transmission = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarRentalListingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarRentalListingDetails_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventListingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    TicketPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Organizer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventListingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventListingDetails_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantListingDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuisineType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableCapacity = table.Column<int>(type: "int", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningHours = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantListingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantListingDetails_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityListingDetails_ListingId",
                table: "ActivityListingDetails",
                column: "ListingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarRentalListingDetails_ListingId",
                table: "CarRentalListingDetails",
                column: "ListingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventListingDetails_ListingId",
                table: "EventListingDetails",
                column: "ListingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantListingDetails_ListingId",
                table: "RestaurantListingDetails",
                column: "ListingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityListingDetails");

            migrationBuilder.DropTable(
                name: "CarRentalListingDetails");

            migrationBuilder.DropTable(
                name: "EventListingDetails");

            migrationBuilder.DropTable(
                name: "RestaurantListingDetails");
        }
    }
}
