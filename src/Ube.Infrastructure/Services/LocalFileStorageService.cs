using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ube.Application.Common.Interfaces.Services;

namespace Ube.Infrastructure.Services;

// Local dev stand-in for AzureBlobStorageService - saves under wwwroot/uploads and
// serves through ASP.NET's static file middleware instead of Azure Blob Storage.
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IWebHostEnvironment env, IConfiguration config)
    {
        _webRootPath = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _baseUrl = (config["LocalStorage:BaseUrl"] ?? "http://localhost:5037").TrimEnd('/');
    }

    public async Task<string> UploadAsync(Stream content, string extension, string contentType, string containerName, CancellationToken ct = default)
    {
        var containerPath = Path.Combine(_webRootPath, "uploads", containerName);
        Directory.CreateDirectory(containerPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(containerPath, fileName);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, ct);

        return $"{_baseUrl}/uploads/{containerName}/{fileName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (TryParseLocalPath(fileUrl, out var relativePath))
        {
            var filePath = Path.Combine(_webRootPath, relativePath);
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    public string GetReadUrl(string fileUrl, TimeSpan expiresIn) => fileUrl;

    private static bool TryParseLocalPath(string fileUrl, out string relativePath)
    {
        relativePath = string.Empty;
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri)) return false;

        relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return true;
    }
}
