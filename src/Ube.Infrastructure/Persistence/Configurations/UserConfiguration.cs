using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
       builder.HasKey(u => u.Id);
       builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(255);

        builder.HasIndex(x => x.Email)
               .IsUnique();

        builder.Property(x => x.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
               .HasMaxLength(20);
    }
}