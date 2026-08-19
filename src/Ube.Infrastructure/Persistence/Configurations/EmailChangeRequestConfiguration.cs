using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;

public class EmailChangeRequestConfiguration : IEntityTypeConfiguration<EmailChangeRequest>
{
    public void Configure(EntityTypeBuilder<EmailChangeRequest> builder)
    {
        builder.ToTable("EmailChangeRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CurrentEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RequestedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ReviewNotes).HasMaxLength(500);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
               .WithMany()
               .HasForeignKey(x => x.ReviewedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
