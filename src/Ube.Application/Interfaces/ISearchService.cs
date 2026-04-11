using Ube.Application.DTOs.Search;

namespace Ube.Application.Interfaces;

public interface ISearchService
{
    Task<IReadOnlyList<SearchListingDto>> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken);
}
