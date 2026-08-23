using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Listings;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;

namespace Ube.Infrastructure.Persistence.Seed;

public static class TestDataSeeder
{
    public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa");
    public static readonly Guid SuperAdminUserId = Guid.Parse("99999999-0000-0000-0000-999999999999");
    public static readonly Guid VendorUserId = Guid.Parse("bbbbbbbb-0000-0000-0000-bbbbbbbbbbbb");
    public static readonly Guid VendorProfileId = Guid.Parse("bbbbbbbb-1111-1111-1111-bbbbbbbbbbbb");

    public static readonly Guid Customer1Id = Guid.Parse("cccccccc-0000-0000-0000-cccccccccccc");
    public static readonly Guid Customer2Id = Guid.Parse("cccccccc-1111-1111-1111-cccccccccccc");
    public static readonly Guid Customer3Id = Guid.Parse("cccccccc-2222-2222-2222-cccccccccccc");
    public static readonly Guid Customer4Id = Guid.Parse("cccccccc-6666-6666-6666-cccccccccccc");
    public static readonly Guid Customer5Id = Guid.Parse("cccccccc-7777-7777-7777-cccccccccccc");

    public static readonly Guid CategoryId = Guid.Parse("dddddddd-0000-0000-0000-dddddddddddd");

    // One category per ListingType so search/filter/category-tile UI has a real
    // spread of data to work against, not just the one Photography vendor.
    public static readonly Guid CategoryHotelId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid CategoryRestaurantId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid CategoryEventId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid CategoryCarRentalId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid CategoryActivityId = Guid.Parse("10000000-0000-0000-0000-000000000005");

    public static readonly Guid Banner1Id = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid Banner2Id = Guid.Parse("30000000-0000-0000-0000-000000000002");

    public static readonly Guid Promotion1Id = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid Promotion2Id = Guid.Parse("40000000-0000-0000-0000-000000000002");

    public static readonly Guid Listing1Id = Guid.Parse("eeeeeeee-0000-0000-0000-eeeeeeeeeeee");
    public static readonly Guid Listing2Id = Guid.Parse("eeeeeeee-1111-1111-1111-eeeeeeeeeeee");
    public static readonly Guid Listing3Id = Guid.Parse("eeeeeeee-2222-2222-2222-eeeeeeeeeeee");
    public static readonly Guid Listing4Id = Guid.Parse("eeeeeeee-3333-3333-3333-eeeeeeeeeeee");
    public static readonly Guid Listing5Id = Guid.Parse("eeeeeeee-4444-4444-4444-eeeeeeeeeeee");

    // 2 listings per new type-scoped category (see CategoryHotelId etc. above).
    public static readonly Guid ListingHotel1Id = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid ListingHotel2Id = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid ListingRestaurant1Id = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid ListingRestaurant2Id = Guid.Parse("20000000-0000-0000-0000-000000000004");
    public static readonly Guid ListingEvent1Id = Guid.Parse("20000000-0000-0000-0000-000000000005");
    public static readonly Guid ListingEvent2Id = Guid.Parse("20000000-0000-0000-0000-000000000006");
    public static readonly Guid ListingCarRental1Id = Guid.Parse("20000000-0000-0000-0000-000000000007");
    public static readonly Guid ListingCarRental2Id = Guid.Parse("20000000-0000-0000-0000-000000000008");
    public static readonly Guid ListingActivity1Id = Guid.Parse("20000000-0000-0000-0000-000000000009");
    public static readonly Guid ListingActivity2Id = Guid.Parse("20000000-0000-0000-0000-00000000000a");

    public static readonly Guid VendorPayoutId = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb");
    public static readonly Guid MainVendorApplicationId = Guid.Parse("bbbbbbbb-4444-4444-4444-bbbbbbbbbbbb");

    public static readonly Guid VendorApplication1Id = Guid.Parse("dddddddd-1111-1111-1111-dddddddddddd");
    public static readonly Guid VendorApplication2Id = Guid.Parse("dddddddd-2222-2222-2222-dddddddddddd");
    public static readonly Guid VendorApplication3Id = Guid.Parse("dddddddd-3333-3333-3333-dddddddddddd");
    public static readonly Guid VendorApplication4Id = Guid.Parse("dddddddd-4444-4444-4444-dddddddddddd");
    public static readonly Guid VendorApplication5Id = Guid.Parse("dddddddd-5555-5555-5555-dddddddddddd");

    public static readonly Guid AdminLocalizationId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");
    public static readonly Guid SuperAdminLocalizationId = Guid.Parse("99999999-1111-1111-1111-999999999999");
    public static readonly Guid VendorLocalizationId = Guid.Parse("bbbbbbbb-3333-3333-3333-bbbbbbbbbbbb");
    public static readonly Guid Customer1LocalizationId = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc");
    public static readonly Guid Customer2LocalizationId = Guid.Parse("cccccccc-4444-4444-4444-cccccccccccc");
    public static readonly Guid Customer3LocalizationId = Guid.Parse("cccccccc-5555-5555-5555-cccccccccccc");
    public static readonly Guid Customer4LocalizationId = Guid.Parse("cccccccc-6666-6666-6666-cccccccccccc");
    public static readonly Guid Customer5LocalizationId = Guid.Parse("cccccccc-7777-7777-7777-cccccccccccc");

    public static readonly Guid[,] BookingIds = new Guid[3, 5]
    {
        {
            Guid.Parse("f0000000-0000-0000-0000-000000000001"),
            Guid.Parse("f0000000-0000-0000-0000-000000000002"),
            Guid.Parse("f0000000-0000-0000-0000-000000000003"),
            Guid.Parse("f0000000-0000-0000-0000-000000000004"),
            Guid.Parse("f0000000-0000-0000-0000-000000000005")
        },
        {
            Guid.Parse("f0000000-0000-0000-0000-000000000006"),
            Guid.Parse("f0000000-0000-0000-0000-000000000007"),
            Guid.Parse("f0000000-0000-0000-0000-000000000008"),
            Guid.Parse("f0000000-0000-0000-0000-000000000009"),
            Guid.Parse("f0000000-0000-0000-0000-000000000010")
        },
        {
            Guid.Parse("f0000000-0000-0000-0000-000000000011"),
            Guid.Parse("f0000000-0000-0000-0000-000000000012"),
            Guid.Parse("f0000000-0000-0000-0000-000000000013"),
            Guid.Parse("f0000000-0000-0000-0000-000000000014"),
            Guid.Parse("f0000000-0000-0000-0000-000000000015")
        }
    };

    public const string SeedPassword = "SecurePassword123!";

    public static async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var now = DateTime.UtcNow;

        UpsertUser(dbContext, AdminUserId, UserRole.Admin, "admin@ube.local", "System", "Admin", "+94770000000", true, now, cancellationToken);
        UpsertUser(dbContext, SuperAdminUserId, UserRole.SuperAdmin, "superadmin@ube.local", "System", "SuperAdmin", "+94770000099", true, now, cancellationToken);
        UpsertUser(dbContext, VendorUserId, UserRole.Vendor, "vendor@ube.local", "Main", "Vendor", "+94770000001", true, now, cancellationToken);
        UpsertUser(dbContext, Customer1Id, UserRole.User, "customer1@ube.local", "Customer", "One", "+94770000011", true, now, cancellationToken);
        UpsertUser(dbContext, Customer2Id, UserRole.User, "customer2@ube.local", "Customer", "Two", "+94770000012", true, now, cancellationToken);
        UpsertUser(dbContext, Customer3Id, UserRole.User, "customer3@ube.local", "Customer", "Three", "+94770000013", true, now, cancellationToken);
        UpsertUser(dbContext, Customer4Id, UserRole.User, "customer4@ube.local", "Customer", "Four", "+94770000014", true, now, cancellationToken);
        UpsertUser(dbContext, Customer5Id, UserRole.User, "customer5@ube.local", "Customer", "Five", "+94770000015", true, now, cancellationToken);

        // Ensure each user has localization settings
        UpsertUserLocalization(dbContext, AdminLocalizationId, AdminUserId, "en", "UTC", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, SuperAdminLocalizationId, SuperAdminUserId, "en", "UTC", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, VendorLocalizationId, VendorUserId, "si-LK", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer1LocalizationId, Customer1Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer2LocalizationId, Customer2Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer3LocalizationId, Customer3Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer4LocalizationId, Customer4Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer5LocalizationId, Customer5Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);

        UpsertVendorProfile(dbContext, VendorProfileId, VendorUserId, "Main Vendor Studio", "Photography", "Simple seeded vendor profile.", "+94770000001", now, cancellationToken);
        // Add vendor payout defaults
        UpsertVendorPayout(dbContext, VendorPayoutId, VendorProfileId, "Seed Bank", "000123456789", "Main Vendor", "Colombo Branch", now, cancellationToken);
        // "Photography" doesn't map cleanly onto any fixed ListingType, but of the
        // 5 it's closest to a bookable Activity, so that's what its listings use.
        UpsertCategory(dbContext, CategoryId, "Photography", "Seed category for all listings.", ListingType.Activity, BookingConfirmationType.Request, now, cancellationToken);

        UpsertCategory(dbContext, CategoryHotelId, "Hotels & Resorts", "Stays across hotels, apartments, and resorts.", ListingType.Hotel, BookingConfirmationType.Request, now, cancellationToken);
        UpsertCategory(dbContext, CategoryRestaurantId, "Restaurants & Dining", "Table reservations at restaurants and cafes.", ListingType.Restaurant, BookingConfirmationType.Request, now, cancellationToken);
        // Tickets are a quick, fixed-inventory purchase, not a stay/table a
        // vendor needs to individually review - Instant here means a ticket
        // buyer gets a Confirmed booking immediately instead of sitting
        // Pending indefinitely awaiting manual approval.
        UpsertCategory(dbContext, CategoryEventId, "Events & Tickets", "Concerts, sports, and theater tickets.", ListingType.Event, BookingConfirmationType.Instant, now, cancellationToken);
        UpsertCategory(dbContext, CategoryCarRentalId, "Car Rentals", "Self-drive and chauffeur car rentals.", ListingType.CarRental, BookingConfirmationType.Request, now, cancellationToken);
        UpsertCategory(dbContext, CategoryActivityId, "Activities & Tours", "Tours, experiences, and bookable activities.", ListingType.Activity, BookingConfirmationType.Request, now, cancellationToken);

        var today = DateOnly.FromDateTime(now);

        await UpsertBanner(
            dbContext,
            Banner1Id,
            "Summer getaway",
            "Book curated stays and experiences with featured banner placements.",
            "/images/banners/seed-summer-getaway.jpg",
            BannerPlacement.LandingPage,
            1,
            today.AddDays(-7),
            today.AddDays(21),
            RecordStatus.Active,
            now,
            cancellationToken);

        await UpsertBanner(
            dbContext,
            Banner2Id,
            "Weekend escape",
            "Promote top listings across explore and category pages.",
            "/images/banners/seed-weekend-escape.jpg",
            BannerPlacement.ExplorePage,
            2,
            today.AddDays(-3),
            today.AddDays(14),
            RecordStatus.Active,
            now,
            cancellationToken);

        await UpsertPromotion(
            dbContext,
            Promotion1Id,
            "SAVE10",
            PromotionType.Percentage,
            10m,
            100,
            today.AddDays(-14),
            today.AddDays(30),
            RecordStatus.Active,
            now,
            cancellationToken);

        await UpsertPromotion(
            dbContext,
            Promotion2Id,
            "FIXED500",
            PromotionType.FixedAmount,
            500m,
            50,
            today.AddDays(-10),
            today.AddDays(20),
            RecordStatus.Active,
            now,
            cancellationToken);

        // The main seeded account already has the Vendor role and a vendor profile,
        // so its onboarding application must also be approved. The frontend uses
        // this application status to decide whether to open the vendor portal.
        UpsertVendorApplication(
            dbContext,
            MainVendorApplicationId,
            VendorUserId,
            "Main Vendor Studio",
            "Photography",
            "Simple seeded vendor profile.",
            "Colombo",
            "Main Vendor",
            "+94770000001",
            "https://example.com/license-main-vendor.pdf",
            "https://example.com/insurance-main-vendor.pdf",
            "https://example.com/tax-main-vendor.pdf",
            VendorApplicationStatus.Approved,
            now,
            cancellationToken);

        UpsertVendorApplication(
            dbContext,
            VendorApplication1Id,
            Customer1Id,
            "Customer One Events",
            "Events",
            "Photography for customer one events.",
            "Colombo",
            "Customer One",
            "+94770000011",
            "https://example.com/license-c1.pdf",
            "https://example.com/insurance-c1.pdf",
            "https://example.com/tax-c1.pdf",
            VendorApplicationStatus.Pending,
            now,
            cancellationToken);

        UpsertVendorApplication(
            dbContext,
            VendorApplication2Id,
            Customer2Id,
            "Customer Two Studio",
            "Portrait",
            "Portrait and lifestyle sessions from customer two.",
            "Kandy",
            "Customer Two",
            "+94770000012",
            "https://example.com/license-c2.pdf",
            "https://example.com/insurance-c2.pdf",
            "https://example.com/tax-c2.pdf",
            VendorApplicationStatus.Pending,
            now,
            cancellationToken);

        UpsertVendorApplication(
            dbContext,
            VendorApplication3Id,
            Customer3Id,
            "Customer Three Media",
            "Corporate",
            "Corporate and product work from customer three.",
            "Galle",
            "Customer Three",
            "+94770000013",
            "https://example.com/license-c3.pdf",
            "https://example.com/insurance-c3.pdf",
            "https://example.com/tax-c3.pdf",
            VendorApplicationStatus.Pending,
            now,
            cancellationToken);

        UpsertVendorApplication(
            dbContext,
            VendorApplication4Id,
            Customer4Id,
            "Customer Four Solutions",
            "Videography",
            "Professional video production and editing services.",
            "Matara",
            "Customer Four",
            "+94770000014",
            "https://example.com/license-c4.pdf",
            "https://example.com/insurance-c4.pdf",
            "https://example.com/tax-c4.pdf",
            VendorApplicationStatus.Pending,
            now,
            cancellationToken);

        UpsertVendorApplication(
            dbContext,
            VendorApplication5Id,
            Customer5Id,
            "Customer Five Designs",
            "Graphic Design",
            "Creative graphic design and branding services.",
            "Jaffna",
            "Customer Five",
            "+94770000015",
            "https://example.com/license-c5.pdf",
            "https://example.com/insurance-c5.pdf",
            "https://example.com/tax-c5.pdf",
            VendorApplicationStatus.Pending,
            now,
            cancellationToken);

        // Every photography listing is a session package (a flat rate for the
        // shoot, not priced per attendee) - PricingUnitOverride = FixedPrice
        // so a "2 guests" selection on any of these doesn't multiply the
        // price, matching how a real photography session is actually sold.
        var listings = new[]
        {
            new { Id = Listing1Id, Title = "Wedding Photography", Price = 25000m, Location = "Colombo", Description = "Full day wedding photography package.", Image1 = "https://images.unsplash.com/photo-1519741497674-611481863552?w=800", Image2 = "https://images.unsplash.com/photo-1519225421980-715cb0215aed?w=800", ActivityType = "Wedding Photography", Duration = 8, Difficulty = "Easy", MinGroup = 2, MaxGroup = 2, Included = "Photographer,Edited Photos,Online Gallery", Safety = "None" },
            new { Id = Listing2Id, Title = "Event Photography", Price = 18000m, Location = "Kandy", Description = "Photography for events and functions.", Image1 = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800", Image2 = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800", ActivityType = "Event Photography", Duration = 5, Difficulty = "Easy", MinGroup = 1, MaxGroup = 1, Included = "Photographer,Edited Photos", Safety = "None" },
            new { Id = Listing3Id, Title = "Portrait Session", Price = 12000m, Location = "Galle", Description = "Outdoor portrait photography session.", Image1 = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=800", Image2 = "https://images.unsplash.com/photo-1552058544-f2b08422138a?w=800", ActivityType = "Portrait Photography", Duration = 2, Difficulty = "Easy", MinGroup = 1, MaxGroup = 1, Included = "Photographer,10 Edited Photos", Safety = "None" },
            new { Id = Listing4Id, Title = "Corporate Photography", Price = 22000m, Location = "Negombo", Description = "Corporate and team photography package.", Image1 = "https://images.unsplash.com/photo-1521737604893-d14cc237f11d?w=800", Image2 = "https://images.unsplash.com/photo-1600880292203-757bb62b4baf?w=800", ActivityType = "Corporate Photography", Duration = 6, Difficulty = "Easy", MinGroup = 1, MaxGroup = 1, Included = "Photographer,Edited Photos,Same-Day Preview", Safety = "None" },
            new { Id = Listing5Id, Title = "Product Photography", Price = 15000m, Location = "Colombo", Description = "Studio product photography package.", Image1 = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800", Image2 = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=800", ActivityType = "Product Photography", Duration = 4, Difficulty = "Easy", MinGroup = 1, MaxGroup = 1, Included = "Studio Time,Photographer,Edited Photos", Safety = "None" }
        };

        foreach (var listing in listings)
        {
            UpsertListing(dbContext, listing.Id, VendorProfileId, CategoryId, listing.Title, listing.Description, listing.Price, listing.Location, ListingType.Activity, now, cancellationToken, PricingUnit.FixedPrice);
            UpsertActivityDetails(dbContext, listing.Id, listing.ActivityType, listing.Duration, listing.Difficulty, listing.Price, listing.MinGroup, listing.MaxGroup, listing.Included, listing.Safety);
            UpsertListingImages(dbContext, listing.Id, listing.Image1, listing.Image2);
        }

        // 2 listings per type-scoped category so search/filter/category tiles have
        // real spread across all 5 listing types, not just the photography vendor.
        // Pricing model is deliberately mixed across these so the feature is
        // actually exercised: a hotel room and a restaurant table are flat
        // (FixedPrice - a "2 guests" pick doesn't double the price), while
        // event tickets and a guided tour are genuinely PerPerson (each
        // extra person really does cost more).

        // Hotels: PerNight - a room is booked per night stayed, real hotel
        // pricing. "Number of Rooms" (quantity) multiplies rooms x nights,
        // not guests within a room.
        UpsertListing(dbContext, ListingHotel1Id, VendorProfileId, CategoryHotelId, "Seaside Resort Room", "Ocean-view double room with breakfast included.", 32000m, "Galle", ListingType.Hotel, now, cancellationToken, PricingUnit.PerNight);
        UpsertHotelDetails(dbContext, ListingHotel1Id, 32000m, 10, "WiFi,Pool,Breakfast,Air Conditioning", "Deluxe,Suite,Standard", "2:00 PM", "11:00 AM", "Resort", "Deluxe Ocean View");
        UpsertListingImages(dbContext, ListingHotel1Id, "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800", "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800");

        UpsertListing(dbContext, ListingHotel2Id, VendorProfileId, CategoryHotelId, "City Center Apartment", "Self-catered 1-bedroom apartment near the city center.", 19000m, "Colombo", ListingType.Hotel, now, cancellationToken, PricingUnit.PerNight);
        UpsertHotelDetails(dbContext, ListingHotel2Id, 19000m, 5, "WiFi,Kitchen,Washer", "Studio,1-Bedroom", "3:00 PM", "10:00 AM", "Apartment", "1-Bedroom");
        UpsertListingImages(dbContext, ListingHotel2Id, "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800", "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800");

        // Restaurants: FixedPrice - reserving the table costs the same
        // whether 2 or 4 people actually sit at it; party size is
        // capacity, not a price multiplier.
        UpsertListing(dbContext, ListingRestaurant1Id, VendorProfileId, CategoryRestaurantId, "Table at Spice Garden", "Table for two at a popular Sri Lankan fine-dining restaurant.", 6000m, "Colombo", ListingType.Restaurant, now, cancellationToken, PricingUnit.FixedPrice);
        UpsertRestaurantDetails(dbContext, ListingRestaurant1Id, "Sri Lankan", 3000m, "11:00 AM - 10:00 PM", 4, "Indoor,Outdoor", "2 hour seating limit");
        UpsertListingImages(dbContext, ListingRestaurant1Id, "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800", "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=800");

        UpsertListing(dbContext, ListingRestaurant2Id, VendorProfileId, CategoryRestaurantId, "Rooftop Grill Reservation", "Rooftop grill and bar table reservation.", 8500m, "Kandy", ListingType.Restaurant, now, cancellationToken, PricingUnit.FixedPrice);
        UpsertRestaurantDetails(dbContext, ListingRestaurant2Id, "Grill & BBQ", 4500m, "5:00 PM - 11:00 PM", 6, "Rooftop,VIP Booth", "Advance booking required on weekends");
        UpsertListingImages(dbContext, ListingRestaurant2Id, "https://images.unsplash.com/photo-1544025162-d76694265947?w=800", "https://images.unsplash.com/photo-1552566626-52f8b828add9?w=800");

        // Events: PerPerson - each ticket is a separate seat/entry; buying
        // 3 tickets genuinely costs 3x, unlike a table or a room.
        UpsertListing(dbContext, ListingEvent1Id, VendorProfileId, CategoryEventId, "Colombo Music Festival", "General admission ticket to the annual music festival.", 5000m, "Colombo", ListingType.Event, now, cancellationToken, PricingUnit.PerPerson);
        UpsertEventDetails(dbContext, ListingEvent1Id, "Colombo Music Festival", "Colombo Live Events", now.AddMonths(2), 500, 5000m, "Concert", "Viharamahadevi Open Air Theater", "Colombo 7");
        UpsertListingImages(dbContext, ListingEvent1Id, "https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?w=800", "https://images.unsplash.com/photo-1459749411175-04bf5292ceea?w=800");

        UpsertListing(dbContext, ListingEvent2Id, VendorProfileId, CategoryEventId, "Theater Night: The Play", "Ticket to an evening theater performance.", 3500m, "Kandy", ListingType.Event, now, cancellationToken, PricingUnit.PerPerson);
        UpsertEventDetails(dbContext, ListingEvent2Id, "Theater Night: The Play", "Kandy Theater Guild", now.AddMonths(1), 200, 3500m, "Theater", "Kandy City Theatre", "Kandy");
        UpsertListingImages(dbContext, ListingEvent2Id, "https://images.unsplash.com/photo-1503095396549-807759245b35?w=800", "https://images.unsplash.com/photo-1507924538820-ede94a04019d?w=800");

        // Car rentals: PerDay - the car itself is rented per day; how many
        // passengers ride in it doesn't change the daily rate.
        UpsertListing(dbContext, ListingCarRental1Id, VendorProfileId, CategoryCarRentalId, "Compact Car Rental", "Self-drive compact car, daily rental.", 9000m, "Colombo", ListingType.CarRental, now, cancellationToken, PricingUnit.PerDay);
        UpsertCarRentalDetails(dbContext, ListingCarRental1Id, "Toyota", "Aqua", "Automatic", 9000m, 5, "Petrol", "Available", 2022, null, "Colombo Airport", "Colombo Airport");
        UpsertListingImages(dbContext, ListingCarRental1Id, "https://images.unsplash.com/photo-1502877338535-766e1452684a?w=800", "https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?w=800");

        UpsertListing(dbContext, ListingCarRental2Id, VendorProfileId, CategoryCarRentalId, "SUV with Driver", "Chauffeur-driven SUV, daily rental.", 16000m, "Negombo", ListingType.CarRental, now, cancellationToken, PricingUnit.PerDay);
        UpsertCarRentalDetails(dbContext, ListingCarRental2Id, "Toyota", "Prado", "Automatic", 16000m, 7, "Diesel", "Available", 2023, 2500m, "Negombo", "Negombo", "Full Coverage");
        UpsertListingImages(dbContext, ListingCarRental2Id, "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800", "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800");

        // Sigiriya tour: PerPerson - each extra person is a real added
        // cost to the vendor (their own entry ticket + a lunch), unlike
        // the rafting trip below where the boat/guide is a flat package
        // regardless of headcount up to capacity.
        UpsertListing(dbContext, ListingActivity1Id, VendorProfileId, CategoryActivityId, "Sigiriya Day Tour", "Full-day guided tour of Sigiriya rock fortress.", 14000m, "Sigiriya", ListingType.Activity, now, cancellationToken, PricingUnit.PerPerson);
        UpsertActivityDetails(dbContext, ListingActivity1Id, "Cultural Tour", 8, "Moderate", 14000m, 2, 15, "Guide,Entry Tickets,Lunch", "Comfortable walking shoes required");
        UpsertListingImages(dbContext, ListingActivity1Id, "https://images.unsplash.com/photo-1580889240912-c17ba9b02f8b?w=800", "https://images.unsplash.com/photo-1544644181-1484b3fdfc62?w=800");

        UpsertListing(dbContext, ListingActivity2Id, VendorProfileId, CategoryActivityId, "Whitewater Rafting", "Half-day guided whitewater rafting experience.", 11000m, "Kitulgala", ListingType.Activity, now, cancellationToken, PricingUnit.PerPerson);
        UpsertActivityDetails(dbContext, ListingActivity2Id, "Water Sports", 4, "Moderate", 11000m, 1, 15, "Safety Gear,Guide,Snacks", "Must know how to swim");
        UpsertListingImages(dbContext, ListingActivity2Id, "https://images.unsplash.com/photo-1530866495561-507c9faab8d1?w=800", "https://images.unsplash.com/photo-1530866495561-2b4a9ef78e6f?w=800");
        UpsertOptionGroupWithValues(dbContext, ListingActivity2Id, "Group Size", new[]
        {
            ("Standard (1-9 people)", 11000m),
            ("Large Group (10+, 15% off)", 9350m),
            ("Private Trip (2-4 people, +1500/person)", 12500m)
        });

        var customerIds = new[] { Customer1Id, Customer2Id, Customer3Id };
        var customerNames = new[] { "Customer One", "Customer Two", "Customer Three" };
        var listingIds = new[] { Listing1Id, Listing2Id, Listing3Id, Listing4Id, Listing5Id };
        var bookingStatuses = new[]
        {
            BookingStatus.Confirmed,
            BookingStatus.Pending,
            BookingStatus.Completed,
            BookingStatus.Confirmed,
            BookingStatus.Pending
        };

        for (var customerIndex = 0; customerIndex < customerIds.Length; customerIndex++)
        {
            for (var listingIndex = 0; listingIndex < listingIds.Length; listingIndex++)
            {
                var bookingId = BookingIds[customerIndex, listingIndex];
                var startDate = new DateTime(2026, 5, 10 + (customerIndex * 5) + listingIndex, 10, 0, 0, DateTimeKind.Utc);
                var amount = 12000m + (listingIndex * 2500m) + (customerIndex * 1000m);

                UpsertBooking(
                    dbContext,
                    bookingId,
                    listingIds[listingIndex],
                    customerIds[customerIndex],
                    $"BKG-{customerIndex + 1:00}{listingIndex + 1:00}",
                    startDate,
                    startDate.AddHours(4),
                    bookingStatuses[listingIndex],
                    amount,
                    now,
                    cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded test dataset: 1 admin, 1 approved vendor, 5 customers, 5 pending vendor applications, 5 listings, 15 bookings.");

    }

    private static void UpsertUser(
        ApplicationDbContext dbContext,
        Guid id,
        UserRole role,
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        bool isEmailVerified,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var user = dbContext.Users.FirstOrDefault(x => x.Id == id);
        if (user == null)
        {
            dbContext.Users.Add(new User
            {
                Id = id,
                Role = role,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                IsEmailVerified = isEmailVerified,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedPassword),
                AuthProvider = AuthProvider.Local,
                CreatedAt = now
            });
            return;
        }

        user.Role = role;
        user.Email = email;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = phoneNumber;
        user.IsEmailVerified = isEmailVerified;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedPassword);
        user.AuthProvider = AuthProvider.Local;
        user.UpdatedAt = now;
        dbContext.Users.Update(user);
    }

    private static void UpsertVendorProfile(
        ApplicationDbContext dbContext,
        Guid profileId,
        Guid userId,
        string businessName,
        string businessType,
        string businessDescription,
        string contactNumber,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var profile = dbContext.VendorProfiles.FirstOrDefault(x => x.Id == profileId || x.UserId == userId);
        if (profile == null)
        {
            dbContext.VendorProfiles.Add(new VendorProfile
            {
                Id = profileId,
                UserId = userId,
                BusinessName = businessName,
                BusinessType = businessType,
                BusinessDescription = businessDescription,
                Bio = businessDescription,
                ContactNumber = contactNumber,
                IsActive = true,
                CreatedAt = now
            });
            return;
        }

        profile.UserId = userId;
        profile.BusinessName = businessName;
        profile.BusinessType = businessType;
        profile.BusinessDescription = businessDescription;
        profile.Bio = businessDescription;
        profile.ContactNumber = contactNumber;
        profile.IsActive = true;
        profile.UpdatedAt = now;
        dbContext.VendorProfiles.Update(profile);
    }

    private static void UpsertVendorPayout(
        ApplicationDbContext dbContext,
        Guid payoutId,
        Guid vendorProfileId,
        string bankName,
        string accountNumber,
        string accountHolderName,
        string branch,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var payout = dbContext.VendorPayouts.FirstOrDefault(x => x.Id == payoutId || x.VendorProfileId == vendorProfileId);
        if (payout == null)
        {
            dbContext.VendorPayouts.Add(new VendorPayout
            {
                Id = payoutId,
                VendorProfileId = vendorProfileId,
                BankName = bankName,
                AccountNumber = accountNumber,
                AccountHolderName = accountHolderName,
                Branch = branch,
                CreatedAt = now
            });
            return;
        }

        payout.BankName = bankName;
        payout.AccountNumber = accountNumber;
        payout.AccountHolderName = accountHolderName;
        payout.Branch = branch;
        payout.UpdatedAt = now;
        dbContext.VendorPayouts.Update(payout);
    }

    private static void UpsertUserLocalization(
        ApplicationDbContext dbContext,
        Guid localizationId,
        Guid userId,
        string language,
        string timeZone,
        string currency,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var settings = dbContext.UserLocalizationSettings.FirstOrDefault(x => x.UserId == userId);
        if (settings == null)
        {
            dbContext.UserLocalizationSettings.Add(new UserLocalizationSettings
            {
                Id = localizationId,
                UserId = userId,
                Language = language,
                TimeZone = timeZone,
                Currency = currency,
                CreatedAt = now
            });
            return;
        }

        settings.Language = language;
        settings.TimeZone = timeZone;
        settings.Currency = currency;
        dbContext.UserLocalizationSettings.Update(settings);
    }

    private static void UpsertCategory(
        ApplicationDbContext dbContext,
        Guid categoryId,
        string name,
        string description,
        ListingType type,
        BookingConfirmationType bookingType,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var category = dbContext.Categories.FirstOrDefault(x => x.Id == categoryId);
        if (category == null)
        {
            dbContext.Categories.Add(new Category
            {
                Id = categoryId,
                Name = name,
                Description = description,
                Type = type,
                BookingType = bookingType,
                Status = Ube.Domain.Enums.RecordStatus.Active,
                CreatedAt = now
            });
            return;
        }

        category.Name = name;
        category.Description = description;
        category.Type = type;
        category.BookingType = bookingType;
        category.Status = Ube.Domain.Enums.RecordStatus.Active;
        dbContext.Categories.Update(category);
    }

    private static async Task UpsertBanner(
        ApplicationDbContext dbContext,
        Guid bannerId,
        string title,
        string subtitle,
        string imageUrl,
        BannerPlacement placement,
        int displayOrder,
        DateOnly startDate,
        DateOnly endDate,
        RecordStatus status,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Banners.AnyAsync(x => x.Id == bannerId, cancellationToken);
        if (exists)
            return;

        dbContext.Banners.Add(new Banner
        {
            Id = bannerId,
            Title = title,
            Subtitle = subtitle,
            ImageUrl = imageUrl,
            Placement = placement,
            DisplayOrder = displayOrder,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            CreatedAt = now
        });
    }

    private static async Task UpsertPromotion(
        ApplicationDbContext dbContext,
        Guid promotionId,
        string promoCode,
        PromotionType type,
        decimal value,
        int usageLimit,
        DateOnly startDate,
        DateOnly endDate,
        RecordStatus status,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Promotions.AnyAsync(x => x.Id == promotionId, cancellationToken);
        if (exists)
            return;

        dbContext.Promotions.Add(new Promotion
        {
            Id = promotionId,
            PromoCode = promoCode,
            Type = type,
            Value = value,
            UsageCount = 0,
            UsageLimit = usageLimit,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            CreatedAt = now
        });
    }

    private static void UpsertListing(
        ApplicationDbContext dbContext,
        Guid listingId,
        Guid vendorProfileId,
        Guid categoryId,
        string title,
        string description,
        decimal price,
        string location,
        ListingType type,
        DateTime now,
        CancellationToken cancellationToken,
        PricingUnit? pricingUnitOverride = null)
    {
        var listing = dbContext.Listings.FirstOrDefault(x => x.Id == listingId);
        if (listing == null)
        {
            dbContext.Listings.Add(new Listing
            {
                Id = listingId,
                VendorProfileId = vendorProfileId,
                CategoryId = categoryId,
                Title = title,
                Description = description,
                Price = price,
                Currency = "LKR",
                Location = location,
                IsActive = true,
                Type = type,
                AvailabilityType = AvailabilityType.Capacity,
                Capacity = 1,
                PricingUnitOverride = pricingUnitOverride,
                CreatedAt = now
            });
            return;
        }

        listing.VendorProfileId = vendorProfileId;
        listing.CategoryId = categoryId;
        listing.Title = title;
        listing.Description = description;
        listing.Price = price;
        listing.Currency = "LKR";
        listing.Location = location;
        listing.IsActive = true;
        listing.Type = type;
        listing.AvailabilityType = AvailabilityType.Capacity;
        listing.Capacity = 1;
        listing.PricingUnitOverride = pricingUnitOverride;
        listing.UpdatedAt = now;
        dbContext.Listings.Update(listing);
    }

    // Two real, thematically-matching Unsplash photos per listing (a
    // wedding photo for "Wedding Photography", a hotel room for the
    // seaside resort, etc.) instead of a repeated or random placeholder -
    // same CDN/URL pattern the frontend's own FALLBACK_IMAGE already uses.
    private static void UpsertListingImages(
        ApplicationDbContext dbContext,
        Guid listingId,
        string primaryImageUrl,
        string secondaryImageUrl)
    {
        var alreadyHasImages = dbContext.ListingImages.Any(x => x.ListingId == listingId);
        if (alreadyHasImages)
            return;

        dbContext.ListingImages.AddRange(
            new ListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                ImageUrl = primaryImageUrl,
                IsPrimary = true
            },
            new ListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                ImageUrl = secondaryImageUrl,
                IsPrimary = false
            });

        var listing = dbContext.Listings.Local.FirstOrDefault(x => x.Id == listingId)
            ?? dbContext.Listings.First(x => x.Id == listingId);
        listing.ThumbnailUrl = primaryImageUrl;
    }

    private static void UpsertOptionGroupWithValues(
        ApplicationDbContext dbContext, Guid listingId, string groupName,
        (string Name, decimal PriceOverride)[] values)
    {
        if (dbContext.Set<ListingOptionGroup>().Any(x => x.ListingId == listingId && x.Name == groupName))
            return;

        var group = new ListingOptionGroup { Id = Guid.NewGuid(), ListingId = listingId, Name = groupName, DisplayOrder = 0 };
        dbContext.Set<ListingOptionGroup>().Add(group);

        for (var i = 0; i < values.Length; i++)
        {
            dbContext.Set<ListingOptionValue>().Add(new ListingOptionValue
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                Name = values[i].Name,
                DisplayOrder = i,
                PriceOverride = values[i].PriceOverride
            });
        }
    }

    private static void UpsertHotelDetails(
        ApplicationDbContext dbContext, Guid listingId, decimal pricePerNight, int availableRooms,
        string amenities, string roomTypes, string checkInTime, string checkOutTime,
        string propertyType, string primaryRoomType)
    {
        var details = dbContext.Set<HotelListingDetails>().FirstOrDefault(x => x.ListingId == listingId);
        if (details == null)
        {
            dbContext.Set<HotelListingDetails>().Add(new HotelListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                PricePerNight = pricePerNight,
                AvailableRooms = availableRooms,
                Amenities = amenities,
                RoomTypes = roomTypes,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime,
                PropertyType = propertyType,
                PrimaryRoomType = primaryRoomType
            });
            return;
        }

        details.PricePerNight = pricePerNight;
        details.AvailableRooms = availableRooms;
        details.Amenities = amenities;
        details.RoomTypes = roomTypes;
        details.CheckInTime = checkInTime;
        details.CheckOutTime = checkOutTime;
        details.PropertyType = propertyType;
        details.PrimaryRoomType = primaryRoomType;
    }

    private static void UpsertRestaurantDetails(
        ApplicationDbContext dbContext, Guid listingId, string cuisineType, decimal averageCost,
        string openingHours, int tableCapacity, string tableTypes, string reservationRules)
    {
        var details = dbContext.Set<RestaurantListingDetails>().FirstOrDefault(x => x.ListingId == listingId);
        if (details == null)
        {
            dbContext.Set<RestaurantListingDetails>().Add(new RestaurantListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                CuisineType = cuisineType,
                AverageCost = averageCost,
                OpeningHours = openingHours,
                TableCapacity = tableCapacity,
                TableTypes = tableTypes,
                ReservationRules = reservationRules
            });
            return;
        }

        details.CuisineType = cuisineType;
        details.AverageCost = averageCost;
        details.OpeningHours = openingHours;
        details.TableCapacity = tableCapacity;
        details.TableTypes = tableTypes;
        details.ReservationRules = reservationRules;
    }

    private static void UpsertEventDetails(
        ApplicationDbContext dbContext, Guid listingId, string eventName, string organizer,
        DateTime dateAndTime, int seatCount, decimal ticketPrice, string eventType,
        string venueName, string venueAddress)
    {
        var details = dbContext.Set<EventListingDetails>().FirstOrDefault(x => x.ListingId == listingId);
        if (details == null)
        {
            dbContext.Set<EventListingDetails>().Add(new EventListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                EventName = eventName,
                Organizer = organizer,
                DateAndTime = dateAndTime,
                SeatCount = seatCount,
                TicketPrice = ticketPrice,
                EventType = eventType,
                VenueName = venueName,
                VenueAddress = venueAddress
            });
            return;
        }

        details.EventName = eventName;
        details.Organizer = organizer;
        details.DateAndTime = dateAndTime;
        details.SeatCount = seatCount;
        details.TicketPrice = ticketPrice;
        details.EventType = eventType;
        details.VenueName = venueName;
        details.VenueAddress = venueAddress;
    }

    private static void UpsertCarRentalDetails(
        ApplicationDbContext dbContext, Guid listingId, string brand, string model, string transmission,
        decimal pricePerDay, int seatCount, string fuelType, string availabilityStatus, int year,
        decimal? hourlyRate, string pickupLocation, string returnLocation, string? insuranceOptions = null)
    {
        var details = dbContext.Set<CarRentalListingDetails>().FirstOrDefault(x => x.ListingId == listingId);
        if (details == null)
        {
            dbContext.Set<CarRentalListingDetails>().Add(new CarRentalListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                Brand = brand,
                Model = model,
                Transmission = transmission,
                PricePerDay = pricePerDay,
                SeatCount = seatCount,
                FuelType = fuelType,
                AvailabilityStatus = availabilityStatus,
                Year = year,
                HourlyRate = hourlyRate,
                PickupLocation = pickupLocation,
                ReturnLocation = returnLocation,
                InsuranceOptions = insuranceOptions
            });
            return;
        }

        details.Brand = brand;
        details.Model = model;
        details.Transmission = transmission;
        details.PricePerDay = pricePerDay;
        details.SeatCount = seatCount;
        details.FuelType = fuelType;
        details.AvailabilityStatus = availabilityStatus;
        details.Year = year;
        details.HourlyRate = hourlyRate;
        details.PickupLocation = pickupLocation;
        details.ReturnLocation = returnLocation;
        details.InsuranceOptions = insuranceOptions;
    }

    private static void UpsertActivityDetails(
        ApplicationDbContext dbContext, Guid listingId, string activityType, int durationHours,
        string difficultyLevel, decimal price, int minGroupSize, int maxGroupSize,
        string includedServices, string safetyRequirements)
    {
        var details = dbContext.Set<ActivityListingDetails>().FirstOrDefault(x => x.ListingId == listingId);
        if (details == null)
        {
            dbContext.Set<ActivityListingDetails>().Add(new ActivityListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                ActivityType = activityType,
                DurationHours = durationHours,
                DifficultyLevel = difficultyLevel,
                Price = price,
                MinGroupSize = minGroupSize,
                MaxGroupSize = maxGroupSize,
                IncludedServices = includedServices,
                SafetyRequirements = safetyRequirements
            });
            return;
        }

        details.ActivityType = activityType;
        details.DurationHours = durationHours;
        details.DifficultyLevel = difficultyLevel;
        details.Price = price;
        details.MinGroupSize = minGroupSize;
        details.MaxGroupSize = maxGroupSize;
        details.IncludedServices = includedServices;
        details.SafetyRequirements = safetyRequirements;
    }

    private static void UpsertVendorApplication(
        ApplicationDbContext dbContext,
        Guid applicationId,
        Guid userId,
        string businessName,
        string businessType,
        string description,
        string address,
        string contactName,
        string phone,
        string? businessLicensePath,
        string? insuranceCertificatePath,
        string? taxDocumentPath,
        VendorApplicationStatus status,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var nameParts = contactName.Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var application = dbContext.VendorApplications.FirstOrDefault(x => x.Id == applicationId);
        if (application == null)
        {
            dbContext.VendorApplications.Add(new VendorApplication
            {
                Id = applicationId,
                UserId = userId,
                BusinessName = businessName,
                BusinessType = businessType,
                Description = description,
                Address = address,
                FirstName = firstName,
                LastName = lastName,
                Email = string.Empty,
                Phone = phone,
                BusinessLicensePath = businessLicensePath,
                InsuranceCertificatePath = insuranceCertificatePath,
                TaxDocumentPath = taxDocumentPath,
                Status = status,
                SubmittedAt = now,
                CreatedAt = now
            });
            return;
        }

        application.UserId = userId;
        application.BusinessName = businessName;
        application.BusinessType = businessType;
        application.Description = description;
        application.Address = address;
        application.FirstName = firstName;
        application.LastName = lastName;
        application.Phone = phone;
        application.BusinessLicensePath = businessLicensePath;
        application.InsuranceCertificatePath = insuranceCertificatePath;
        application.TaxDocumentPath = taxDocumentPath;
        application.Status = status;
        application.SubmittedAt = now;
        dbContext.VendorApplications.Update(application);
    }

    private static void UpsertBooking(
        ApplicationDbContext dbContext,
        Guid bookingId,
        Guid listingId,
        Guid customerId,
        string bookingNumber,
        DateTime startDateTime,
        DateTime endDateTime,
        BookingStatus status,
        decimal totalAmount,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var booking = dbContext.Bookings.FirstOrDefault(x => x.Id == bookingId);
        if (booking == null)
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = bookingId,
                BookingNumber = bookingNumber,
                ListingId = listingId,
                CustomerId = customerId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Status = status,
                TotalAmount = totalAmount,
                Currency = "LKR",
                CreatedAt = now
            });
            return;
        }

        booking.BookingNumber = bookingNumber;
        booking.ListingId = listingId;
        booking.CustomerId = customerId;
        booking.StartDateTime = startDateTime;
        booking.EndDateTime = endDateTime;
        booking.Status = status;
        booking.TotalAmount = totalAmount;
        booking.Currency = "LKR";
        booking.UpdatedAt = now;
        dbContext.Bookings.Update(booking);
    }
}
