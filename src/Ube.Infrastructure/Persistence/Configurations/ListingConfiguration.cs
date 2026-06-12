using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

        builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(1000);

        builder.Property(x => x.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);

        builder.Property(x => x.Location)
                .HasMaxLength(200);

        builder.Property(x => x.IsActive)
                .IsRequired();

        builder.Property(x => x.OriginalCategoryName)
                .HasMaxLength(200);

        builder.Property(x => x.ThumbnailUrl)
                .HasMaxLength(500);

        builder.Property(x => x.Capacity)
                .IsRequired();

        builder.Property(x => x.AvailabilityType)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AvailabilityType.Capacity)
                .HasSentinel((AvailabilityType)0);

        builder.HasIndex(x => x.VendorProfileId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Category)
                .WithMany(x => x.Listings)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VendorProfile)
                .WithMany()
                .HasForeignKey(x => x.VendorProfileId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
