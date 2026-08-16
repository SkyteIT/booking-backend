using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ActivityListingDetailsConfiguration : IEntityTypeConfiguration<ActivityListingDetails>
{
    public void Configure(EntityTypeBuilder<ActivityListingDetails> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
    }
}
