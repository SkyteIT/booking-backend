using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;

public class RoleChangeRequestConfiguration : IEntityTypeConfiguration<RoleChangeRequest>
{
    public void Configure(EntityTypeBuilder<RoleChangeRequest> builder)
    {
        builder.ToTable("RoleChangeRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ReviewNotes).HasMaxLength(500);

        builder.HasIndex(x => x.TargetUserId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.TargetUser)
               .WithMany()
               .HasForeignKey(x => x.TargetUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByUser)
               .WithMany()
               .HasForeignKey(x => x.RequestedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
               .WithMany()
               .HasForeignKey(x => x.ReviewedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
