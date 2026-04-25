using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Vendors;

public class VendorPayoutConfiguration : IEntityTypeConfiguration<VendorPayout>
{
    public void Configure(EntityTypeBuilder<VendorPayout> builder)
    {
        builder.ToTable("VendorPayouts");

        builder.HasKey(x => x.Id);

        // Relationship (1-1)
        builder.HasOne(x => x.VendorProfile)
               .WithOne()
               .HasForeignKey<VendorPayout>(x => x.VendorProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        // Fields
        builder.Property(x => x.BankName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.AccountNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.AccountHolderName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Branch)
               .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt);
        builder.HasIndex(x => x.VendorProfileId)
                .IsUnique();
    }
}