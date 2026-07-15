using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class HotelListingDetailsConfiguration : IEntityTypeConfiguration<HotelListingDetails>
{
    public void Configure(EntityTypeBuilder<HotelListingDetails> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PricePerNight).HasColumnType("decimal(18,2)");
    }
}
