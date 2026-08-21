using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Bookings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
       builder.HasKey(x => x.Id);
       builder.Property(x => x.BookingNumber)
                .IsRequired()
                .HasMaxLength(20);

       builder.Property(x => x.StartDateTime)
              .IsRequired();
        builder.Property(x => x.EndDateTime)
                .IsRequired();
        builder.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);
        builder.Property(x => x.Status)
                .IsRequired();
        builder.HasIndex(x => x.ListingId);

        // Composite subsumes a plain CustomerId index (leftmost-prefix) and
        // additionally backs GetBookingsByCustomerIdAsync's default sort and
        // the fraud-check range queries (CustomerId == x && CreatedAt >= since).
        builder.HasIndex(x => new { x.CustomerId, x.CreatedAt });

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.BookingNumber)
                .IsUnique();

        builder.HasIndex(x => x.StartDateTime);

        // Vendor booking list's default sort (GetBookingsByVendorIdAsync).
        builder.HasIndex(x => x.CreatedAt);
        builder.Property(x => x.RowVersion)
               .IsRowVersion();
        builder.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ListingUnit)
                .WithMany()
                .HasForeignKey(x => x.ListingUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ListingUnitId);
    }
}
