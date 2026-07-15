using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BusinessName)
                .IsRequired()
                .HasMaxLength(200);
        builder.Property(x => x.BusinessType)
                .IsRequired()
                .HasMaxLength(200);
        builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(1000);
        builder.Property(x => x.ContactNumber)
                .IsRequired()
                .HasMaxLength(20);
        builder.Property(x => x.IsActive)
                .IsRequired();

        builder.HasIndex(x => x.UserId)
                .IsUnique();
        builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        
        
    }
}