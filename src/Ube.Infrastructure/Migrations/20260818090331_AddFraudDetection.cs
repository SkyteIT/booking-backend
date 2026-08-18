using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFraudDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guarded rather than a plain AddColumn - this dev database
            // already has this column from an earlier partial migration
            // attempt; a fresh database will still get it added normally.
            migrationBuilder.Sql(@"
IF COL_LENGTH('Bookings', 'IsHeldForFraudReview') IS NULL
BEGIN
    ALTER TABLE [Bookings] ADD [IsHeldForFraudReview] bit NOT NULL DEFAULT CAST(0 AS bit);
END");

            // Same guard as above - this dev database already has this
            // table (and its indexes) from the same earlier partial attempt.
            migrationBuilder.Sql(@"
IF OBJECT_ID('FraudFlags') IS NULL
BEGIN
    CREATE TABLE [FraudFlags] (
        [Id] uniqueidentifier NOT NULL,
        [BookingId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [RuleTriggered] int NOT NULL,
        [Severity] int NOT NULL,
        [Details] nvarchar(500) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ReviewedByUserId] uniqueidentifier NULL,
        [ReviewedAt] datetime2 NULL,
        [ReviewNotes] nvarchar(500) NULL,
        [ResolvedStatusOnClear] int NULL,
        [CollectionMethodOnClear] int NULL,
        CONSTRAINT [PK_FraudFlags] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FraudFlags_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FraudFlags_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FraudFlags_BookingId' AND object_id = OBJECT_ID('FraudFlags'))
    CREATE INDEX [IX_FraudFlags_BookingId] ON [FraudFlags] ([BookingId]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FraudFlags_CustomerId' AND object_id = OBJECT_ID('FraudFlags'))
    CREATE INDEX [IX_FraudFlags_CustomerId] ON [FraudFlags] ([CustomerId]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FraudFlags_Status' AND object_id = OBJECT_ID('FraudFlags'))
    CREATE INDEX [IX_FraudFlags_Status] ON [FraudFlags] ([Status]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FraudFlags");

            migrationBuilder.DropColumn(
                name: "IsHeldForFraudReview",
                table: "Bookings");
        }
    }
}
