using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.BusinessName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(vp => vp.BusinessType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vp => vp.Description)
            .HasMaxLength(1000);

        builder.Property(vp => vp.ContactNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(vp => vp.User)
            .WithMany(u => u.VendorProfiles)
            .HasForeignKey(vp => vp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(vp => vp.Listings)
            .WithOne(l => l.VendorProfile)
            .HasForeignKey(l => l.VendorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(vp => vp.UserId);
        builder.HasIndex(vp => vp.IsActive);
    }
}
