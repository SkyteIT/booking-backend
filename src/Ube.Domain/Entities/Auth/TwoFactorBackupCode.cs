namespace Ube.Domain.Entities.Auth;
public class TwoFactorBackupCode
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // BCrypt hash - the plaintext code is shown to the user once, at
    // generation time, and never stored or recoverable afterward.
    public string CodeHash { get; set; } = string.Empty;

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UsedAt { get; set; }
}
