using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ube.Application.Features.Vendors;

public class SubmitVendorApplicationRequest
{
    [Required]
    public string BusinessName { get; set; } = string.Empty;

    [Required]
    public string BusinessType { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public string? Website { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = new();

    [Required]
    public IFormFile BusinessLicense { get; set; } = null!;

    [Required]
    public IFormFile InsuranceCertificate { get; set; } = null!;

    [Required]
    public IFormFile TaxDocument { get; set; } = null!;
}
