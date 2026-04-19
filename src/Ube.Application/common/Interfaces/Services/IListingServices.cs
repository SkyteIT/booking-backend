using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Services;
public interface IListingService
{
    Task<List<ListingDto>> GetVendorListingsAsync(Guid vendorId);
}
