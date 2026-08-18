using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Notifications;

namespace Ube.Infrastructure.Persistence.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("PushSubscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint).IsRequired();
        builder.Property(x => x.P256dh).IsRequired();
        builder.Property(x => x.Auth).IsRequired();

        // A given browser endpoint can only be subscribed once - re-subscribing
        // (e.g. after the user re-enables permission) upserts rather than duplicates.
        builder.HasIndex(x => x.Endpoint).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
