using Ube.Domain.Constants;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ube.Infrastructure.Persistence;

public static class DataSeeder
{
    // Internal sentinel category name used to hold listings whose real
    // category was deleted. Never shown in the UI (status = Inactive).
    // UncategorizedName is defined in Ube.Domain.Constants.CategoryConstants
    // Kept here for backwards compatibility
    public const string UncategorizedName = CategoryConstants.UncategorizedName;

    private record SeedListing(
        string Title, string Category, string Location,
        decimal Price, decimal Rating, bool Available, string Thumbnail);

    private static readonly SeedListing[] Listings =
    [
        new("Araliya Beach Resort",       "Hotels",      "Sri Lanka",   350m, 4.9m, true,  "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=900"),
        new("City Center Apartment",      "Apartments",  "Paris",       120m, 4.5m, true,  "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=900"),
        new("Desert Safari Adventure",    "Activities",  "Dubai",        85m, 4.8m, true,  "https://plus.unsplash.com/premium_photo-1661963573455-ba0446e2cab9?w=900"),
        new("Kayaking Kitulgala",         "Activities",  "Kitulgala",    50m, 4.7m, true,  "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=900"),
        new("Ocean View Restaurant",      "Restaurants", "Miami",        45m, 4.6m, true,  "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=900"),
        new("Grand Plaza Hotel",          "Hotels",      "New York",    220m, 4.4m, false, "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=900"),
        new("Jazz Night Live Event",      "Events",      "New Orleans",  35m, 4.8m, true,  "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=900"),
        new("BMW 5 Series Rental",        "Car Rentals", "Los Angeles",  95m, 4.7m, true,  "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=900"),
        new("Mountain Escape Cabin",      "Apartments",  "Aspen",       180m, 4.3m, true,  "https://images.unsplash.com/photo-1510798831971-661eb04b3739?w=900"),
        new("Snorkeling Reef Experience", "Activities",  "Bali",         70m, 4.9m, true,  "https://plus.unsplash.com/premium_photo-1661894232140-73d96a67731b?w=900"),
        new("Professional Camera Kit",    "Equipment",   "London",       40m, 4.2m, true,  "https://images.unsplash.com/photo-1516724562728-afc824a36e84?w=900"),
        new("Boutique Riverside Hotel",   "Hotels",      "Amsterdam",   160m, 4.6m, true,  "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=900"),
    ];

    public static async Task SeedAsync(UbeDbContext db)
    {
        // ── Step 1: Ensure the hidden __Uncategorized__ category exists ──────
        // This is used as a safe holding place for listings whose real category
        // was deleted. It is never shown in the admin UI or public search.
        var uncategorized = await db.Categories
            .FirstOrDefaultAsync(c => c.Name == UncategorizedName);

        if (uncategorized is null)
        {
            uncategorized = new Category
            {
                Id = Guid.NewGuid(),
                Name = UncategorizedName,
                Status = RecordStatus.Inactive, // hidden from UI and search
                DisplayOrder = int.MaxValue,
            };
            db.Categories.Add(uncategorized);
            await db.SaveChangesAsync();
        }

        // ── Step 2: Build category name → ID map (active categories only) ───
        var categoryMap = await db.Categories
            .Where(c => c.Status != RecordStatus.Deleted && c.Name != UncategorizedName)
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        // ── Step 3: Re-link any listings currently parked in __Uncategorized__
        // This runs every startup so that: delete Hotels → restart → re-add Hotels
        // → restart again will re-link the hotel listings automatically.
        var orphanedListings = await db.Listings
            .Where(l => l.CategoryId == uncategorized.Id)
            .ToListAsync();

        // Build a title → desired category name lookup from our seed data
        var titleToCategoryName = Listings.ToDictionary(l => l.Title, l => l.Category);

        foreach (var listing in orphanedListings)
        {
            if (!titleToCategoryName.TryGetValue(listing.Title, out var desiredCategoryName))
                continue;

            if (!categoryMap.TryGetValue(desiredCategoryName, out var correctCatId))
                continue; // desired category still doesn't exist yet — leave it parked

            // Re-link to the correct category and make it active again
            listing.CategoryId = correctCatId;
            listing.Status = RecordStatus.Active;
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        // ── Step 4: Insert missing seed listings ─────────────────────────────
        var existingTitles = await db.Listings
            .Select(l => l.Title)
            .ToHashSetAsync();

        foreach (var item in Listings)
        {
            if (existingTitles.Contains(item.Title)) continue;

            if (!categoryMap.TryGetValue(item.Category, out var catId)) continue;

            db.Listings.Add(new Listing
            {
                Id = Guid.NewGuid(),
                Title = item.Title,
                CategoryId = catId,
                Location = item.Location,
                PriceFrom = item.Price,
                Rating = item.Rating,
                IsAvailable = item.Available,
                IsFeatured = false,
                ThumbnailUrl = item.Thumbnail,
                Status = RecordStatus.Active,
            });
        }

        await db.SaveChangesAsync();
    }
}