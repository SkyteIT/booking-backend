using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Banners_Placement_Status_StartDate_EndDate",
                table: "Banners");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Banners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Placement_Status_StartDate_EndDate_DisplayOrder",
                table: "Banners",
                columns: new[] { "Placement", "Status", "StartDate", "EndDate", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Banners_Placement_Status_StartDate_EndDate_DisplayOrder",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Banners");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Placement_Status_StartDate_EndDate",
                table: "Banners",
                columns: new[] { "Placement", "Status", "StartDate", "EndDate" });
        }
    }
}
