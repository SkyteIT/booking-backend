using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class PayoutExportRunConfiguration : IEntityTypeConfiguration<PayoutExportRun>
{
    public void Configure(EntityTypeBuilder<PayoutExportRun> builder)
    {
        builder.ToTable("PayoutExportRuns");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchIdsJson).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.FileChecksum).HasMaxLength(128);

        builder.HasIndex(x => x.Status);
    }
}
