using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Notifications;

namespace Ube.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.IsRead });

        // GetByUserIdAsync filters by UserId only (no IsRead) and sorts by
        // CreatedAt - the (UserId, IsRead) index above doesn't cover that
        // sort, so it's a separate composite rather than folding CreatedAt
        // into the existing one.
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}
