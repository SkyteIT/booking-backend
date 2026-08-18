using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

        builder.Property(x => x.Description)
                .HasMaxLength(500);

        builder.Property(x => x.BookingType).HasMaxLength(50);
        builder.Property(x => x.ServiceModel).HasMaxLength(50);
        builder.Property(x => x.DefaultCommissionPercent).HasPrecision(5, 2);
        builder.Property(x => x.PlatformServiceFee).HasPrecision(10, 2);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.BannerImageUrl).HasMaxLength(500);

        builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
