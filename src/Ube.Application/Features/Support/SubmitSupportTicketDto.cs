using System.ComponentModel.DataAnnotations;

namespace Ube.Application.Features.Support;

public class SubmitSupportTicketDto
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Message { get; set; } = string.Empty;
}
