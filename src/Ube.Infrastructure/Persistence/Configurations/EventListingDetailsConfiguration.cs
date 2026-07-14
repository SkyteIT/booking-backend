using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class EventListingDetailsConfiguration : IEntityTypeConfiguration<EventListingDetails>
{
    public void Configure(EntityTypeBuilder<EventListingDetails> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TicketPrice).HasColumnType("decimal(18,2)");
    }
}
