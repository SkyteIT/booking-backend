using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Domain.Entities.Carts;

namespace Ube.Application.Features.Cart;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto?> GetCartByUserIdAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        return cart == null ? null : MapToDto(cart);
    }

    public async Task<CartDto> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart != null)
            return MapToDto(cart);

        cart = new Ube.Domain.Entities.Carts.Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _cartRepository.AddAsync(cart);
        await _cartRepository.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        var listing = await _listingRepository.GetByIdAsync(request.ListingId)
            ?? throw new NotFoundException($"Listing with ID {request.ListingId} not found.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Ube.Domain.Entities.Carts.Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepository.AddAsync(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ListingId == request.ListingId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ListingId = request.ListingId,
                    Quantity = request.Quantity,
                    UnitPrice = listing.Price,
                    TotalPrice = request.Quantity * listing.Price,
                    AddedAt = DateTime.UtcNow
                };
                await _cartRepository.AddCartItemAsync(cartItem);
                cart.Items.Add(cartItem);
            }

            UpdateCartTotals(cart);
            await _cartRepository.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return MapToDto(cart);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid userId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than 0.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Cart not found for user {userId}.");

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId)
                ?? throw new NotFoundException($"Cart item with ID {request.CartItemId} not found.");

            cartItem.Quantity = request.Quantity;
            cartItem.TotalPrice = request.Quantity * cartItem.UnitPrice;

            UpdateCartTotals(cart);
            await _cartRepository.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return MapToDto(cart);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            var cartItem = cart?.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (cart == null || cartItem == null)
            {
                await _unitOfWork.RollbackAsync();
                return false;
            }

            cart.Items.Remove(cartItem);
            await _cartRepository.RemoveItemAsync(cartItem);

            UpdateCartTotals(cart);
            await _cartRepository.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ClearCartAsync(Guid userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                await _unitOfWork.RollbackAsync();
                return false;
            }

            foreach (var item in cart.Items.ToList())
            {
                await _cartRepository.RemoveItemAsync(item);
            }

            cart.TotalPrice = 0;
            cart.ItemCount = 0;
            cart.UpdatedAt = DateTime.UtcNow;

            await _cartRepository.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static void UpdateCartTotals(Ube.Domain.Entities.Carts.Cart cart)
    {
        cart.TotalPrice = cart.Items.Sum(i => i.TotalPrice);
        cart.ItemCount = cart.Items.Sum(i => i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;
    }

    private static CartDto MapToDto(Ube.Domain.Entities.Carts.Cart cart) => new()
    {
        Id = cart.Id,
        UserId = cart.UserId,
        TotalPrice = cart.TotalPrice,
        Currency = cart.Currency,
        ItemCount = cart.ItemCount,
        CreatedAt = cart.CreatedAt,
        UpdatedAt = cart.UpdatedAt,
        Items = cart.Items.Select(i => new CartItemDto
        {
            Id = i.Id,
            CartId = i.CartId,
            ListingId = i.ListingId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice,
            AddedAt = i.AddedAt
        }).ToList()
    };
}
