using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class PaymentReconciliationService : IPaymentReconciliationService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IPaymentGatewayClient _gateway;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public PaymentReconciliationService(
        IPaymentRepository paymentRepo,
        IPaymentGatewayClient gateway,
        IPaymentAuditLogRepository auditRepo)
    {
        _paymentRepo = paymentRepo;
        _gateway = gateway;
        _auditRepo = auditRepo;
    }

    public async Task<ReconciliationReportDto> ReconcileAsync(Guid actorUserId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        var payments = await _paymentRepo.GetCapturedInRangeAsync(periodStart, periodEnd, ct);
        var mismatches = new List<ReconciliationMismatchDto>();

        foreach (var payment in payments)
        {
            if (string.IsNullOrEmpty(payment.GatewayReference))
                continue;

            // A Captured payment in our own records should still show as
            // Succeeded at the gateway. Anything else means our database
            // and the gateway have drifted apart - flagged for manual
            // review, never silently trusted or auto-corrected.
            var gatewayStatus = await _gateway.ConfirmChargeAsync(payment.GatewayReference, ct);
            if (gatewayStatus != GatewayChargeStatus.Succeeded)
            {
                mismatches.Add(new ReconciliationMismatchDto
                {
                    PaymentId = payment.Id,
                    ExpectedStatus = PaymentStatus.Captured.ToString(),
                    GatewayReportedStatus = gatewayStatus.ToString()
                });

                await _auditRepo.AddAsync(new PaymentAuditLogEntry
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = actorUserId,
                    Action = "ReconciliationMismatch",
                    EntityType = nameof(Payment),
                    EntityId = payment.Id,
                    MetadataJson = $"{{\"gatewayStatus\":\"{gatewayStatus}\"}}"
                }, ct);
            }
        }

        return new ReconciliationReportDto
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PaymentsChecked = payments.Count,
            Mismatches = mismatches,
            RanAt = DateTime.UtcNow
        };
    }
}
