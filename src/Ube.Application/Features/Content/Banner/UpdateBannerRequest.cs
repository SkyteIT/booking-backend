using Microsoft.AspNetCore.Http;

namespace Ube.Application.Features.Content.Banner;

public class UpdateBannerRequest
{
    public string Data { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
