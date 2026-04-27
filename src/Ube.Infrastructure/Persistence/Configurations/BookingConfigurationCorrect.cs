using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Bookings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class BookingConfigurationCorrect : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.HasOne(b => b.Listing)
            .WithMany(l => l.Bookings)
            .HasForeignKey(b => b.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Customer)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(b => b.ListingId);
        builder.HasIndex(b => b.CustomerId);
        builder.HasIndex(b => b.Status);
    }
}
