using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;

namespace Ube.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(UbeDbContext db)
    {
        // --- Seed Categories ---
        var categoryNames = new[]
        {
            "Hotels", "Restaurants", "Events",
            "Activities", "Car Rentals", "Apartments", "Equipment"
        };

        var existingNames = await db.Categories
            .Select(c => c.Name)
            .ToHashSetAsync();

        foreach (var name in categoryNames)
        {
            if (!existingNames.Contains(name))
            {
                db.Categories.Add(new Category
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Status = RecordStatus.Active,
                });
            }
        }

        await db.SaveChangesAsync();

        // --- Load category name→id map ---
        var categoryMap = await db.Categories
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        // --- Seed Listings ---
        var existingTitles = await db.Listings
            .Select(l => l.Title)
            .ToHashSetAsync();

        var listings = new[]
        {
            new { Title = "Araliya Beach Resort",      Category = "Hotels",      Location = "Sri Lanka",    Price = 350m, Rating = 4.9m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=900" },
            new { Title = "City Center Apartment",     Category = "Apartments",  Location = "Paris",        Price = 120m, Rating = 4.5m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=900" },
            new { Title = "Desert Safari Adventure",   Category = "Activities",  Location = "Dubai",        Price = 85m,  Rating = 4.8m, Available = true,  Thumbnail = "https://plus.unsplash.com/premium_photo-1661963573455-ba0446e2cab9?w=900" },
            new { Title = "Kayaking Kitulgala",        Category = "Activities",  Location = "Kitulgala",    Price = 50m,  Rating = 4.7m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=900" },
            new { Title = "Ocean View Restaurant",     Category = "Restaurants", Location = "Miami",        Price = 45m,  Rating = 4.6m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=900" },
            new { Title = "Grand Plaza Hotel",         Category = "Hotels",      Location = "New York",     Price = 220m, Rating = 4.4m, Available = false, Thumbnail = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=900" },
            new { Title = "Jazz Night Live Event",     Category = "Events",      Location = "New Orleans",  Price = 35m,  Rating = 4.8m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=900" },
            new { Title = "BMW 5 Series Rental",       Category = "Car Rentals", Location = "Los Angeles",  Price = 95m,  Rating = 4.7m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=900" },
            new { Title = "Mountain Escape Cabin",     Category = "Apartments",  Location = "Aspen",        Price = 180m, Rating = 4.3m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1510798831971-661eb04b3739?w=900" },
            new { Title = "Snorkeling Reef Experience",Category = "Activities",  Location = "Bali",         Price = 70m,  Rating = 4.9m, Available = true,  Thumbnail = "https://plus.unsplash.com/premium_photo-1661894232140-73d96a67731b?w=900" },
            new { Title = "Professional Camera Kit",   Category = "Equipment",   Location = "London",       Price = 40m,  Rating = 4.2m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1516724562728-afc824a36e84?w=900" },
            new { Title = "Boutique Riverside Hotel",  Category = "Hotels",      Location = "Amsterdam",    Price = 160m, Rating = 4.6m, Available = true,  Thumbnail = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=900" },
        };

        foreach (var item in listings)
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