using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class PaymentDisputeConfiguration : IEntityTypeConfiguration<PaymentDispute>
{
    public void Configure(EntityTypeBuilder<PaymentDispute> builder)
    {
        builder.ToTable("PaymentDisputes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ExternalDisputeReference).HasMaxLength(200);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DisputeFeeAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.Status);

        builder.HasOne<Payment>()
               .WithMany()
               .HasForeignKey(x => x.PaymentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
