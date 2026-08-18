using Microsoft.AspNetCore.Http;

namespace Ube.Application.Features.Vendors;

public class UploadImageRequest
{
    public IFormFile File { get; set; } = null!;
}
