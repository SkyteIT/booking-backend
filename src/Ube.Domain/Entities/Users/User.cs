namespace Ube.Domain.Entities;

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

    public string AuthProvider { get; set; } = "Local";

    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}