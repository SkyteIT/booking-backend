using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class VendorCommissionOverrideConfiguration : IEntityTypeConfiguration<VendorCommissionOverride>
{
    public void Configure(EntityTypeBuilder<VendorCommissionOverride> builder)
    {
        builder.ToTable("VendorCommissionOverrides");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CommissionPercent).HasColumnType("decimal(9,4)");

        builder.HasIndex(x => new { x.VendorProfileId, x.CategoryId, x.Status });
    }
}
