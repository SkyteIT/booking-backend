namespace Ube.Application.Features.Listings;

public interface IListingOptionService
{
    Task<IReadOnlyList<ListingOptionGroupDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<ListingOptionGroupDto> AddGroupAsync(Guid listingId, Guid userId, AddListingOptionGroupRequest request, CancellationToken ct = default);
    Task<ListingOptionGroupDto> UpdateGroupAsync(Guid listingId, Guid groupId, Guid userId, UpdateListingOptionGroupRequest request, CancellationToken ct = default);
    Task DeleteGroupAsync(Guid listingId, Guid groupId, Guid userId, CancellationToken ct = default);

    Task<ListingOptionValueDto> AddValueAsync(Guid listingId, Guid groupId, Guid userId, AddListingOptionValueRequest request, CancellationToken ct = default);
    Task<ListingOptionValueDto> UpdateValueAsync(Guid listingId, Guid groupId, Guid valueId, Guid userId, UpdateListingOptionValueRequest request, CancellationToken ct = default);
    Task DeleteValueAsync(Guid listingId, Guid groupId, Guid valueId, Guid userId, CancellationToken ct = default);
}
