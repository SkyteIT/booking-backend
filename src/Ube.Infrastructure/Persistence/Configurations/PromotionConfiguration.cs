using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Content;

namespace Ube.Infrastructure.Persistence.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PromoCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.PromoCode).IsUnique();
    }
}
