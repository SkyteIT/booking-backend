using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Carts;

namespace Ube.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(ci => ci.TotalPrice)
            .HasPrecision(18, 2);

        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Listing)
            .WithMany(l => l.CartItems)
            .HasForeignKey(ci => ci.ListingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(ci => ci.CartId);
        builder.HasIndex(ci => ci.ListingId);
    }
}
