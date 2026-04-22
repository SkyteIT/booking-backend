using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
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

    // ================= LISTING DETAIL TABLES =================
    public DbSet<HotelListingDetails> HotelListingDetails { get; set; } = default!;
    public DbSet<RestaurantListingDetails> RestaurantListingDetails { get; set; } = default!;
    public DbSet<EventListingDetails> EventListingDetails { get; set; } = default!;
    public DbSet<CarRentalListingDetails> CarRentalListingDetails { get; set; } = default!;
    public DbSet<ActivityListingDetails> ActivityListingDetails { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================= RELATIONSHIPS =================

        // Booking -> User (Customer)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking -> Listing
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Listing)
            .WithMany()
            .HasForeignKey(b => b.ListingId)
            .OnDelete(DeleteBehavior.NoAction);

        // Listing -> Vendor
        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Vendor)
            .WithMany()
            .HasForeignKey(l => l.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Listing -> Category
        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Category)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // VendorProfile -> User
        modelBuilder.Entity<VendorProfile>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= DECIMAL PRECISION =================
        modelBuilder.Entity<Listing>()
            .Property(l => l.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        // ================= ONE-TO-ONE LISTING DETAILS =================

        modelBuilder.Entity<HotelListingDetails>()
            .HasOne(h => h.Listing)
            .WithOne()
            .HasForeignKey<HotelListingDetails>(h => h.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RestaurantListingDetails>()
            .HasOne(r => r.Listing)
            .WithOne()
            .HasForeignKey<RestaurantListingDetails>(r => r.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventListingDetails>()
            .HasOne(e => e.Listing)
            .WithOne()
            .HasForeignKey<EventListingDetails>(e => e.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CarRentalListingDetails>()
            .HasOne(c => c.Listing)
            .WithOne()
            .HasForeignKey<CarRentalListingDetails>(c => c.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ActivityListingDetails>()
            .HasOne(a => a.Listing)
            .WithOne()
            .HasForeignKey<ActivityListingDetails>(a => a.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}