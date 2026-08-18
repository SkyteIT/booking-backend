using System.ComponentModel.DataAnnotations;

namespace Ube.Application.Features.Auth;

public class ConfirmSelfServiceTwoFactorDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
}
