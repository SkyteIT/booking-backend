namespace Ube.Application.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Currency { get; set; } = "LKR";
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int TotalVendors { get; set; }
}