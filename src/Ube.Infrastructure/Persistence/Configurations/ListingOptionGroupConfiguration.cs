using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingOptionGroupConfiguration : IEntityTypeConfiguration<ListingOptionGroup>
{
    public void Configure(EntityTypeBuilder<ListingOptionGroup> builder)
    {
        builder.ToTable("ListingOptionGroups");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.ListingId);

        // Removing a group removes its values with it - a value has no
        // meaning detached from the group that defines it.
        builder.HasMany(x => x.Values)
            .WithOne(x => x.Group)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
