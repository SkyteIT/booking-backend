namespace Ube.Domain.Entities.Auth;

// "Remember this device" - skips the 2FA challenge on this browser for a
// sliding window after a successful code verification, the same pattern
// WhatsApp Web uses. Only the SHA-256 hash of the raw token is stored;
// the raw token itself lives only in the client's browser storage.
public class TrustedDevice
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
