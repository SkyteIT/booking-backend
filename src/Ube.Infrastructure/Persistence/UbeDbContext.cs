using Microsoft.EntityFrameworkCore;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Notifications;

namespace Ube.Infrastructure.Persistence;

public class UbeDbContext : DbContext, IAppDbContext
{
    public UbeDbContext(DbContextOptions<UbeDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UbeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
