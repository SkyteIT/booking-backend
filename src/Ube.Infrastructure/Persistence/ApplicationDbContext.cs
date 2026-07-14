using Microsoft.EntityFrameworkCore;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Notifications;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IAppDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {}

    // ================= BASE TABLES =================
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Listing> Listings { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<VendorApplication> VendorApplications { get; set; } = default!;
    public DbSet<VendorProfile> VendorProfiles { get; set; } = default!;
    public DbSet<VendorPayout> VendorPayouts { get; set; } = default!;
    public DbSet<BlockedDate> BlockedDates { get; set; } = default!;
    public DbSet<UserLocalizationSettings> UserLocalizationSettings { get; set; } = default!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    public DbSet<Banner> Banners { get; set; } = default!;
    public DbSet<Promotion> Promotions { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; } = default!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = default!;
    public DbSet<ListingImage> ListingImages { get; set; } = default!;

    // ================= LISTING DETAIL TABLES =================
    public DbSet<HotelListingDetails> HotelListingDetails { get; set; } = default!;
    public DbSet<RestaurantListingDetails> RestaurantListingDetails { get; set; } = default!;
    public DbSet<EventListingDetails> EventListingDetails { get; set; } = default!;
    public DbSet<CarRentalListingDetails> CarRentalListingDetails { get; set; } = default!;
    public DbSet<ActivityListingDetails> ActivityListingDetails { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly (Configurations directory)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
