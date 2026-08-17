using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingUnitsAndCategoryEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceModel",
                table: "Categories",
                type: "int",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BookingType",
                table: "Categories",
                type: "int",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentCollectionModel",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ListingUnitId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ListingUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PriceOverride = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: true),
                    ColumnIndex = table.Column<int>(type: "int", nullable: true),
                    SlotStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SlotDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingUnits_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ListingUnitId",
                table: "Bookings",
                column: "ListingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingUnits_ListingId",
                table: "ListingUnits",
                column: "ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ListingUnits_ListingUnitId",
                table: "Bookings",
                column: "ListingUnitId",
                principalTable: "ListingUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ListingUnits_ListingUnitId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "ListingUnits");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ListingUnitId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentCollectionModel",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ListingUnitId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceModel",
                table: "Categories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookingType",
                table: "Categories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
