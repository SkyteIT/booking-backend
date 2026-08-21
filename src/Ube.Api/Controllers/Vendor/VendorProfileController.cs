using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Features.Vendors;
using Ube.Application.Features.Auth;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Exceptions;


namespace Ube.Api.Controllers.Vendor;

[Authorize(Roles = "Vendor")]
[ApiController]
[Route("api/vendor/profile")]
public class VendorProfileController : ControllerBase
{
    private readonly IVendorProfileService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;
    private readonly IFileStorageService _fileStorage;

    public VendorProfileController(IVendorProfileService service, ICurrentUserService currentUser, IAuthService authService, IFileStorageService fileStorage)
    {
        _service = service;
        _currentUser = currentUser;
        _authService = authService;
        _fileStorage = fileStorage;
    }

    // get vendor profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _currentUser.UserId;
        var result = await _service.GetVendorProfileAsync(userId);
        return Ok(result);
    }

    //  update vendor profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateVendorProfileDto dto)
    {
        var userId = _currentUser.UserId;
        var result = await _service.UpdateVendorProfileAsync(userId, dto);
        return Ok(result);
    }
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
    {
        var userId = _currentUser.UserId;
        var file = request.File;

        if (file == null || file.Length == 0)
            throw new BusinessRuleException("Invalid file");

        var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedTypes.Contains(extension))
            throw new BusinessRuleException("Only JPG/PNG files are allowed");

        const long maxFileSize = 2 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new BusinessRuleException("File size must not exceed 2MB");

        await using var stream = file.OpenReadStream();
        var imageUrl = await _fileStorage.UploadAsync(stream, extension, file.ContentType, IFileStorageService.ImagesContainer);

        // Use AuthService to update and return the CurrentUserDto so frontend gets the full user
        var user = await _authService.UpdateProfileImageAsync(userId, imageUrl);

        return Ok(user);
    }
}