using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;
public class TwoFactorBackupCodeConfiguration : IEntityTypeConfiguration<TwoFactorBackupCode>
{
    public void Configure(EntityTypeBuilder<TwoFactorBackupCode> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.HasIndex(x => x.UserId);
    }
}
