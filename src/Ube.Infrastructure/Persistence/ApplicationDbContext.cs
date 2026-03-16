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
    {}

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Listing> Listings { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<Category> Categories {get; set; } = default!;
    public DbSet <VendorApplication> VendorApplications { get; set; } = default!;
    public DbSet <VendorProfile> VendorProfiles {get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}