using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;
public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ExpiryDate)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique();
        // index on UserId for faster lookups
        builder.HasIndex(x => x.UserId);
    }
}
