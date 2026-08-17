using Ube.Domain.Entities.Users;

namespace Ube.Domain.Entities.Reviews;

public class ReviewLike
{
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
