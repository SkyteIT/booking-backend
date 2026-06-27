using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.DTOs.Search;
using Ube.Application.Interfaces;

namespace Ube.Application.Services;

public class SearchService : ISearchService
{
    private readonly IListingRepository _listingRepo;

    public SearchService(IListingRepository listingRepo)
    {
        _listingRepo = listingRepo;
    }

    public Task<IReadOnlyList<SearchListingDto>> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken)
        => _listingRepo.SearchAsync(request, cancellationToken);
}
