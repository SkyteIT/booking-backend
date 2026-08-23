using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingOptionValueConfiguration : IEntityTypeConfiguration<ListingOptionValue>
{
    public void Configure(EntityTypeBuilder<ListingOptionValue> builder)
    {
        builder.ToTable("ListingOptionValues");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PriceModifier).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PriceOverride).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ConfirmationTypeOverride).HasConversion<int?>();

        builder.HasIndex(x => x.GroupId);
    }
}
