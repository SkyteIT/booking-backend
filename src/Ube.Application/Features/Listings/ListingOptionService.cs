using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Listings;

public class ListingOptionService : IListingOptionService
{
    private readonly IListingOptionRepository _optionRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IVendorProfileRepository _vendorProfileRepo;

    public ListingOptionService(
        IListingOptionRepository optionRepo,
        IListingRepository listingRepo,
        IVendorProfileRepository vendorProfileRepo)
    {
        _optionRepo = optionRepo;
        _listingRepo = listingRepo;
        _vendorProfileRepo = vendorProfileRepo;
    }

    public async Task<IReadOnlyList<ListingOptionGroupDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var groups = await _optionRepo.GetByListingIdAsync(listingId, ct);
        return groups.Select(ToDto).ToList();
    }

    public async Task<ListingOptionGroupDto> AddGroupAsync(Guid listingId, Guid userId, AddListingOptionGroupRequest request, CancellationToken ct = default)
    {
        var listing = await EnsureOwnedListingAsync(listingId, userId, ct);

        var group = new ListingOptionGroup
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            Name = request.Name,
            DisplayOrder = request.DisplayOrder
        };

        await _optionRepo.AddGroupAsync(group, ct);
        return ToDto(group);
    }

    public async Task<ListingOptionGroupDto> UpdateGroupAsync(Guid listingId, Guid groupId, Guid userId, UpdateListingOptionGroupRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var group = await _optionRepo.GetGroupByIdAsync(groupId, ct)
            ?? throw new NotFoundException("Option group not found");
        if (group.ListingId != listingId)
            throw new NotFoundException("Option group not found");

        if (request.Name is not null) group.Name = request.Name;
        if (request.DisplayOrder.HasValue) group.DisplayOrder = request.DisplayOrder.Value;

        await _optionRepo.UpdateGroupAsync(group, ct);
        return ToDto(group);
    }

    public async Task DeleteGroupAsync(Guid listingId, Guid groupId, Guid userId, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var group = await _optionRepo.GetGroupByIdAsync(groupId, ct)
            ?? throw new NotFoundException("Option group not found");
        if (group.ListingId != listingId)
            throw new NotFoundException("Option group not found");

        await _optionRepo.DeleteGroupAsync(group, ct);
    }

    public async Task<ListingOptionValueDto> AddValueAsync(Guid listingId, Guid groupId, Guid userId, AddListingOptionValueRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var group = await _optionRepo.GetGroupByIdAsync(groupId, ct)
            ?? throw new NotFoundException("Option group not found");
        if (group.ListingId != listingId)
            throw new NotFoundException("Option group not found");

        var value = new ListingOptionValue
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            Name = request.Name,
            DisplayOrder = request.DisplayOrder,
            PriceModifier = request.PriceModifier,
            PriceOverride = request.PriceOverride,
            ConfirmationTypeOverride = request.ConfirmationTypeOverride,
            RequiresSeatSelection = request.RequiresSeatSelection
        };

        await _optionRepo.AddValueAsync(value, ct);
        return ToDto(value);
    }

    public async Task<ListingOptionValueDto> UpdateValueAsync(Guid listingId, Guid groupId, Guid valueId, Guid userId, UpdateListingOptionValueRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var value = await _optionRepo.GetValueByIdAsync(valueId, ct)
            ?? throw new NotFoundException("Option value not found");
        if (value.GroupId != groupId || value.Group.ListingId != listingId)
            throw new NotFoundException("Option value not found");

        if (request.Name is not null) value.Name = request.Name;
        if (request.DisplayOrder.HasValue) value.DisplayOrder = request.DisplayOrder.Value;
        if (request.PriceModifier.HasValue) value.PriceModifier = request.PriceModifier.Value;
        if (request.RequiresSeatSelection.HasValue) value.RequiresSeatSelection = request.RequiresSeatSelection.Value;
        if (request.ClearConfirmationTypeOverride)
            value.ConfirmationTypeOverride = null;
        else if (request.ConfirmationTypeOverride.HasValue)
            value.ConfirmationTypeOverride = request.ConfirmationTypeOverride;
        if (request.ClearPriceOverride)
            value.PriceOverride = null;
        else if (request.PriceOverride.HasValue)
            value.PriceOverride = request.PriceOverride;

        await _optionRepo.UpdateValueAsync(value, ct);
        return ToDto(value);
    }

    public async Task DeleteValueAsync(Guid listingId, Guid groupId, Guid valueId, Guid userId, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var value = await _optionRepo.GetValueByIdAsync(valueId, ct)
            ?? throw new NotFoundException("Option value not found");
        if (value.GroupId != groupId || value.Group.ListingId != listingId)
            throw new NotFoundException("Option value not found");

        await _optionRepo.DeleteValueAsync(value, ct);
    }

    private async Task<Listing> EnsureOwnedListingAsync(Guid listingId, Guid userId, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found");

        var vendorProfile = await _vendorProfileRepo.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to manage options for this listing.");

        return listing;
    }

    private static ListingOptionGroupDto ToDto(ListingOptionGroup g) => new()
    {
        Id = g.Id,
        ListingId = g.ListingId,
        Name = g.Name,
        DisplayOrder = g.DisplayOrder,
        Values = g.Values.OrderBy(v => v.DisplayOrder).Select(ToDto).ToList()
    };

    private static ListingOptionValueDto ToDto(ListingOptionValue v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        DisplayOrder = v.DisplayOrder,
        PriceModifier = v.PriceModifier,
        PriceOverride = v.PriceOverride,
        ConfirmationTypeOverride = v.ConfirmationTypeOverride,
        RequiresSeatSelection = v.RequiresSeatSelection
    };
}
