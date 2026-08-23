using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IListingOptionRepository
{
    Task<List<ListingOptionGroup>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default);
    Task<ListingOptionGroup?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default);
    Task AddGroupAsync(ListingOptionGroup group, CancellationToken ct = default);
    Task UpdateGroupAsync(ListingOptionGroup group, CancellationToken ct = default);
    Task DeleteGroupAsync(ListingOptionGroup group, CancellationToken ct = default);

    Task<ListingOptionValue?> GetValueByIdAsync(Guid valueId, CancellationToken ct = default);
    Task AddValueAsync(ListingOptionValue value, CancellationToken ct = default);
    Task UpdateValueAsync(ListingOptionValue value, CancellationToken ct = default);
    Task DeleteValueAsync(ListingOptionValue value, CancellationToken ct = default);

    // Batch-fetch for CheckoutService - resolves the option values a
    // customer selected, with their Group loaded so the caller can
    // verify each one actually belongs to the listing being checked out.
    Task<List<ListingOptionValue>> GetValuesByIdsAsync(IEnumerable<Guid> valueIds, CancellationToken ct = default);
}
