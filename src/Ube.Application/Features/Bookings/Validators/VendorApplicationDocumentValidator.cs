using Microsoft.AspNetCore.Http;
using Ube.Application.Common.Exceptions;

namespace Ube.Application.Features.Bookings.Validators;

public static class VendorApplicationDocumentValidator
{
    private static readonly string[] AllowedTypes = { ".pdf", ".docx" };
    private const long MaxSize = 5 * 1024 * 1024; // 5MB

    public static void Validate(IFormFile? file, string label)
    {
        if (file == null || file.Length == 0)
            throw new BusinessRuleException($"{label} is required");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedTypes.Contains(extension))
            throw new BusinessRuleException($"{label} must be a PDF or DOCX file");

        if (file.Length > MaxSize)
            throw new BusinessRuleException($"{label} must not exceed 5MB");
    }
}
