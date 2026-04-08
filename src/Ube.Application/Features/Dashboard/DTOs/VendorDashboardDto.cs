namespace Ube.Application.Features.Dashboard.DTOs;

public class VendorDashboardDto
{
    public int TotalListings { get; set;}
    public int ActiveBookings { get; set;}
    public decimal TotalRevenue { get; set;}
    public double AverageRating { get; set;}
}