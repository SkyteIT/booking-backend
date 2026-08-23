using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations;

public partial class FixMissingBannerDisplayOrder : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Banners', 'DisplayOrder') IS NULL
BEGIN
    ALTER TABLE dbo.Banners
    ADD DisplayOrder int NOT NULL CONSTRAINT DF_Banners_DisplayOrder DEFAULT(0);
END;
");

        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Banners_Placement_Status_StartDate_EndDate_DisplayOrder'
      AND object_id = OBJECT_ID('dbo.Banners')
)
BEGIN
    CREATE INDEX IX_Banners_Placement_Status_StartDate_EndDate_DisplayOrder
    ON dbo.Banners (Placement, Status, StartDate, EndDate, DisplayOrder);
END;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Banners_Placement_Status_StartDate_EndDate_DisplayOrder'
      AND object_id = OBJECT_ID('dbo.Banners')
)
BEGIN
    DROP INDEX IX_Banners_Placement_Status_StartDate_EndDate_DisplayOrder ON dbo.Banners;
END;
");

        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Banners', 'DisplayOrder') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Banners DROP CONSTRAINT DF_Banners_DisplayOrder;
    ALTER TABLE dbo.Banners DROP COLUMN DisplayOrder;
END;
");
    }
}
