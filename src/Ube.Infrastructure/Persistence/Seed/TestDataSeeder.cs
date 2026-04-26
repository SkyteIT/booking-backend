using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Users;

namespace Ube.Infrastructure.Persistence.Seed;

public static class TestDataSeeder
{
    public static readonly Guid CustomerUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherVendorUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid VendorProfileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid OtherVendorProfileId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid CategoryPhotographyId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid ListingId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static readonly Guid PendingBookingId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    public static readonly Guid ConfirmedBookingId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    public static readonly Guid CancelledBookingId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public static async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var now = DateTime.UtcNow;

        if (!await dbContext.Users.AnyAsync(x => x.Id == CustomerUserId, cancellationToken))
        {
            dbContext.Users.Add(new User
            {
                Id = CustomerUserId,
                Email = "customer@test.local",
                FirstName = "Isuru",
                LastName = "Kavinda",
                PhoneNumber = "+94770000001",
                IsEmailVerified = true,
                AuthProvider = AuthProvider.Local,
                CreatedAt = now
            });
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == UserId, cancellationToken))
        {
            dbContext.Users.Add(new User
            {
                Id = UserId,
                Email = "vendor@testU.local",
                FirstName = "Main",
                LastName = "Vendor",
                PhoneNumber = "+94770000002",
                IsEmailVerified = true,
                AuthProvider = AuthProvider.Local,
                CreatedAt = now
            });
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == OtherVendorUserId, cancellationToken))
        {
            dbContext.Users.Add(new User
            {
                Id = OtherVendorUserId,
                Email = "other-vendor@test.local",
                FirstName = "Other",
                LastName = "Vendor",
                PhoneNumber = "+94770000003",
                IsEmailVerified = true,
                AuthProvider = AuthProvider.Local,
                CreatedAt = now
            });
        }

        if (!await dbContext.VendorProfiles.AnyAsync(x => x.Id == VendorProfileId, cancellationToken))
        {
            dbContext.VendorProfiles.Add(new VendorProfile
            {
                Id = VendorProfileId,
                UserId = UserId,
                BusinessName = "Main Vendor Studio",
                BusinessType = "Photography",
                Description = "Seeded vendor profile for booking status tests.",
                ContactNumber = "+94770000002",
                IsActive = true,
                CreatedAt = now
            });
        }

        if (!await dbContext.VendorProfiles.AnyAsync(x => x.Id == OtherVendorProfileId, cancellationToken))
        {
            dbContext.VendorProfiles.Add(new VendorProfile
            {
                Id = OtherVendorProfileId,
                UserId = OtherVendorUserId,
                BusinessName = "Other Vendor Co",
                BusinessType = "Catering",
                Description = "Second vendor profile for authorization rule tests.",
                ContactNumber = "+94770000003",
                IsActive = true,
                CreatedAt = now
            });
        }

        if (!await dbContext.Categories.AnyAsync(x => x.Id == CategoryPhotographyId, cancellationToken))
        {
            dbContext.Categories.Add(new Category
            {
                Id = CategoryPhotographyId,
                Name = "Photography",
                Description = "Seed category",
                IsActive = true,
                CreatedAt = now
            });
        }

        if (!await dbContext.Listings.AnyAsync(x => x.Id == ListingId, cancellationToken))
        {
            dbContext.Listings.Add(new Listing
            {
                Id = ListingId,
                VendorProfileId = VendorProfileId,
                CategoryId = CategoryPhotographyId,
                Title = "Event Photography - Basic",
                Description = "Seed listing for booking status transition testing.",
                BasePrice = 15000m,
                Currency = "LKR",
                Location = "Colombo",
                IsActive = true,
                CreatedAt = now
            });
        }

        if (!await dbContext.Bookings.AnyAsync(x => x.Id == PendingBookingId, cancellationToken))
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = PendingBookingId,
                BookingNumber = "BKG-000001",
                ListingId = ListingId,
                CustomerId = CustomerUserId,
                StartDateTime = now.AddDays(10),
                EndDateTime = now.AddDays(10).AddHours(4),
                Status = BookingStatus.Pending,
                TotalAmount = 15000m,
                Currency = "LKR",
                CreatedAt = now
            });
        }

        if (!await dbContext.Bookings.AnyAsync(x => x.Id == ConfirmedBookingId, cancellationToken))
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = ConfirmedBookingId,
                BookingNumber = "BKG-000002",
                ListingId = ListingId,
                CustomerId = CustomerUserId,
                StartDateTime = now.AddDays(15),
                EndDateTime = now.AddDays(15).AddHours(4),
                Status = BookingStatus.Confirmed,
                TotalAmount = 18000m,
                Currency = "LKR",
                CreatedAt = now
            });
        }
        if (!await dbContext.Bookings.AnyAsync(x => x.Id == CancelledBookingId, cancellationToken))
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = CancelledBookingId,
                BookingNumber = "BKG-000003",
                ListingId = ListingId,
                CustomerId = CustomerUserId,
                StartDateTime = now.AddDays(20),
                EndDateTime = now.AddDays(20).AddHours(4),
                Status = BookingStatus.Cancelled,
                TotalAmount = 15000m,
                Currency = "LKR",
                CreatedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seed data ready. VendorProfileId={VendorProfileId}, OtherVendorProfileId={OtherVendorProfileId}, PendingBookingId={PendingBookingId}, ConfirmedBookingId={ConfirmedBookingId}, CancelledBookingId={CancelledBookingId}",
            VendorProfileId,
            OtherVendorProfileId,
            PendingBookingId,
            ConfirmedBookingId,
            CancelledBookingId);
    }
}
