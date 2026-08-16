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

        builder.HasIndex(x => x.CustomerId);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.BookingNumber)
                .IsUnique();

        builder.HasIndex(x => x.StartDateTime);
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
    }
}
