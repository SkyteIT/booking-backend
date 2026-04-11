using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Location)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.PriceFrom)
            .HasPrecision(18, 2);

        builder.Property(x => x.Rating)
            .HasPrecision(3, 2);

        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(500);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Listings)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
