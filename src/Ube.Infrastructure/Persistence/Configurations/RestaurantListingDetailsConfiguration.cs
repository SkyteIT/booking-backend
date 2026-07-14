using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class RestaurantListingDetailsConfiguration : IEntityTypeConfiguration<RestaurantListingDetails>
{
    public void Configure(EntityTypeBuilder<RestaurantListingDetails> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AverageCost).HasColumnType("decimal(18,2)");
    }
}
