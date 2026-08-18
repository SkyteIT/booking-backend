using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations.Listings;

public class SeasonalPricingRuleConfiguration : IEntityTypeConfiguration<SeasonalPricingRule>
{
    public void Configure(EntityTypeBuilder<SeasonalPricingRule> builder)
    {
        builder.ToTable("SeasonalPricingRules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AdjustmentValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");

        builder.HasIndex(x => new { x.ListingId, x.ListingUnitId });

        // Restrict, not Cascade - ListingUnitId already cascades from
        // Listing via ListingUnits, and SQL Server rejects two cascade
        // paths to the same table.
        builder.HasOne(x => x.Listing)
               .WithMany()
               .HasForeignKey(x => x.ListingId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ListingUnit)
               .WithMany()
               .HasForeignKey(x => x.ListingUnitId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
