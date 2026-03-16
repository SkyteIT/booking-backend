using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence.Configurations;

public class VendorApplicationConfiguration : IEntityTypeConfiguration<VendorApplication>
{
    public void Configure(EntityTypeBuilder<VendorApplication> builder)
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
        builder.Property(x => x.Status)
                .IsRequired();
        builder.Property(x => x.RejectionReason)
                .HasMaxLength(500);
        builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        
        
    }
}