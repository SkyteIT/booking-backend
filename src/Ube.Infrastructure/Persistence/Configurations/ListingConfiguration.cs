using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(l => l.Price)
            .HasPrecision(18, 2);

        builder.Property(l => l.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(l => l.Location)
            .HasMaxLength(500);

        builder.HasOne(l => l.VendorProfile)
            .WithMany(vp => vp.Listings)
            .HasForeignKey(l => l.VendorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Category)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Bookings)
            .WithOne(b => b.Listing)
            .HasForeignKey(b => b.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Reviews)
            .WithOne(r => r.Listing)
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.CartItems)
            .WithOne(ci => ci.Listing)
            .HasForeignKey(ci => ci.ListingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(l => l.VendorProfileId);
        builder.HasIndex(l => l.CategoryId);
        builder.HasIndex(l => l.IsActive);
    }
}
