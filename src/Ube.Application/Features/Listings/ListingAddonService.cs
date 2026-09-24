using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Listings;

public class ListingAddonService : IListingAddonService
{
    private readonly IListingAddonRepository _addonRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IVendorProfileRepository _vendorProfileRepo;

    public ListingAddonService(
        IListingAddonRepository addonRepo,
        IListingRepository listingRepo,
        IVendorProfileRepository vendorProfileRepo)
    {
        _addonRepo = addonRepo;
        _listingRepo = listingRepo;
        _vendorProfileRepo = vendorProfileRepo;
    }

    public async Task<IReadOnlyList<ListingAddonDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var addons = await _addonRepo.GetByListingIdAsync(listingId, ct);
        return addons.Select(ToDto).ToList();
    }

    public async Task<ListingAddonDto> CreateAsync(Guid listingId, Guid userId, CreateListingAddonRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessRuleException("Add-on name is required.");

        if (request.Price < 0)
            throw new BusinessRuleException("Price cannot be negative.");

        var addon = new ListingAddon
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            PricingModel = request.PricingModel,
            IsActive = true
        };

        await _addonRepo.AddAsync(addon, ct);
        return ToDto(addon);
    }

    public async Task<ListingAddonDto> UpdateAsync(Guid listingId, Guid addonId, Guid userId, UpdateListingAddonRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var addon = await _addonRepo.GetByIdAsync(addonId, ct)
            ?? throw new NotFoundException("Add-on not found.");

        if (addon.ListingId != listingId)
            throw new NotFoundException("Add-on not found.");

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BusinessRuleException("Add-on name cannot be empty.");
            addon.Name = request.Name.Trim();
        }

        if (request.Description is not null)
            addon.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (request.Price.HasValue)
        {
            if (request.Price.Value < 0)
                throw new BusinessRuleException("Price cannot be negative.");
            addon.Price = request.Price.Value;
        }

        if (request.PricingModel.HasValue)
            addon.PricingModel = request.PricingModel.Value;

        if (request.IsActive.HasValue)
            addon.IsActive = request.IsActive.Value;

        addon.UpdatedAt = DateTime.UtcNow;

        await _addonRepo.UpdateAsync(addon, ct);
        return ToDto(addon);
    }

    public async Task DeleteAsync(Guid listingId, Guid addonId, Guid userId, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var addon = await _addonRepo.GetByIdAsync(addonId, ct)
            ?? throw new NotFoundException("Add-on not found.");

        if (addon.ListingId != listingId)
            throw new NotFoundException("Add-on not found.");

        await _addonRepo.DeleteAsync(addon, ct);
    }

    private async Task EnsureOwnedListingAsync(Guid listingId, Guid userId, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found.");

        var vendorProfile = await _vendorProfileRepo.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to manage add-ons for this listing.");
    }

    private static ListingAddonDto ToDto(ListingAddon a) => new()
    {
        Id = a.Id,
        ListingId = a.ListingId,
        Name = a.Name,
        Description = a.Description,
        Price = a.Price,
        PricingModel = a.PricingModel,
        IsActive = a.IsActive
    };
}
