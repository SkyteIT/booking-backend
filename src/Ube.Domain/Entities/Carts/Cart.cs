using Ube.Domain.Entities.Users;

namespace Ube.Domain.Entities.Carts;

public class Cart
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "LKR";

    public int ItemCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
