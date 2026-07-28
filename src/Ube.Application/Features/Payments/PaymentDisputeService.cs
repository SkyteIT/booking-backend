using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

// Handles a bank/card dispute (chargeback) - a different event than a
// Refund, since the customer's bank pulls money back from the PLATFORM
// directly, without the platform choosing to. Recorded manually today;
// there's no real gateway/webhook yet to detect this automatically (same
// honest limitation as PaymentReconciliationService).
public class PaymentDisputeService : IPaymentDisputeService
{
    private readonly IPaymentDisputeRepository _disputeRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public PaymentDisputeService(
        IPaymentDisputeRepository disputeRepo,
        IPaymentRepository paymentRepo,
        ILedgerRepository ledgerRepo,
        IPaymentAuditLogRepository auditRepo)
    {
        _disputeRepo = disputeRepo;
        _paymentRepo = paymentRepo;
        _ledgerRepo = ledgerRepo;
        _auditRepo = auditRepo;
    }

    public async Task<PaymentDisputeDto> RecordDisputeAsync(Guid actorUserId, RecordDisputeRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(request.PaymentId, ct)
            ?? throw new NotFoundException("Payment not found");

        if (payment.Status is not (PaymentStatus.Captured or PaymentStatus.PartiallyRefunded))
            throw new BusinessRuleException("Only a captured (or partially refunded) payment can be disputed");

        if (request.Amount <= 0 || request.Amount > payment.Amount)
            throw new BusinessRuleException("Dispute amount must be positive and cannot exceed the original payment amount");

        var dispute = new PaymentDispute
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Amount = request.Amount,
            Reason = request.Reason,
            DisputeFeeAmount = request.DisputeFeeAmount,
            ExternalDisputeReference = request.ExternalDisputeReference,
            RecordedByUserId = actorUserId,
            Status = PaymentDisputeStatus.Opened
        };

        await _disputeRepo.AddAsync(dispute, ct);

        // Provisional debit - the bank pulls this money back immediately,
        // well before any final Won/Lost outcome is known.
        var entries = new List<LedgerEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.Dispute,
                Direction = LedgerDirection.Debit,
                Amount = request.Amount,
                PaymentId = payment.Id
            }
        };

        var vendorShare = payment.Amount == 0
            ? 0
            : MoneyMath.RoundCurrency(payment.NetVendorAmount * request.Amount / payment.Amount);

        if (vendorShare > 0)
        {
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Vendor,
                VendorProfileId = payment.VendorProfileId,
                EntryType = LedgerEntryType.Dispute,
                Direction = LedgerDirection.Debit,
                Amount = vendorShare,
                PaymentId = payment.Id
            });
        }

        if (request.DisputeFeeAmount.HasValue && request.DisputeFeeAmount.Value > 0)
        {
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.DisputeFee,
                Direction = LedgerDirection.Debit,
                Amount = request.DisputeFeeAmount.Value,
                PaymentId = payment.Id
            });
        }

        await _ledgerRepo.AddRangeAsync(entries, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "PaymentDisputeRecorded",
            EntityType = nameof(PaymentDispute),
            EntityId = dispute.Id,
            MetadataJson = $"{{\"amount\":{request.Amount},\"reason\":\"{request.Reason}\"}}"
        }, ct);

        return ToDto(dispute);
    }

    public async Task<PaymentDisputeDto> ResolveDisputeAsync(Guid actorUserId, Guid disputeId, ResolveDisputeRequest request, CancellationToken ct = default)
    {
        if (request.Outcome == PaymentDisputeStatus.Opened)
            throw new BusinessRuleException("Outcome must be Won, Lost, or Withdrawn");

        var dispute = await _disputeRepo.GetByIdAsync(disputeId, ct)
            ?? throw new NotFoundException("Dispute not found");

        if (dispute.Status != PaymentDisputeStatus.Opened)
            throw new BusinessRuleException("Only an opened dispute can be resolved");

        var payment = await _paymentRepo.GetByIdAsync(dispute.PaymentId, ct)
            ?? throw new NotFoundException("Payment not found");

        dispute.Status = request.Outcome;
        dispute.ResolvedByUserId = actorUserId;
        dispute.ResolvedAt = DateTime.UtcNow;
        await _disputeRepo.UpdateAsync(dispute, ct);

        if (request.Outcome is PaymentDisputeStatus.Won or PaymentDisputeStatus.Withdrawn)
        {
            // The platform gets the disputed amount back - reverse the
            // original debit with a NEW entry, never edit the old one.
            // The dispute fee is NOT reversed, win or lose - matches real
            // gateway behavior (it's the cost of having a dispute at all).
            var entries = new List<LedgerEntry>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    AccountType = LedgerAccountType.Platform,
                    EntryType = LedgerEntryType.DisputeReversal,
                    Direction = LedgerDirection.Credit,
                    Amount = dispute.Amount,
                    PaymentId = payment.Id
                }
            };

            var vendorShare = payment.Amount == 0
                ? 0
                : MoneyMath.RoundCurrency(payment.NetVendorAmount * dispute.Amount / payment.Amount);

            if (vendorShare > 0)
            {
                entries.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(),
                    AccountType = LedgerAccountType.Vendor,
                    VendorProfileId = payment.VendorProfileId,
                    EntryType = LedgerEntryType.DisputeReversal,
                    Direction = LedgerDirection.Credit,
                    Amount = vendorShare,
                    PaymentId = payment.Id
                });
            }

            await _ledgerRepo.AddRangeAsync(entries, ct);
        }
        else if (request.Outcome == PaymentDisputeStatus.Lost)
        {
            // The provisional debit from RecordDisputeAsync stands - the
            // money is genuinely gone. Distinguish this from a normal
            // Refunded payment since the platform didn't choose this.
            payment.Status = PaymentStatus.ChargedBack;
            await _paymentRepo.UpdateAsync(payment, ct);
        }

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = $"PaymentDisputeResolved_{request.Outcome}",
            EntityType = nameof(PaymentDispute),
            EntityId = dispute.Id
        }, ct);

        return ToDto(dispute);
    }

    public async Task<IReadOnlyList<PaymentDisputeDto>> GetForPaymentAsync(Guid paymentId, CancellationToken ct = default)
    {
        var disputes = await _disputeRepo.GetByPaymentIdAsync(paymentId, ct);
        return disputes.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<PaymentDisputeDto>> GetForVendorAsync(Guid vendorProfileId, CancellationToken ct = default)
    {
        var disputes = await _disputeRepo.GetByVendorIdAsync(vendorProfileId, ct);
        return disputes.Select(ToDto).ToList();
    }

    private static PaymentDisputeDto ToDto(PaymentDispute d) => new()
    {
        Id = d.Id,
        PaymentId = d.PaymentId,
        Amount = d.Amount,
        Reason = d.Reason,
        DisputeFeeAmount = d.DisputeFeeAmount,
        ExternalDisputeReference = d.ExternalDisputeReference,
        Status = d.Status,
        OpenedAt = d.OpenedAt,
        ResolvedAt = d.ResolvedAt,
        CreatedAt = d.CreatedAt
    };
}
