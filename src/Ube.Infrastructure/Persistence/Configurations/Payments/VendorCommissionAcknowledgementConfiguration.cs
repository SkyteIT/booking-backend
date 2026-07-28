using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Configurations.Payments;

public class VendorCommissionAcknowledgementConfiguration : IEntityTypeConfiguration<VendorCommissionAcknowledgement>
{
    public void Configure(EntityTypeBuilder<VendorCommissionAcknowledgement> builder)
    {
        builder.ToTable("VendorCommissionAcknowledgements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CommissionPercentShown).HasColumnType("decimal(9,4)");

        builder.HasIndex(x => x.VendorProfileId);
    }
}
