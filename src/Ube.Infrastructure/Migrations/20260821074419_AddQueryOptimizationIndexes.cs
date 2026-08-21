using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryOptimizationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_VendorApplications_Status_SubmittedAt",
                table: "VendorApplications",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_OriginalCategoryName",
                table: "Listings",
                column: "OriginalCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CreatedAt",
                table: "Bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId_CreatedAt",
                table: "Bookings",
                columns: new[] { "CustomerId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VendorApplications_Status_SubmittedAt",
                table: "VendorApplications");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Listings_OriginalCategoryName",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CreatedAt",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerId_CreatedAt",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings",
                column: "CustomerId");
        }
    }
}
