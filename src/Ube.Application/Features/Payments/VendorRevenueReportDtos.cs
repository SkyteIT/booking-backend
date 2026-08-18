namespace Ube.Application.Features.Payments;

public enum ReportGranularity
{
    Daily,
    Weekly,
    Monthly
}

public class RevenueReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportGranularity Granularity { get; set; } = ReportGranularity.Daily;
}

public class RevenuePeriodDto
{
    public DateTime PeriodStart { get; set; }
    public decimal NetRevenue { get; set; }
    public int BookingCount { get; set; }
}

// The literal "what's mine vs what I owe UBE" breakdown - every figure
// shown, not just a single net number.
public class GrossCommissionRefundBreakdownDto
{
    public decimal GrossRevenue { get; set; }        // sum of Charge credits - what customers paid
    public decimal PlatformCommission { get; set; }  // sum of Commission debits - UBE's cut
    public decimal Refunds { get; set; }              // sum of Refund debits
    public decimal NetEarnings { get; set; }          // Gross - Commission - Refunds = payable to vendor
}

public class VendorEarningsDto
{
    public GrossCommissionRefundBreakdownDto Breakdown { get; set; } = new();
    public List<RevenuePeriodDto> RevenueTrend { get; set; } = new();
    public int TotalBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public decimal RevenueChangePercent { get; set; } // vs the immediately preceding period of equal length
    public BookingStatusBreakdownDto StatusBreakdown { get; set; } = new();
    public List<ListingRevenueDto> TopListings { get; set; } = new();
}

public class BookingStatusBreakdownDto
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int Completed { get; set; }
    public int Total { get; set; }
}

public class ListingRevenueDto
{
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public decimal GrossRevenue { get; set; }
    public int BookingCount { get; set; }
}
