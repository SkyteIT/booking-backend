namespace Ube.Application.Features.Dashboard;

public class VendorBookingCountsDto
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int Completed { get; set; }
    public int Total { get; set; }

}
