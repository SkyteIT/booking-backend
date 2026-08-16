using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Currency).IsRequired().HasMaxLength(10);
        builder.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.GatewayReference).HasMaxLength(200);

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CommissionPercentApplied).HasColumnType("decimal(9,4)");
        builder.Property(x => x.CommissionAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PlatformFeeAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NetVendorAmount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.VendorProfileId);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
    }
}
