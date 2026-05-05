using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Listings;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;

namespace Ube.Infrastructure.Persistence.Seed;

public static class TestDataSeeder
{
    public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa");
    public static readonly Guid VendorUserId = Guid.Parse("bbbbbbbb-0000-0000-0000-bbbbbbbbbbbb");
    public static readonly Guid VendorProfileId = Guid.Parse("bbbbbbbb-1111-1111-1111-bbbbbbbbbbbb");

    public static readonly Guid Customer1Id = Guid.Parse("cccccccc-0000-0000-0000-cccccccccccc");
    public static readonly Guid Customer2Id = Guid.Parse("cccccccc-1111-1111-1111-cccccccccccc");
    public static readonly Guid Customer3Id = Guid.Parse("cccccccc-2222-2222-2222-cccccccccccc");
    public static readonly Guid Customer4Id = Guid.Parse("cccccccc-6666-6666-6666-cccccccccccc");
    public static readonly Guid Customer5Id = Guid.Parse("cccccccc-7777-7777-7777-cccccccccccc");

    public static readonly Guid CategoryId = Guid.Parse("dddddddd-0000-0000-0000-dddddddddddd");

    public static readonly Guid Listing1Id = Guid.Parse("eeeeeeee-0000-0000-0000-eeeeeeeeeeee");
    public static readonly Guid Listing2Id = Guid.Parse("eeeeeeee-1111-1111-1111-eeeeeeeeeeee");
    public static readonly Guid Listing3Id = Guid.Parse("eeeeeeee-2222-2222-2222-eeeeeeeeeeee");
    public static readonly Guid Listing4Id = Guid.Parse("eeeeeeee-3333-3333-3333-eeeeeeeeeeee");
    public static readonly Guid Listing5Id = Guid.Parse("eeeeeeee-4444-4444-4444-eeeeeeeeeeee");

    public static readonly Guid VendorPayoutId = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb");

    public static readonly Guid VendorApplication1Id = Guid.Parse("dddddddd-1111-1111-1111-dddddddddddd");
    public static readonly Guid VendorApplication2Id = Guid.Parse("dddddddd-2222-2222-2222-dddddddddddd");
    public static readonly Guid VendorApplication3Id = Guid.Parse("dddddddd-3333-3333-3333-dddddddddddd");
    public static readonly Guid VendorApplication4Id = Guid.Parse("dddddddd-4444-4444-4444-dddddddddddd");
    public static readonly Guid VendorApplication5Id = Guid.Parse("dddddddd-5555-5555-5555-dddddddddddd");

    public static readonly Guid AdminLocalizationId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");
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
        UpsertUser(dbContext, VendorUserId, UserRole.Vendor, "vendor@ube.local", "Main", "Vendor", "+94770000001", true, now, cancellationToken);
        UpsertUser(dbContext, Customer1Id, UserRole.User, "customer1@ube.local", "Customer", "One", "+94770000011", true, now, cancellationToken);
        UpsertUser(dbContext, Customer2Id, UserRole.User, "customer2@ube.local", "Customer", "Two", "+94770000012", true, now, cancellationToken);
        UpsertUser(dbContext, Customer3Id, UserRole.User, "customer3@ube.local", "Customer", "Three", "+94770000013", true, now, cancellationToken);
        UpsertUser(dbContext, Customer4Id, UserRole.User, "customer4@ube.local", "Customer", "Four", "+94770000014", true, now, cancellationToken);
        UpsertUser(dbContext, Customer5Id, UserRole.User, "customer5@ube.local", "Customer", "Five", "+94770000015", true, now, cancellationToken);

        // Ensure each user has localization settings
        UpsertUserLocalization(dbContext, AdminLocalizationId, AdminUserId, "en", "UTC", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, VendorLocalizationId, VendorUserId, "si-LK", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer1LocalizationId, Customer1Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer2LocalizationId, Customer2Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer3LocalizationId, Customer3Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer4LocalizationId, Customer4Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);
        UpsertUserLocalization(dbContext, Customer5LocalizationId, Customer5Id, "en", "Asia/Colombo", "LKR", now, cancellationToken);

        UpsertVendorProfile(dbContext, VendorProfileId, VendorUserId, "Main Vendor Studio", "Photography", "Simple seeded vendor profile.", "+94770000001", now, cancellationToken);
        // Add vendor payout defaults
        UpsertVendorPayout(dbContext, VendorPayoutId, VendorProfileId, "Seed Bank", "000123456789", "Main Vendor", "Colombo Branch", now, cancellationToken);
        UpsertCategory(dbContext, CategoryId, "Photography", "Seed category for all listings.", now, cancellationToken);

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

        var listings = new[]
        {
            new { Id = Listing1Id, Title = "Wedding Photography", Price = 25000m, Location = "Colombo", Description = "Full day wedding photography package." },
            new { Id = Listing2Id, Title = "Event Photography", Price = 18000m, Location = "Kandy", Description = "Photography for events and functions." },
            new { Id = Listing3Id, Title = "Portrait Session", Price = 12000m, Location = "Galle", Description = "Outdoor portrait photography session." },
            new { Id = Listing4Id, Title = "Corporate Photography", Price = 22000m, Location = "Negombo", Description = "Corporate and team photography package." },
            new { Id = Listing5Id, Title = "Product Photography", Price = 15000m, Location = "Colombo", Description = "Studio product photography package." }
        };

        foreach (var listing in listings)
        {
            UpsertListing(dbContext, listing.Id, VendorProfileId, CategoryId, listing.Title, listing.Description, listing.Price, listing.Location, now, cancellationToken);
        }

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
            "Seeded test dataset: 1 admin, 1 vendor, 5 customers, 5 pending vendor applications, 5 listings, 15 bookings.");

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
                IsActive = true,
                CreatedAt = now
            });
            return;
        }

        category.Name = name;
        category.Description = description;
        category.IsActive = true;
        dbContext.Categories.Update(category);
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
        DateTime now,
        CancellationToken cancellationToken)
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
                AvailabilityType = AvailabilityType.Capacity,
                Capacity = 1,
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
        listing.AvailabilityType = AvailabilityType.Capacity;
        listing.Capacity = 1;
        listing.UpdatedAt = now;
        dbContext.Listings.Update(listing);
    }

    private static void UpsertVendorApplication(
        ApplicationDbContext dbContext,
        Guid applicationId,
        Guid userId,
        string businessName,
        string businessType,
        string description,
        string address,
        string contactPersonName,
        string contactNumber,
        string businessLicenseUrl,
        string insurenceCertificateUrl,
        string taxDocumentUrl,
        VendorApplicationStatus status,
        DateTime now,
        CancellationToken cancellationToken)
    {
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
                ContactPersonName = contactPersonName,
                ContactNumber = contactNumber,
                BusinessLicenseUrl = businessLicenseUrl,
                InsurenceCertificateUrl = insurenceCertificateUrl,
                TaxDocumentUrl = taxDocumentUrl,
                Status = status,
                SubmittedAt = now
            });
            return;
        }

        application.UserId = userId;
        application.BusinessName = businessName;
        application.BusinessType = businessType;
        application.Description = description;
        application.Address = address;
        application.ContactPersonName = contactPersonName;
        application.ContactNumber = contactNumber;
        application.BusinessLicenseUrl = businessLicenseUrl;
        application.InsurenceCertificateUrl = insurenceCertificateUrl;
        application.TaxDocumentUrl = taxDocumentUrl;
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
