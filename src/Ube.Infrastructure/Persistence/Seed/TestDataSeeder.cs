using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;

namespace Ube.Infrastructure.Persistence.Seed;

public static class TestDataSeeder
{
    public static readonly Guid CustomerUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherVendorUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PendingVendorApplicationId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid RejectedVendorApplicationId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static readonly Guid VendorProfileId = UserId;
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

        var mainVendorUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == UserId, cancellationToken);
        if (mainVendorUser == null)
        {
            dbContext.Users.Add(new User
            {
                Id = UserId,
                Role = UserRole.Vendor,
                Email = "vendor@testU.local",
                FirstName = "Main",
                LastName = "Vendor",
                PhoneNumber = "+94770000002",
                IsEmailVerified = true,
                AuthProvider = AuthProvider.Local,
                CreatedAt = now
            });
        }
        else
        {
            mainVendorUser.Role = UserRole.Vendor;
            dbContext.Users.Update(mainVendorUser);
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

        var pendingVendorApplication = await dbContext.VendorApplications
            .FirstOrDefaultAsync(x => x.Id == PendingVendorApplicationId, cancellationToken);

        if (pendingVendorApplication == null)
        {
            dbContext.VendorApplications.Add(new VendorApplication
            {
                Id = PendingVendorApplicationId,
                UserId = CustomerUserId,
                BusinessName = "Seeded Events by Isuru",
                BusinessType = "Events",
                Description = "Pending vendor application used to exercise the admin review endpoints.",
                Address = "No. 12, Galle Road, Colombo",
                ContactPersonName = "Isuru Kavinda",
                ContactNumber = "+94770000001",
                BusinessLicenseUrl = "/seed-docs/vendor-application/business-license.pdf",
                InsurenceCertificateUrl = "/seed-docs/vendor-application/insurance-certificate.pdf",
                TaxDocumentUrl = "/seed-docs/vendor-application/tax-document.pdf",
                Status = VendorApplicationStatus.Pending,
                SubmittedAt = now.AddDays(-2)
            });
        }
        else
        {
            pendingVendorApplication.UserId = CustomerUserId;
            pendingVendorApplication.BusinessName = "Seeded Events by Isuru";
            pendingVendorApplication.BusinessType = "Events";
            pendingVendorApplication.Description = "Pending vendor application used to exercise the admin review endpoints.";
            pendingVendorApplication.Address = "No. 12, Galle Road, Colombo";
            pendingVendorApplication.ContactPersonName = "Isuru Kavinda";
            pendingVendorApplication.ContactNumber = "+94770000001";
            pendingVendorApplication.BusinessLicenseUrl = "/seed-docs/vendor-application/business-license.pdf";
            pendingVendorApplication.InsurenceCertificateUrl = "/seed-docs/vendor-application/insurance-certificate.pdf";
            pendingVendorApplication.TaxDocumentUrl = "/seed-docs/vendor-application/tax-document.pdf";
            pendingVendorApplication.Status = VendorApplicationStatus.Pending;
            pendingVendorApplication.SubmittedAt = now.AddDays(-2);
            pendingVendorApplication.ReviewedAt = null;
            pendingVendorApplication.ReviewedBy = null;
            pendingVendorApplication.RejectionReason = null;
            dbContext.VendorApplications.Update(pendingVendorApplication);
        }

        var rejectedVendorApplication = await dbContext.VendorApplications
            .FirstOrDefaultAsync(x => x.Id == RejectedVendorApplicationId, cancellationToken);

        if (rejectedVendorApplication == null)
        {
            dbContext.VendorApplications.Add(new VendorApplication
            {
                Id = RejectedVendorApplicationId,
                UserId = OtherVendorUserId,
                BusinessName = "Other Vendor Catering",
                BusinessType = "Catering",
                Description = "Rejected vendor application used to exercise status filters.",
                Address = "No. 25, Duplication Road, Colombo",
                ContactPersonName = "Other Vendor",
                ContactNumber = "+94770000003",
                BusinessLicenseUrl = "/seed-docs/vendor-application/rejected-business-license.pdf",
                InsurenceCertificateUrl = "/seed-docs/vendor-application/rejected-insurance-certificate.pdf",
                TaxDocumentUrl = "/seed-docs/vendor-application/rejected-tax-document.pdf",
                Status = VendorApplicationStatus.Rejected,
                SubmittedAt = now.AddDays(-5),
                ReviewedAt = now.AddDays(-4),
                ReviewedBy = UserId,
                RejectionReason = "Insufficient supporting documents"
            });
        }
        else
        {
            rejectedVendorApplication.UserId = OtherVendorUserId;
            rejectedVendorApplication.BusinessName = "Other Vendor Catering";
            rejectedVendorApplication.BusinessType = "Catering";
            rejectedVendorApplication.Description = "Rejected vendor application used to exercise status filters.";
            rejectedVendorApplication.Address = "No. 25, Duplication Road, Colombo";
            rejectedVendorApplication.ContactPersonName = "Other Vendor";
            rejectedVendorApplication.ContactNumber = "+94770000003";
            rejectedVendorApplication.BusinessLicenseUrl = "/seed-docs/vendor-application/rejected-business-license.pdf";
            rejectedVendorApplication.InsurenceCertificateUrl = "/seed-docs/vendor-application/rejected-insurance-certificate.pdf";
            rejectedVendorApplication.TaxDocumentUrl = "/seed-docs/vendor-application/rejected-tax-document.pdf";
            rejectedVendorApplication.Status = VendorApplicationStatus.Rejected;
            rejectedVendorApplication.SubmittedAt = now.AddDays(-5);
            rejectedVendorApplication.ReviewedAt = now.AddDays(-4);
            rejectedVendorApplication.ReviewedBy = UserId;
            rejectedVendorApplication.RejectionReason = "Insufficient supporting documents";
            dbContext.VendorApplications.Update(rejectedVendorApplication);
        }

        var mainVendorProfile = await dbContext.VendorProfiles
            .FirstOrDefaultAsync(x => x.UserId == UserId, cancellationToken);

        if (mainVendorProfile == null)
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
        else
        {
            mainVendorProfile.BusinessName = "Main Vendor Studio";
            mainVendorProfile.BusinessType = "Photography";
            mainVendorProfile.Description = "Seeded vendor profile for booking status tests.";
            mainVendorProfile.ContactNumber = "+94770000002";
            mainVendorProfile.IsActive = true;
            dbContext.VendorProfiles.Update(mainVendorProfile);
        }

        var otherVendorProfile = await dbContext.VendorProfiles
            .FirstOrDefaultAsync(x => x.UserId == OtherVendorUserId, cancellationToken);

        if (otherVendorProfile == null)
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
        else
        {
            otherVendorProfile.BusinessName = "Other Vendor Co";
            otherVendorProfile.BusinessType = "Catering";
            otherVendorProfile.Description = "Second vendor profile for authorization rule tests.";
            otherVendorProfile.ContactNumber = "+94770000003";
            otherVendorProfile.IsActive = true;
            dbContext.VendorProfiles.Update(otherVendorProfile);
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

        var resolvedMainVendorProfileId = mainVendorProfile?.Id ?? VendorProfileId;

        // Ensure listing exists and has correct owner/capacity
        var existingListing = await dbContext.Listings.FirstOrDefaultAsync(x => x.Id == ListingId, cancellationToken);
        
        if (existingListing == null)
        {
            dbContext.Listings.Add(new Listing
            {
                Id = ListingId,
                VendorProfileId = resolvedMainVendorProfileId,
                CategoryId = CategoryPhotographyId,
                Title = "Event Photography - Basic",
                Description = "Seed listing for booking status transition testing.",
                Price = 15000m,
                Currency = "LKR",
                Location = "Colombo",
                IsActive = true,
                Capacity = 5,
                AvailabilityType = Ube.Domain.Enums.Listings.AvailabilityType.Capacity,
                CreatedAt = now
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Keep seeded listing ownership and availability deterministic across reruns.
            existingListing.VendorProfileId = resolvedMainVendorProfileId;
            existingListing.Capacity = 5;
            existingListing.AvailabilityType = Ube.Domain.Enums.Listings.AvailabilityType.Capacity;
            dbContext.Listings.Update(existingListing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var pendingStart = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Utc);
        var confirmedStart = new DateTime(2026, 5, 15, 10, 0, 0, DateTimeKind.Utc);
        var cancelledStart = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc);

        var pendingBooking = await dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == PendingBookingId, cancellationToken);
        if (pendingBooking == null)
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = PendingBookingId,
                BookingNumber = "BKG-000001",
                ListingId = ListingId,
                CustomerId = CustomerUserId,
                StartDateTime = pendingStart,
                EndDateTime = pendingStart.AddHours(4),
                Status = BookingStatus.Pending,
                TotalAmount = 15000m,
                Currency = "LKR",
                CreatedAt = now
            });
        }
        else
        {
            pendingBooking.ListingId = ListingId;
            pendingBooking.CustomerId = CustomerUserId;
            pendingBooking.StartDateTime = pendingStart;
            pendingBooking.EndDateTime = pendingStart.AddHours(4);
            pendingBooking.Status = BookingStatus.Pending;
            pendingBooking.TotalAmount = 15000m;
            pendingBooking.Currency = "LKR";
            pendingBooking.CreatedAt = now;
            dbContext.Bookings.Update(pendingBooking);
        }

        var confirmedBooking = await dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == ConfirmedBookingId, cancellationToken);
        if (confirmedBooking == null)
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = ConfirmedBookingId,
                BookingNumber = "BKG-000002",
                ListingId = ListingId,
                CustomerId = CustomerUserId,
                StartDateTime = confirmedStart,
                EndDateTime = confirmedStart.AddHours(4),
                Status = BookingStatus.Confirmed,
                TotalAmount = 18000m,
                Currency = "LKR",
                CreatedAt = now
            });
        }
        else
        {
            confirmedBooking.ListingId = ListingId;
            confirmedBooking.CustomerId = CustomerUserId;
            confirmedBooking.StartDateTime = confirmedStart;
            confirmedBooking.EndDateTime = confirmedStart.AddHours(4);
            confirmedBooking.Status = BookingStatus.Confirmed;
            confirmedBooking.TotalAmount = 18000m;
            confirmedBooking.Currency = "LKR";
            dbContext.Bookings.Update(confirmedBooking);
        }

        var cancelledBooking = await dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == CancelledBookingId, cancellationToken);
        if (cancelledBooking == null)
        {
            dbContext.Bookings.Add(new Booking
            {
                Id = CancelledBookingId,
                BookingNumber = "BKG-000003",
                ListingId = ListingId,
                CustomerId = CustomerUserId,
                StartDateTime = cancelledStart,
                EndDateTime = cancelledStart.AddHours(4),
                Status = BookingStatus.Cancelled,
                TotalAmount = 15000m,
                Currency = "LKR",
                CreatedAt = now
            });
        }
        else
        {
            cancelledBooking.ListingId = ListingId;
            cancelledBooking.CustomerId = CustomerUserId;
            cancelledBooking.StartDateTime = cancelledStart;
            cancelledBooking.EndDateTime = cancelledStart.AddHours(4);
            cancelledBooking.Status = BookingStatus.Cancelled;
            cancelledBooking.TotalAmount = 15000m;
            cancelledBooking.Currency = "LKR";
            dbContext.Bookings.Update(cancelledBooking);
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
