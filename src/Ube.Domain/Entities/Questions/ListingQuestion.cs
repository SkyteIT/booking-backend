using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;

namespace Ube.Domain.Entities.Questions;

// A customer's plain-text question about a listing, with an optional
// vendor answer - deliberately separate from Review (no rating, no
// completed-booking requirement, no moderation flow).
public class ListingQuestion
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    // Denormalized from Listing.VendorProfile.UserId, same pattern as
    // Review.VendorId - lets a vendor query "questions on my listings"
    // without joining through Listing every time.
    public Guid VendorId { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public DateTime? AnsweredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
