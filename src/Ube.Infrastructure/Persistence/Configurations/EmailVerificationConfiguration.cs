using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;
public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
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

        
        builder.HasIndex(x => x.Token)
            .IsUnique();
        //optional: index on UserId for faster lookups when verifying email
        builder.HasIndex(x => x.UserId);
    }
}