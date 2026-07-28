using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class PayoutBatchConfiguration : IEntityTypeConfiguration<PayoutBatch>
{
    public void Configure(EntityTypeBuilder<PayoutBatch> builder)
    {
        builder.ToTable("PayoutBatches");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.VendorProfileId);
        builder.HasIndex(x => x.Status);
    }
}
