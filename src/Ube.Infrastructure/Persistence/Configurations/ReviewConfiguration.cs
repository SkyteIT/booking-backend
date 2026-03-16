using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;

namespace Ube.Inrastructure.persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>

{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
                .IsRequired();
        builder.Property(x => x.Comment)
                .IsRequired()
                .HasMaxLength(1000);
        builder.HasIndex(x => x.BookingId);
        builder.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Listing>()
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}