using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MG5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "VendorApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxDocumentUrl",
                table: "VendorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "VendorApplications");

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
                name: "ReviewedBy",
                table: "VendorApplications");

            migrationBuilder.DropColumn(
                name: "TaxDocumentUrl",
                table: "VendorApplications");
        }
    }
}
