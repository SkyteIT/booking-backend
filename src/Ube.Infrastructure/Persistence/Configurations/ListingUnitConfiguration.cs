using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingUnitConfiguration : IEntityTypeConfiguration<ListingUnit>
{
    public void Configure(EntityTypeBuilder<ListingUnit> builder)
    {
        builder.ToTable("ListingUnits");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kind).IsRequired().HasConversion<int>();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Code).HasMaxLength(20);
        builder.Property(x => x.PriceOverride).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.ListingId);
    }
}
