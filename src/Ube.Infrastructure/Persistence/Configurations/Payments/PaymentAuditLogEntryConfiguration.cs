using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class PaymentAuditLogEntryConfiguration : IEntityTypeConfiguration<PaymentAuditLogEntry>
{
    public void Configure(EntityTypeBuilder<PaymentAuditLogEntry> builder)
    {
        builder.ToTable("PaymentAuditLogEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MetadataJson).HasMaxLength(2000);

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
