using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeVendorApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessLicenseUrl",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "InsurenceCertificateUrl",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "TaxDocumentUrl",
                table: "VendorApplications");

            migrationBuilder.RenameColumn(
                name: "ContactNumber",
                table: "VendorApplications",
                newName: "Phone");

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicensePath",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categories",
                table: "VendorApplications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VendorApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "VendorApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "VendorApplications",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "VendorApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCertificatePath",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "VendorApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxDocumentPath",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "VendorApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VendorApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "VendorApplications",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessLicensePath",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "Categories",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "InsuranceCertificatePath",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "TaxDocumentPath",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "VendorApplications");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "VendorApplications",
                newName: "ContactNumber");

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicenseUrl",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                table: "VendorApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsurenceCertificateUrl",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxDocumentUrl",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
