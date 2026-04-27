using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence.Configurations;

public class VendorApplicationConfiguration : IEntityTypeConfiguration<VendorApplication>
{
    public void Configure(EntityTypeBuilder<VendorApplication> builder)
    {
        builder.HasKey(va => va.Id);

        builder.Property(va => va.BusinessName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(va => va.BusinessType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(va => va.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(va => va.ContactNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(va => va.RejectionReason)
            .HasMaxLength(500);

        builder.HasOne(va => va.User)
            .WithMany()
            .HasForeignKey(va => va.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(va => va.UserId);
        builder.HasIndex(va => va.Status);
    }
}
