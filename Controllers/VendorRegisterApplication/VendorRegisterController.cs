using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.DTOs;
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
        [AllowAnonymous]
        public async Task<IActionResult> SubmitApplication(
    [FromForm] VendorRegisterApplicationDto dto,
    IFormFile? businessLicense,
    IFormFile? insuranceCertificate,
    IFormFile? taxDocument)
        {
            Console.WriteLine("🔥 STEP 1 PASSED - DIRECT FORM BINDING WORKING");

            try
            {
                // ======================
                // DTO CHECK
                // ======================
                if (dto == null)
                {
                    Console.WriteLine("❌ DTO IS NULL");
                    return BadRequest("DTO is null");
                }

                Console.WriteLine("🔥 2. DTO RECEIVED");
                Console.WriteLine($"BusinessName: {dto.BusinessName}");
                Console.WriteLine($"Email: {dto.Email}");
                Console.WriteLine($"Phone: {dto.Phone}");

                // ======================
                // UPLOAD FOLDER
                // ======================
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                    Console.WriteLine("📁 Upload folder created");
                }

                // ======================
                // FILE SAVE METHOD
                // ======================
                async Task<string?> SaveFile(IFormFile? file)
                {
                    if (file == null)
                        return null;

                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    Console.WriteLine($"📁 File saved: {fileName}");
                    return fileName;
                }

                // ======================
                // SAVE FILES
                // ======================
                Console.WriteLine("🔥 3. Saving files...");
                var licensePath = await SaveFile(businessLicense);
                var insurancePath = await SaveFile(insuranceCertificate);
                var taxPath = await SaveFile(taxDocument);

                // ======================
                // ENTITY CREATION
                // ======================
                Console.WriteLine("🔥 4. Creating entity...");

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

                    Categories = dto.Categories ?? "",

                    BusinessLicensePath = licensePath,
                    InsuranceCertificatePath = insurancePath,
                    TaxDocumentPath = taxPath,

                    IsSubmitted = true,
                    CreatedAt = DateTime.UtcNow
                };

                // ======================
                // DB SAVE
                // ======================
                Console.WriteLine("🔥 5. Adding to DB");

                _context.VendorRegisterApplications.Add(app);

                Console.WriteLine("🔥 6. Saving to database");

                await _context.SaveChangesAsync();

                Console.WriteLine("✅ 7. SUCCESS SAVED");

                return Ok(new
                {
                    message = "Application submitted successfully",
                    id = app.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR OCCURRED");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Submission failed",
                    error = ex.Message
                });
            }
        }
    }
}