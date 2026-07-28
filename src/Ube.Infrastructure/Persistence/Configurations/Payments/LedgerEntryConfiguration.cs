using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.VendorProfileId);
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.RefundId);
        builder.HasIndex(x => x.PayoutBatchId);
        builder.HasIndex(x => x.VendorCommissionInvoiceId);
        builder.HasIndex(x => x.BookingId);
    }
}
