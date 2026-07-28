using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerVendorCommissionInvoiceLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VendorCommissionInvoiceId",
                table: "LedgerEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_VendorCommissionInvoiceId",
                table: "LedgerEntries",
                column: "VendorCommissionInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_VendorCommissionInvoiceId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "VendorCommissionInvoiceId",
                table: "LedgerEntries");
        }
    }
}
