using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixListingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_VendorProfiles_VendorId",
                table: "Listings");

            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "Listings",
                newName: "VendorProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Listings_VendorId",
                table: "Listings",
                newName: "IX_Listings_VendorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_VendorProfiles_VendorProfileId",
                table: "Listings",
                column: "VendorProfileId",
                principalTable: "VendorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_VendorProfiles_VendorProfileId",
                table: "Listings");

            migrationBuilder.RenameColumn(
                name: "VendorProfileId",
                table: "Listings",
                newName: "VendorId");

            migrationBuilder.RenameIndex(
                name: "IX_Listings_VendorProfileId",
                table: "Listings",
                newName: "IX_Listings_VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_VendorProfiles_VendorId",
                table: "Listings",
                column: "VendorId",
                principalTable: "VendorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
