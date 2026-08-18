using Ube.Domain.Entities.Fraud;
using Ube.Domain.Enums.Fraud;

namespace Ube.Application.Features.Fraud;

public record FraudFlagListItem(
    FraudFlag Flag,
    string BookingNumber,
    string ListingTitle,
    string CustomerName);

public interface IFraudFlagRepository
{
    Task AddAsync(FraudFlag flag, CancellationToken ct = default);
    Task<FraudFlag?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(FraudFlag flag, CancellationToken ct = default);
    Task<(List<FraudFlagListItem> Items, int TotalCount)> GetPagedAsync(
        FraudFlagStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
}
