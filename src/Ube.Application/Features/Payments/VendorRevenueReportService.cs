using System.Text;
using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

// Sourced from Payment/Refund records, not the vendor's own ledger.
// The ledger only ever records ONE side of a transaction on the vendor's
// account (a Credit of the already-net amount for PlatformCollected
// payments, or a Debit of just the commission owed for VendorCollected
// payments - see PaymentService.WriteChargeLedgerEntriesAsync) - it never
// carries the gross amount for a VendorCollected booking (the vendor
// collected that directly, off-ledger). Payment.Amount/CommissionAmount/
// PlatformFeeAmount/NetVendorAmount are recorded for every captured
// payment regardless of collection method, so they're the only correct
// source for a Gross/Commission/Net breakdown.
public class VendorRevenueReportService : IVendorRevenueReportService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRefundRepository _refundRepository;

    public VendorRevenueReportService(IPaymentRepository paymentRepository, IRefundRepository refundRepository)
    {
        _paymentRepository = paymentRepository;
        _refundRepository = refundRepository;
    }

    private static (DateTime Start, DateTime End) ResolveRange(RevenueReportRequest request)
    {
        var start = (request.StartDate ?? DateTime.UtcNow.AddDays(-30)).Date;
        var end = request.EndDate ?? DateTime.UtcNow;
        return (start, end);
    }

    // Payments in range, plus each one's processed-refund total (0 if none) -
    // shared by both the trend and the breakdown so refunds are attributed
    // consistently in both.
    private async Task<List<(Payment Payment, decimal RefundedAmount)>> GetPaymentsWithRefundsAsync(
        Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct)
    {
        var (start, end) = ResolveRange(request);
        var payments = await _paymentRepository.GetCapturedByVendorInRangeAsync(vendorProfileId, start, end, ct);

        var refundTotals = await _refundRepository.GetProcessedAmountsByPaymentIdsAsync(payments.Select(p => p.Id), ct);

        return payments
            .Select(p => (Payment: p, RefundedAmount: refundTotals.GetValueOrDefault(p.Id)))
            .ToList();
    }

    public async Task<List<RevenuePeriodDto>> GetReportAsync(Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default)
    {
        var paymentsWithRefunds = await GetPaymentsWithRefundsAsync(vendorProfileId, request, ct);

        return paymentsWithRefunds
            .GroupBy(x => GetPeriodStart(x.Payment.CreatedAt, request.Granularity))
            .Select(g => new RevenuePeriodDto
            {
                PeriodStart = g.Key,
                NetRevenue = g.Sum(x => x.Payment.NetVendorAmount - x.RefundedAmount),
                BookingCount = g.Select(x => x.Payment.BookingId).Distinct().Count()
            })
            .OrderBy(p => p.PeriodStart)
            .ToList();
    }

    // The literal "what's mine vs what I owe UBE" breakdown - every
    // figure named and shown, not collapsed into a single net number.
    public async Task<GrossCommissionRefundBreakdownDto> GetGrossCommissionRefundBreakdownAsync(Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default)
    {
        var paymentsWithRefunds = await GetPaymentsWithRefundsAsync(vendorProfileId, request, ct);

        var gross = paymentsWithRefunds.Sum(x => x.Payment.Amount);
        // Commission + the flat platform fee - both are money the vendor
        // never keeps, shown together as one "owed to UBE" figure.
        var commission = paymentsWithRefunds.Sum(x => x.Payment.CommissionAmount + x.Payment.PlatformFeeAmount);
        var refunds = paymentsWithRefunds.Sum(x => x.RefundedAmount);
        var net = paymentsWithRefunds.Sum(x => x.Payment.NetVendorAmount - x.RefundedAmount);

        return new GrossCommissionRefundBreakdownDto
        {
            GrossRevenue = gross,
            PlatformCommission = commission,
            Refunds = refunds,
            NetEarnings = net
        };
    }

    public async Task<byte[]> ExportReportCsvAsync(Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default)
    {
        var periods = await GetReportAsync(vendorProfileId, request, ct);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", new[] { "Period", "Net Revenue", "Bookings" }.Select(CsvEscape)));

        foreach (var p in periods)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                p.PeriodStart.ToString("yyyy-MM-dd"),
                p.NetRevenue.ToString(System.Globalization.CultureInfo.InvariantCulture),
                p.BookingCount.ToString()
            }.Select(CsvEscape)));
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(sb.ToString());
        return preamble.Concat(content).ToArray();
    }

    private static DateTime GetPeriodStart(DateTime date, ReportGranularity granularity)
    {
        var d = date.Date;
        return granularity switch
        {
            ReportGranularity.Weekly => d.AddDays(-(((int)d.DayOfWeek + 6) % 7)), // Monday of that week
            ReportGranularity.Monthly => new DateTime(d.Year, d.Month, 1),
            _ => d
        };
    }

    // Same escape rule as AdminService.ExportBookingsCsvAsync/PayoutExportService -
    // wraps a field in quotes (doubling internal quotes) if it contains a
    // comma, quote, or newline.
    private static string CsvEscape(string field)
    {
        field ??= string.Empty;
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
