using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class LoyaltyDiscountTierConfiguration : IEntityTypeConfiguration<LoyaltyDiscountTier>
{
    public void Configure(EntityTypeBuilder<LoyaltyDiscountTier> builder)
    {
        builder.ToTable("LoyaltyDiscountTiers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiscountPercent).HasColumnType("decimal(9,4)");

        builder.HasIndex(x => x.MonthsActive).IsUnique();
    }
}
