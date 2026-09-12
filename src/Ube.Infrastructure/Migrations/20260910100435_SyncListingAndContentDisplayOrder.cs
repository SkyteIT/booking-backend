using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncListingAndContentDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some development databases predate these columns while their
            // migration history says the creating migrations were applied.
            // Repair those databases without breaking correctly migrated ones.
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Categories', 'DisplayOrder') IS NULL
                    ALTER TABLE dbo.Categories ADD DisplayOrder int NOT NULL
                        CONSTRAINT DF_Categories_DisplayOrder DEFAULT(0);

                IF COL_LENGTH('dbo.ListingUnits', 'DisplayOrder') IS NULL
                    ALTER TABLE dbo.ListingUnits ADD DisplayOrder int NOT NULL
                        CONSTRAINT DF_ListingUnits_DisplayOrder DEFAULT(0);

                IF COL_LENGTH('dbo.CategoryCustomFields', 'DisplayOrder') IS NULL
                    ALTER TABLE dbo.CategoryCustomFields ADD DisplayOrder int NOT NULL
                        CONSTRAINT DF_CategoryCustomFields_DisplayOrder DEFAULT(0);

                IF COL_LENGTH('dbo.Banners', 'DisplayOrder') IS NULL
                    ALTER TABLE dbo.Banners ADD DisplayOrder int NOT NULL
                        CONSTRAINT DF_Banners_DisplayOrder DEFAULT(0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally retained: these columns are part of the model and
            // may have existed before this repair migration was applied.
        }
    }
}
