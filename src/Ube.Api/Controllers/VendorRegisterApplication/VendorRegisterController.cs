using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.DTOs;
using Ube.Domain.Entities;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers
{
    [ApiController]
    [Route("api/vendor-register")]
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
            Console.WriteLine("🔥 SubmitApplication API HIT");
            Console.WriteLine($"BusinessName: {dto.BusinessName}");

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

            // 📁 save files
            var licensePath = await SaveFile(businessLicense);
            var insurancePath = await SaveFile(insuranceCertificate);
            var taxPath = await SaveFile(taxDocument);

            // 💾 save to DB
            var app = new VendorRegisterApplication
            {
                BusinessName = dto.BusinessName,
                BusinessType = dto.BusinessType,
                TaxId = dto.TaxId,
                Website = dto.Website,
                Address = dto.Address,

                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,

                Categories = dto.Categories != null
                    ? string.Join(",", dto.Categories)
                    : "",

                BusinessLicensePath = licensePath,
                InsuranceCertificatePath = insurancePath,
                TaxDocumentPath = taxPath,

                IsSubmitted = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.VendorRegisterApplications.Add(app);

            Console.WriteLine("💾 Saving...");

            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Saved");

            return Ok(app);
        }
    }
}