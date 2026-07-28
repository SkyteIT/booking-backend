using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IPaymentAuditLogRepository
{
    Task AddAsync(PaymentAuditLogEntry entry, CancellationToken ct = default);
}
