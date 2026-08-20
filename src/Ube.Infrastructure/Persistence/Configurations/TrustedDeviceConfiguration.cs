using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;
public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
