using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Fraud;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Configurations.Fraud;

public class FraudFlagConfiguration : IEntityTypeConfiguration<FraudFlag>
{
    public void Configure(EntityTypeBuilder<FraudFlag> builder)
    {
        builder.ToTable("FraudFlags");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Details).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ReviewNotes).HasMaxLength(500);

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);

        builder.HasOne<Booking>(x => x.Booking)
               .WithMany()
               .HasForeignKey(x => x.BookingId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
