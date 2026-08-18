using System.ComponentModel.DataAnnotations;

namespace Ube.Application.Features.Auth;

public class DisableTwoFactorDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}
