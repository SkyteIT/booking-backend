public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!; 
    // "Vendor", "Customer", "Admin"

    public DateTime CreatedAt { get; set; } 
}