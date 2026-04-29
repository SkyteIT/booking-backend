using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations;
public class LocalizationConfiguration : IEntityTypeConfiguration<UserLocalizationSettings>
{
    public void Configure(EntityTypeBuilder<UserLocalizationSettings> builder)
    {
        builder.ToTable("UserLocalizationSettings");

        builder.HasKey(x => x.Id);
        // relationship with User entity
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserLocalizationSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x =>x.Language)
            .IsRequired()
            .HasMaxLength(10);
        builder.Property(x => x.TimeZone)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();
    }
}