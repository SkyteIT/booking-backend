using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    // ================= BASE TABLES =================
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Listing> Listings { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<VendorApplication> VendorApplications { get; set; } = default!;
    public DbSet<VendorProfile> VendorProfiles { get; set; } = default!;
    public DbSet<VendorRegisterApplication> VendorRegisterApplications { get; set; } = default!;
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

        // ================= ONE-TO-ONE LISTING DETAILS =================
        // (These are currently not in separate configuration files)

        modelBuilder.Entity<HotelListingDetails>(entity =>
        {
            entity.HasOne(h => h.Listing)
                .WithOne()
                .HasForeignKey<HotelListingDetails>(h => h.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(h => h.PricePerNight).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<RestaurantListingDetails>(entity =>
        {
            entity.HasOne(r => r.Listing)
                .WithOne()
                .HasForeignKey<RestaurantListingDetails>(r => r.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(r => r.AverageCost).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<EventListingDetails>(entity =>
        {
            entity.HasOne(e => e.Listing)
                .WithOne()
                .HasForeignKey<EventListingDetails>(e => e.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.TicketPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<CarRentalListingDetails>(entity =>
        {
            entity.HasOne(c => c.Listing)
                .WithOne()
                .HasForeignKey<CarRentalListingDetails>(c => c.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(c => c.PricePerDay).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ActivityListingDetails>(entity =>
        {
            entity.HasOne(a => a.Listing)
                .WithOne()
                .HasForeignKey<ActivityListingDetails>(a => a.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(a => a.Price).HasColumnType("decimal(18,2)");
        });
    }
}