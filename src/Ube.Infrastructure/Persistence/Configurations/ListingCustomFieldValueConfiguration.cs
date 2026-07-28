using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingCustomFieldValueConfiguration : IEntityTypeConfiguration<ListingCustomFieldValue>
{
    public void Configure(EntityTypeBuilder<ListingCustomFieldValue> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(x => x.Listing)
            .WithMany(l => l.CustomFieldValues)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade: if an admin removes a custom field from a category, any
        // listing values recorded against it are no longer meaningful and
        // are cleaned up with it (also avoids an FK conflict with the
        // replace-all-on-edit pattern used when saving category fields).
        builder.HasOne(x => x.CategoryCustomField)
            .WithMany()
            .HasForeignKey(x => x.CategoryCustomFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.CategoryCustomFieldId);
    }
}
