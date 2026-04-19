using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence
{
    public interface IListingRepository
    {
        //Get listing by id (used in availability)
        Task<Listing?> GetByIdAsync(Guid listingId);
        Task<List<Listing>> GetByVendorIdAsync(Guid vendorId);
    }
}