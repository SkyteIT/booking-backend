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

        builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

        builder.Property(x => x.Tags)
                .HasMaxLength(500);
        builder.Property(x => x.CancellationPolicy)
                .HasMaxLength(1000);

        builder.HasMany(x => x.Images)
                .WithOne(i => i.Listing)
                .HasForeignKey(i => i.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

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

        builder.HasOne(x => x.HotelDetails)
                .WithOne(d => d.Listing)
                .HasForeignKey<HotelListingDetails>(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RestaurantDetails)
                .WithOne(d => d.Listing)
                .HasForeignKey<RestaurantListingDetails>(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.EventDetails)
                .WithOne(d => d.Listing)
                .HasForeignKey<EventListingDetails>(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CarRentalDetails)
                .WithOne(d => d.Listing)
                .HasForeignKey<CarRentalListingDetails>(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActivityDetails)
                .WithOne(d => d.Listing)
                .HasForeignKey<ActivityListingDetails>(d => d.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}
