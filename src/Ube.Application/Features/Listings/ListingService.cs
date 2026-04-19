using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;
namespace Ube.Application.Features.Listings;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;

    public ListingService(IListingRepository listingRepository)
    {
        _listingRepository = listingRepository;
    }

    public async Task<List<ListingDto>> GetVendorListingsAsync(Guid vendorId)
    {
        var listings = await _listingRepository.GetByVendorIdAsync(vendorId);
        return listings.Select(l => new ListingDto(
            l.Id,
            l.Title))
            .ToList();
    }
}