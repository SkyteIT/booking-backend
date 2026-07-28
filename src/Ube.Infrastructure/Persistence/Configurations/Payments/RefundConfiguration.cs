using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PolicyTierApplied).HasColumnType("decimal(9,4)");

        builder.HasIndex(x => x.PaymentId);

        builder.HasOne<Payment>()
               .WithMany()
               .HasForeignKey(x => x.PaymentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
