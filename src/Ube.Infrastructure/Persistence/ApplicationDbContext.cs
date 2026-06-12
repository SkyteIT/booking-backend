using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Entities.Auth;


namespace Ube.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {}

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Listing> Listings { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<Category> Categories {get; set; } = default!;
    public DbSet <VendorApplication> VendorApplications { get; set; } = default!;
    public DbSet <VendorProfile> VendorProfiles {get; set; } = default!;
    public DbSet<VendorPayout> VendorPayouts { get; set; } = default!;
    public DbSet <BlockedDate> BlockedDates {get; set; } = default!;
    public DbSet <UserLocalizationSettings> UserLocalizationSettings { get; set; } = default!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
