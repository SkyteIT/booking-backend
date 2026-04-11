using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Notifications;

namespace Ube.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Listing> Listings { get; }
    DbSet<Banner> Banners { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
