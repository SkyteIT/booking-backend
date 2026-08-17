using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Reviews;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ReviewLikeConfiguration : IEntityTypeConfiguration<ReviewLike>
{
    public void Configure(EntityTypeBuilder<ReviewLike> builder)
    {
        builder.HasKey(x => x.Id);

        // One like per customer per review - makes toggle naturally
        // idempotent and double-likes impossible at the DB level.
        builder.HasIndex(x => new { x.ReviewId, x.CustomerId }).IsUnique();

        builder.HasOne(x => x.Review)
                .WithMany()
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
