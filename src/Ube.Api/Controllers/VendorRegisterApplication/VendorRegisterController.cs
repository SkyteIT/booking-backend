using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ube.Application.DTOs;
using Ube.Domain.Entities.Vendors;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers
{
    [ApiController]
    [Route("api/vendor-register")]
    [Authorize]
    public class VendorRegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VendorRegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitApplication(
            [FromForm] VendorRegisterApplicationDto dto,
            IFormFile? businessLicense,
            IFormFile? insuranceCertificate,
            IFormFile? taxDocument)
        {
            try 
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                Console.WriteLine($"[VendorRegister] Submitting for User: {userId}");
                Console.WriteLine($"[VendorRegister] BusinessName: {dto.BusinessName}");

                // 📁 create upload folder
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // 📁 helper method to save file
                async Task<string?> SaveFile(IFormFile? file)
                {
                    if (file == null) return null;
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return fileName;
                }

                var licensePath = await SaveFile(businessLicense);
                var insurancePath = await SaveFile(insuranceCertificate);
                var taxPath = await SaveFile(taxDocument);

                var app = new VendorRegisterApplication
                {
                    BusinessName = dto.BusinessName ?? "Unknown",
                    BusinessType = dto.BusinessType ?? "Unknown",
                    TaxId = dto.TaxId,
                    Website = dto.Website,
                    Address = dto.Address ?? "Unknown",
                    FirstName = dto.FirstName ?? "Unknown",
                    LastName = dto.LastName ?? "Unknown",
                    Email = dto.Email ?? "Unknown",
                    Phone = dto.Phone ?? "Unknown",
                    Categories = dto.Categories != null ? string.Join(",", dto.Categories) : "",
                    BusinessLicensePath = licensePath,
                    InsuranceCertificatePath = insurancePath,
                    TaxDocumentPath = taxPath,
                    IsSubmitted = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.VendorRegisterApplications.Add(app);

                var existingProfile = await _context.VendorProfiles.FirstOrDefaultAsync(v => v.UserId == userId);
                if (existingProfile == null)
                {
                    var vendorProfile = new VendorProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        BusinessName = dto.BusinessName ?? "New Vendor",
                        BusinessType = dto.BusinessType ?? "Other",
                        Description = "Vendor approved automatically from application.",
                        ContactNumber = dto.Phone ?? "",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.VendorProfiles.Add(vendorProfile);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Vendor application saved successfully");
                return Ok(app);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SubmitApplication: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}