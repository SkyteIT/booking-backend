using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations.Listings;

public class ListingAddonConfiguration : IEntityTypeConfiguration<ListingAddon>
{
    public void Configure(EntityTypeBuilder<ListingAddon> builder)
    {
        builder.ToTable("ListingAddons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.PricingModel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(x => x.Listing)
            .WithMany(x => x.Addons)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
