using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Reviews;

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
        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}