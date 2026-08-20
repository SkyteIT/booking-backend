using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Vendors;
using Ube.Application.Features.Vendors.Payout;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class PayoutExportService : IPayoutExportService
{
    private readonly IPayoutBatchRepository _batchRepo;
    private readonly IPayoutExportRepository _exportRepo;
    private readonly IPayoutExportSettingsRepository _settingsRepo;
    private readonly IVendorPayoutRepository _vendorPayoutRepo;
    private readonly IVendorProfileRepository _vendorProfileRepo;
    private readonly IEncryptionService _encryption;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public PayoutExportService(
        IPayoutBatchRepository batchRepo,
        IPayoutExportRepository exportRepo,
        IPayoutExportSettingsRepository settingsRepo,
        IVendorPayoutRepository vendorPayoutRepo,
        IVendorProfileRepository vendorProfileRepo,
        IEncryptionService encryption,
        IPaymentAuditLogRepository auditRepo)
    {
        _batchRepo = batchRepo;
        _exportRepo = exportRepo;
        _settingsRepo = settingsRepo;
        _vendorPayoutRepo = vendorPayoutRepo;
        _vendorProfileRepo = vendorProfileRepo;
        _encryption = encryption;
        _auditRepo = auditRepo;
    }

    public async Task<PayoutExportRunDto> RequestExportAsync(Guid requestedByUserId, CancellationToken ct = default)
    {
        var pending = await _batchRepo.GetAllPendingAsync(ct);
        if (pending.Count == 0)
            throw new BusinessRuleException("No pending payout batches to export");

        foreach (var batch in pending)
        {
            batch.Status = PayoutBatchStatus.Exported;
            await _batchRepo.UpdateAsync(batch, ct);
        }

        var run = new PayoutExportRun
        {
            Id = Guid.NewGuid(),
            BatchIdsJson = JsonSerializer.Serialize(pending.Select(b => b.Id)),
            TotalAmount = pending.Sum(b => b.TotalAmount),
            Status = PayoutExportStatus.PendingApproval,
            RequestedByUserId = requestedByUserId
        };

        await _exportRepo.AddAsync(run, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = requestedByUserId,
            Action = "PayoutExportRequested",
            EntityType = nameof(PayoutExportRun),
            EntityId = run.Id
        }, ct);

        return ToDto(run);
    }

    public async Task<IReadOnlyList<PayoutExportRunDto>> GetPendingAsync(CancellationToken ct = default)
    {
        var runs = await _exportRepo.GetPendingAsync(ct);
        return runs.Select(ToDto).ToList();
    }

    public async Task<PayoutExportApprovalResult> ApproveAsync(Guid approvedByUserId, Guid exportRunId, CancellationToken ct = default)
    {
        var run = await _exportRepo.GetByIdAsync(exportRunId, ct)
            ?? throw new NotFoundException("Export run not found");

        if (run.Status != PayoutExportStatus.PendingApproval)
            throw new BusinessRuleException("This export run is not awaiting approval");

        // Maker-checker: the approver can never be the same person who
        // requested the export.
        if (run.RequestedByUserId == approvedByUserId)
            throw new ForbiddenException("The export must be approved by a different admin than the one who requested it");

        var settings = await _settingsRepo.GetOrCreateAsync(ct);

        run.ApprovedByUserId = approvedByUserId;
        run.ApprovedAt = DateTime.UtcNow;

        if (run.TotalAmount > settings.LargeExportThreshold)
        {
            // Large amount: the first approval is recorded, but the file
            // is NOT generated yet - a second, distinct admin has to sign
            // off first.
            run.Status = PayoutExportStatus.PendingSeniorApproval;
            await _exportRepo.UpdateAsync(run, ct);

            await _auditRepo.AddAsync(new PaymentAuditLogEntry
            {
                Id = Guid.NewGuid(),
                ActorUserId = approvedByUserId,
                Action = "PayoutExportApprovedPendingSeniorReview",
                EntityType = nameof(PayoutExportRun),
                EntityId = run.Id,
                MetadataJson = $"{{\"totalAmount\":{run.TotalAmount},\"threshold\":{settings.LargeExportThreshold}}}"
            }, ct);

            return new PayoutExportApprovalResult { Run = ToDto(run), File = null };
        }

        var file = await GenerateFileAsync(run, ct);

        run.Status = PayoutExportStatus.Approved;
        run.FileChecksum = file.Checksum;
        await _exportRepo.UpdateAsync(run, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = approvedByUserId,
            Action = "PayoutExportApproved",
            EntityType = nameof(PayoutExportRun),
            EntityId = run.Id,
            MetadataJson = $"{{\"checksum\":\"{file.Checksum}\",\"totalAmount\":{run.TotalAmount}}}"
        }, ct);

        return new PayoutExportApprovalResult { Run = ToDto(run), File = file };
    }

    public async Task<PayoutExportFileResult> GrantSeniorApprovalAsync(Guid seniorApproverUserId, Guid exportRunId, CancellationToken ct = default)
    {
        var run = await _exportRepo.GetByIdAsync(exportRunId, ct)
            ?? throw new NotFoundException("Export run not found");

        if (run.Status != PayoutExportStatus.PendingSeniorApproval)
            throw new BusinessRuleException("This export run is not awaiting senior approval");

        if (seniorApproverUserId == run.RequestedByUserId || seniorApproverUserId == run.ApprovedByUserId)
            throw new ForbiddenException("Senior approval must come from a third admin, different from both the requester and the first approver");

        var file = await GenerateFileAsync(run, ct);

        run.Status = PayoutExportStatus.Approved;
        run.SecondApprovedByUserId = seniorApproverUserId;
        run.SecondApprovedAt = DateTime.UtcNow;
        run.FileChecksum = file.Checksum;
        await _exportRepo.UpdateAsync(run, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = seniorApproverUserId,
            Action = "PayoutExportSeniorApproved",
            EntityType = nameof(PayoutExportRun),
            EntityId = run.Id,
            MetadataJson = $"{{\"checksum\":\"{file.Checksum}\",\"totalAmount\":{run.TotalAmount}}}"
        }, ct);

        return file;
    }

    public async Task<PayoutExportThresholdDto> GetThresholdAsync(CancellationToken ct = default)
    {
        var settings = await _settingsRepo.GetOrCreateAsync(ct);
        return ToDto(settings);
    }

    public async Task<PayoutExportThresholdDto> UpdateThresholdAsync(Guid actorUserId, UpdatePayoutExportThresholdRequest request, CancellationToken ct = default)
    {
        if (request.LargeExportThreshold < 0)
            throw new BusinessRuleException("Threshold cannot be negative");

        var settings = await _settingsRepo.GetOrCreateAsync(ct);
        settings.LargeExportThreshold = request.LargeExportThreshold;
        settings.UpdatedByUserId = actorUserId;
        settings.UpdatedAt = DateTime.UtcNow;
        await _settingsRepo.UpdateAsync(settings, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "PayoutExportThresholdUpdated",
            EntityType = nameof(PayoutExportSettings),
            EntityId = settings.Id,
            MetadataJson = $"{{\"newThreshold\":{settings.LargeExportThreshold}}}"
        }, ct);

        return ToDto(settings);
    }

    private async Task<PayoutExportFileResult> GenerateFileAsync(PayoutExportRun run, CancellationToken ct)
    {
        var batchIds = JsonSerializer.Deserialize<List<Guid>>(run.BatchIdsJson) ?? [];
        var batches = await _batchRepo.GetByIdsAsync(batchIds, ct);

        var csv = new StringBuilder();
        csv.AppendLine("VendorProfileId,BusinessName,BankName,AccountNumber,AccountHolderName,Branch,Amount,BatchId");

        foreach (var batch in batches)
        {
            var vendor = await _vendorProfileRepo.GetByIdAsync(batch.VendorProfileId);
            var payout = await _vendorPayoutRepo.GetByVendorIdAsync(batch.VendorProfileId);

            var businessName = vendor?.BusinessName ?? "UNKNOWN";
            var bankName = string.IsNullOrEmpty(payout?.BankName) ? "" : SafeDecrypt(payout.BankName);
            var accountNumber = payout != null && !string.IsNullOrEmpty(payout.AccountNumber)
                ? SafeDecrypt(payout.AccountNumber)
                : "MISSING-PAYOUT-DETAILS";
            var accountHolder = string.IsNullOrEmpty(payout?.AccountHolderName) ? "" : SafeDecrypt(payout.AccountHolderName);
            var branch = string.IsNullOrEmpty(payout?.Branch) ? "" : SafeDecrypt(payout.Branch);

            csv.AppendLine(string.Join(',',
                batch.VendorProfileId,
                CsvEscape(businessName),
                CsvEscape(bankName),
                CsvEscape(accountNumber),
                CsvEscape(accountHolder),
                CsvEscape(branch),
                batch.TotalAmount.ToString("F2"),
                batch.Id));
        }

        var content = Encoding.UTF8.GetBytes(csv.ToString());
        var checksum = Convert.ToHexString(SHA256.HashData(content));

        return new PayoutExportFileResult
        {
            FileName = $"payout-export-{run.Id}.csv",
            Content = content,
            Checksum = checksum
        };
    }

    public async Task<PayoutExportRunDto> RejectAsync(Guid actorUserId, Guid exportRunId, CancellationToken ct = default)
    {
        var run = await _exportRepo.GetByIdAsync(exportRunId, ct)
            ?? throw new NotFoundException("Export run not found");

        if (run.Status is not (PayoutExportStatus.PendingApproval or PayoutExportStatus.PendingSeniorApproval))
            throw new BusinessRuleException("This export run is not awaiting approval");

        run.Status = PayoutExportStatus.Rejected;
        await _exportRepo.UpdateAsync(run, ct);

        var batchIds = JsonSerializer.Deserialize<List<Guid>>(run.BatchIdsJson) ?? [];
        var batches = await _batchRepo.GetByIdsAsync(batchIds, ct);
        foreach (var batch in batches.Where(b => b.Status == PayoutBatchStatus.Exported))
        {
            batch.Status = PayoutBatchStatus.Pending;
            await _batchRepo.UpdateAsync(batch, ct);
        }

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "PayoutExportRejected",
            EntityType = nameof(PayoutExportRun),
            EntityId = run.Id
        }, ct);

        return ToDto(run);
    }

    private string SafeDecrypt(string cipherText)
    {
        try { return _encryption.Decrypt(cipherText); }
        catch { return "DECRYPT-ERROR"; }
    }

    private static string CsvEscape(string value)
        => value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static PayoutExportRunDto ToDto(PayoutExportRun r) => new()
    {
        Id = r.Id,
        BatchIds = JsonSerializer.Deserialize<List<Guid>>(r.BatchIdsJson) ?? [],
        TotalAmount = r.TotalAmount,
        Status = r.Status,
        RequestedByUserId = r.RequestedByUserId,
        RequestedAt = r.RequestedAt,
        ApprovedByUserId = r.ApprovedByUserId,
        ApprovedAt = r.ApprovedAt,
        SecondApprovedByUserId = r.SecondApprovedByUserId,
        SecondApprovedAt = r.SecondApprovedAt,
        FileChecksum = r.FileChecksum
    };

    private static PayoutExportThresholdDto ToDto(PayoutExportSettings s) => new()
    {
        LargeExportThreshold = s.LargeExportThreshold,
        UpdatedByUserId = s.UpdatedByUserId,
        UpdatedAt = s.UpdatedAt
    };
}
