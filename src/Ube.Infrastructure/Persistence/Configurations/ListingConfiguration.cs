using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;

namespace ube.Infrastructure.persistence.Configurations;

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
        builder.Property(x =>x.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);
        builder.Property(x => x.Location)
                .HasMaxLength(200);
        builder.Property(x => x.IsActive)
                .IsRequired();

        builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<VendorProfile>()
                .WithMany()
                .HasForeignKey(x => x.VendorProfileId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}