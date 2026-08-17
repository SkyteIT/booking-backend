using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Questions;

namespace Ube.Infrastructure.Persistence.Configurations;

public class ListingQuestionConfiguration : IEntityTypeConfiguration<ListingQuestion>
{
    public void Configure(EntityTypeBuilder<ListingQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionText)
                .IsRequired()
                .HasMaxLength(1000);
        builder.Property(x => x.AnswerText)
                .HasMaxLength(1000);

        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.VendorId);
        builder.HasIndex(x => x.CustomerId);

        builder.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
