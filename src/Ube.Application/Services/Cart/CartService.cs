using Ube.Application.DTOs.Cart;
using Ube.Application.Interfaces.Repositories;
using Ube.Domain.Entities.Carts;
using CartEntity = Ube.Domain.Entities.Carts.Cart;

namespace Ube.Application.Services.Cart;

public sealed class CartService : ICartService
{
    private readonly ICartRepository _repository;

    public CartService(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return Map(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (request.GuestCount <= 0) throw new ArgumentException("Guest count must be greater than zero.");

        var startDate = request.StartDate?.Date ?? DateTime.UtcNow.Date;
        var endDate = request.EndDate?.Date ?? startDate.AddDays(1);
        if (endDate <= startDate) throw new ArgumentException("End date must be after start date.");

        var listing = await _repository.GetListingByIdAsync(request.ListingId);
        if (listing is null || !listing.IsActive) throw new KeyNotFoundException("Listing was not found or is inactive.");

        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(existing =>
            existing.ListingId == request.ListingId &&
            existing.StartDate?.Date == startDate &&
            existing.EndDate?.Date == endDate);
        if (item is null)
        {
            item = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ListingId = listing.Id,
                Listing = listing,
                Quantity = request.Quantity,
                GuestCount = request.GuestCount,
                StartDate = startDate,
                EndDate = endDate,
                UnitPrice = listing.Price,
                TotalPrice = CalculateTotal(listing.Price, request.Quantity, request.GuestCount, startDate, endDate),
            };
            cart.Items.Add(item);
            await _repository.AddCartItemAsync(item);
        }
        else
        {
            item.Quantity += request.Quantity;
            item.GuestCount = request.GuestCount;
            item.TotalPrice = CalculateTotal(item.UnitPrice, item.Quantity, item.GuestCount, startDate, endDate);
        }

        Recalculate(cart);
        await _repository.SaveChangesAsync();
        return Map(cart);
    }

    public async Task<CartDto> UpdateItemAsync(Guid userId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (request.GuestCount.HasValue && request.GuestCount.Value <= 0)
            throw new ArgumentException("Guest count must be greater than zero.");
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(existing => existing.Id == request.CartItemId);
        if (item is null) throw new KeyNotFoundException("Cart item was not found.");

        item.Quantity = request.Quantity;
        item.GuestCount = request.GuestCount ?? item.GuestCount;
        item.StartDate = request.StartDate?.Date ?? item.StartDate;
        item.EndDate = request.EndDate?.Date ?? item.EndDate;
        if (item.StartDate.HasValue && item.EndDate.HasValue && item.EndDate <= item.StartDate)
            throw new ArgumentException("End date must be after start date.");
        item.TotalPrice = CalculateTotal(
            item.UnitPrice,
            item.Quantity,
            item.GuestCount <= 0 ? 1 : item.GuestCount,
            item.StartDate?.Date ?? DateTime.UtcNow.Date,
            item.EndDate?.Date ?? DateTime.UtcNow.Date.AddDays(1));
        Recalculate(cart);
        await _repository.SaveChangesAsync();
        return Map(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(existing => existing.Id == cartItemId);
        if (item is null) throw new KeyNotFoundException("Cart item was not found.");

        await _repository.RemoveItemAsync(item);
        cart.Items.Remove(item);
        Recalculate(cart);
        await _repository.SaveChangesAsync();
        return Map(cart);
    }

    public async Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId);
        foreach (var item in cart.Items.ToList()) await _repository.RemoveItemAsync(item);
        cart.Items.Clear();
        Recalculate(cart);
        await _repository.SaveChangesAsync();
    }

    private async Task<CartEntity> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _repository.GetCartWithItemsAsync(userId);
        if (cart is not null) return cart;

        cart = new CartEntity { Id = Guid.NewGuid(), UserId = userId };
        await _repository.AddAsync(cart);
        await _repository.SaveChangesAsync();
        return cart;
    }

    private static void Recalculate(CartEntity cart)
    {
        cart.ItemCount = cart.Items.Sum(item => item.Quantity);
        cart.TotalPrice = cart.Items.Sum(item => item.TotalPrice);
        cart.UpdatedAt = DateTime.UtcNow;
    }

    private static decimal CalculateTotal(decimal unitPrice, int quantity, int guestCount, DateTime startDate, DateTime endDate)
    {
        var days = Math.Max(1, (endDate.Date - startDate.Date).Days);
        return unitPrice * quantity * guestCount * days;
    }

    private static CartDto Map(CartEntity cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            TotalPrice = cart.TotalPrice,
            Currency = cart.Currency,
            ItemCount = cart.ItemCount,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items.Select(item => new CartItemDto
            {
                Id = item.Id,
                ListingId = item.ListingId,
                Quantity = item.Quantity,
                GuestCount = item.GuestCount,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
            }).ToList(),
        };
    }
}