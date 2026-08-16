using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class CarRentalListingDetailsConfiguration : IEntityTypeConfiguration<CarRentalListingDetails>
{
    public void Configure(EntityTypeBuilder<CarRentalListingDetails> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PricePerDay).HasColumnType("decimal(18,2)");
        builder.Property(x => x.HourlyRate).HasColumnType("decimal(18,2)");
    }
}
