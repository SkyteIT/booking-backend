using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Application.Features.Search;

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
