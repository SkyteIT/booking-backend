using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Reviews;


namespace Ube.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>

{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);
        // rating must be between 1 and 5
        builder.Property(x => x.Rating)
                .IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5"));
        //comment
        builder.Property(x => x.Comment)
                .IsRequired()
                .HasMaxLength(1000);

        // indexes
        builder.HasIndex(x => x.BookingId)
                .IsUnique();
        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.VendorId);

        builder.HasIndex(x => new { x.VendorId, x.CreatedAt });
        // relationships
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
       // builder.HasOne<Vendor>()
         //       .WithMany()
           //     .HasForeignKey(x => x.VendorId)
             //   .OnDelete(DeleteBehavior.Restrict);
    }
}
