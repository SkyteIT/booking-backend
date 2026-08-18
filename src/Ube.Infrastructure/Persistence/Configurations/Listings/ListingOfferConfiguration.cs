using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations.Listings;

public class ListingOfferConfiguration : IEntityTypeConfiguration<ListingOffer>
{
    public void Configure(EntityTypeBuilder<ListingOffer> builder)
    {
        builder.ToTable("ListingOffers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");

        builder.HasIndex(x => x.ListingId);

        // Cascade is safe here (unlike SeasonalPricingRule) - there's only
        // one FK path back to Listings, not two, so no multiple-cascade-
        // paths conflict. Matches ListingUnit's own convention.
        builder.HasOne(x => x.Listing)
               .WithMany(l => l.Offers)
               .HasForeignKey(x => x.ListingId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
