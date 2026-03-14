public class Listing
{
    public Guid Id { get; set; }

    public Guid VendorId { get; set; }
    public User Vendor { get; set; } = default!;

    public string Title { get; set; } = default!;

    public decimal BasePrice { get; set; } // optional but useful
    public string Currency { get; set; } = "LKR";

    public DateTime CreatedAt { get; set; }
}
