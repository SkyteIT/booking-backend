using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Categories_CategoryId1",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_VendorProfiles_VendorId1",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorProfiles_Users_UserId1",
                table: "VendorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_VendorProfiles_UserId1",
                table: "VendorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Listings_CategoryId1",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_VendorId1",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "VendorProfiles");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "VendorId1",
                table: "Listings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "VendorProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId1",
                table: "Listings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId1",
                table: "Listings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_UserId1",
                table: "VendorProfiles",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_CategoryId1",
                table: "Listings",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_VendorId1",
                table: "Listings",
                column: "VendorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Categories_CategoryId1",
                table: "Listings",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_VendorProfiles_VendorId1",
                table: "Listings",
                column: "VendorId1",
                principalTable: "VendorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorProfiles_Users_UserId1",
                table: "VendorProfiles",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
