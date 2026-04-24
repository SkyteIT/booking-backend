using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
        
        builder.Property(x => x.Description)
                .HasMaxLength(500);

        builder.Property(X => X.IsActive)
                .IsRequired();

    }
}