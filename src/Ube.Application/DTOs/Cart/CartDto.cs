namespace Ube.Application.DTOs.Cart;

public sealed class CartDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "LKR";
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
}