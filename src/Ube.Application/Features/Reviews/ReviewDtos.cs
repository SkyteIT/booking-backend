using Ube.Application.Common.Models;

namespace Ube.Application.Features.Reviews;

public class CreateReviewDto
{
    public Guid BookingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public string? VendorReply { get; set; }
    public DateTime? VendorReplyAt { get; set; }
}

public class ReviewRequest : QueryOptions
{
    public int? Rating { get; set; }
}

public class VendorReplyDto
{
    public string Reply { get; set; } = string.Empty;
}

public class HideReviewDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AdminReviewDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid VendorId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public DateTime? HiddenAt { get; set; }
    public string? HideReason { get; set; }
}

// A customer's own review, for their "My Reviews" page - includes which
// listing it's for (and the vendor's reply, if any) since the customer
// needs to see that context, unlike the public vendor/listing-scoped views.
public class CustomerReviewDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? VendorReply { get; set; }
    public DateTime? VendorReplyAt { get; set; }
}
