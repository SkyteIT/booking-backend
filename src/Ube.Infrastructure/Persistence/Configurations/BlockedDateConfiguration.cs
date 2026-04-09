using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class BlockedDateConfiguration : IEntityTypeConfiguration<BlockedDate>
{
    public void Configure(EntityTypeBuilder<BlockedDate> builder)
    {
        builder.HasKey(bd => bd.Id);

        builder.Property(bd => bd.ListingId).IsRequired();

        builder.Property(bd => bd.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(bd => bd.CreatedAt)
            .IsRequired();

        builder.HasIndex( bd => new {bd.ListingId, bd.Date})
            .IsUnique();
        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}