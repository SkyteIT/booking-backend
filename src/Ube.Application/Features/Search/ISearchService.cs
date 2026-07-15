namespace Ube.Application.Features.Search;

public interface ISearchService
{
    Task<IReadOnlyList<SearchListingDto>> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken);
}
