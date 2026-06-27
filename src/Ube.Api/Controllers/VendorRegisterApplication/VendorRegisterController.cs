using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;

namespace Ube.Api.Controllers.VendorRegistration;

[Authorize]
[ApiController]
[Route("api/vendor-register")]
public class VendorRegisterController : ControllerBase
{
    private readonly IVendorApplicationRepository _repo;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<VendorRegisterController> _logger;

    public VendorRegisterController(
        IVendorApplicationRepository repo,
        ICurrentUserService currentUser,
        ILogger<VendorRegisterController> logger)
    {
        _repo = repo;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitApplication(
        [FromForm] VendorRegisterApplicationDto dto,
        IFormFile? businessLicense,
        IFormFile? insuranceCertificate,
        IFormFile? taxDocument)
    {
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(uploadPath);

        async Task<string?> SaveFile(IFormFile? file)
        {
            if (file == null) return null;
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        var application = new VendorApplication
        {
            UserId = _currentUser.UserId,
            BusinessName = dto.BusinessName,
            BusinessType = dto.BusinessType,
            Address = dto.Address,
            Website = dto.Website,
            TaxId = dto.TaxId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Categories = dto.Categories != null ? string.Join(",", dto.Categories) : null,
            BusinessLicensePath = await SaveFile(businessLicense),
            InsuranceCertificatePath = await SaveFile(insuranceCertificate),
            TaxDocumentPath = await SaveFile(taxDocument),
            CurrentStep = dto.CurrentStep,
            Status = Ube.Domain.Enums.Vendors.VendorApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(application);

        _logger.LogInformation("Vendor application submitted by user {UserId}", _currentUser.UserId);

        return Ok(new { application.Id });
    }
}
