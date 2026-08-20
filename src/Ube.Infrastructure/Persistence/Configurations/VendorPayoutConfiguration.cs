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

        // Fields - lengths sized for AES-CBC ciphertext (IV + padded
        // block, Base64-encoded), not the plaintext, since all four are
        // encrypted at rest via IEncryptionService.
        builder.Property(x => x.BankName)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.AccountNumber)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.AccountHolderName)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.Branch)
               .HasMaxLength(250);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt);
        builder.HasIndex(x => x.VendorProfileId)
                .IsUnique();
    }
}