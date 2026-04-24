using Ube.Domain.Enums.Users;


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
    public string Role { get; set; } = "User";
}