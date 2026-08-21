namespace Ube.Application.Features.Search;

public interface ISearchService
{
    Task<SearchListingsResult> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken);
}
