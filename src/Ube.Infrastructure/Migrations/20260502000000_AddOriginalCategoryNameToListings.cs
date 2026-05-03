using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ube.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class AddOriginalCategoryNameToListings : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "OriginalCategoryName",
				table: "Listings",
				type: "nvarchar(150)",
				maxLength: 150,
				nullable: true);

			// Back-fill: set OriginalCategoryName from the joined Category name
			// for all existing listings that already have a valid category.
			migrationBuilder.Sql(@"
                UPDATE l
                SET l.OriginalCategoryName = c.Name
                FROM Listings l
                INNER JOIN Categories c ON l.CategoryId = c.Id
                WHERE c.Name <> '__Uncategorized__'
                  AND l.OriginalCategoryName IS NULL
            ");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "OriginalCategoryName",
				table: "Listings");
		}
	}
}