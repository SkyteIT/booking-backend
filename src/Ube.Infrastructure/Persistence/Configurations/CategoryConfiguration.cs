using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Content;

namespace Ube.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);

        // Basic
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        // Configuration
        builder.Property(x => x.BookingType).HasMaxLength(50);
        builder.Property(x => x.ServiceModel).HasMaxLength(50);

        // Pricing
        builder.Property(x => x.DefaultCommissionPercent).HasPrecision(5, 2);
        builder.Property(x => x.PlatformServiceFee).HasPrecision(10, 2);

        // Media
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.BannerImageUrl).HasMaxLength(500);

        // Unique name index
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
