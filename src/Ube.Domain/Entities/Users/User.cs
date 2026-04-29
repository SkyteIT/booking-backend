
using Ube.Domain.Enums; 
using Ube.Domain.Enums.Users;      
 

using Ube.Domain.Entities.Carts;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Vendors;


namespace Ube.Domain.Entities.Users;

public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsEmailVerified { get; set; } = false;

    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;

    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;



    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<VendorProfile> VendorProfiles { get; set; } = new List<VendorProfile>();
}