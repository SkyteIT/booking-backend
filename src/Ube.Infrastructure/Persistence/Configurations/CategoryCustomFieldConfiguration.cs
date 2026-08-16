using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class CategoryCustomFieldConfiguration : IEntityTypeConfiguration<CategoryCustomField>
{
    public void Configure(EntityTypeBuilder<CategoryCustomField> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FieldType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Options)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.CustomFields)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CategoryId);
    }
}
