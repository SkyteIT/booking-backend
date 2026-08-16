using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class PayoutExportSettingsConfiguration : IEntityTypeConfiguration<PayoutExportSettings>
{
    public void Configure(EntityTypeBuilder<PayoutExportSettings> builder)
    {
        builder.ToTable("PayoutExportSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LargeExportThreshold).HasColumnType("decimal(18,2)");
    }
}
