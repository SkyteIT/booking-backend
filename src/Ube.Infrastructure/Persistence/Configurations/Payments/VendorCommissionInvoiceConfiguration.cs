using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class VendorCommissionInvoiceConfiguration : IEntityTypeConfiguration<VendorCommissionInvoice>
{
    public void Configure(EntityTypeBuilder<VendorCommissionInvoice> builder)
    {
        builder.ToTable("VendorCommissionInvoices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmountOwed).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.VendorProfileId, x.Status });
    }
}
