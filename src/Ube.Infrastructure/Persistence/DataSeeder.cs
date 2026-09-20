using Ube.Domain.Constants;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ube.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // Ensure the hidden __Uncategorized__ category exists for orphan management
        var uncategorized = await db.Categories
            .FirstOrDefaultAsync(c => c.Name == CategoryConstants.UncategorizedName);

        if (uncategorized is null)
        {
            uncategorized = new Category
            {
                Id = Guid.NewGuid(),
                Name = CategoryConstants.UncategorizedName,
                Status = RecordStatus.Inactive,
                DisplayOrder = int.MaxValue,
            };
            db.Categories.Add(uncategorized);
            await db.SaveChangesAsync();
        }

        // Build category name → ID map (active categories only)
        var categoryMap = await db.Categories
            .Where(c => c.Status != RecordStatus.Deleted && c.Name != CategoryConstants.UncategorizedName)
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        // Re-link any listings currently parked in __Uncategorized__
        var orphanedListings = await db.Listings
            .Where(l => l.CategoryId == uncategorized.Id && l.OriginalCategoryName != null)
            .ToListAsync();

        foreach (var listing in orphanedListings)
        {
            if (listing.OriginalCategoryName is null) continue;
            if (!categoryMap.TryGetValue(listing.OriginalCategoryName, out var correctCatId)) continue;

            listing.CategoryId = correctCatId;
            listing.IsActive = true;
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        // Demo listings are no longer inserted; preserve vendor-created listings.
    }
}
