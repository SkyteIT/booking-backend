using Ube.Application.Common.Interfaces.Services;

namespace Ube.Api.Services;

/// <summary>Stores development uploads under wwwroot instead of requiring Azure credentials.</summary>
public sealed class DevelopmentFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    public async Task<string> UploadAsync(
        Stream content,
        string extension,
        string contentType,
        string containerName,
        CancellationToken ct = default)
    {
        var safeContainer = containerName == IFileStorageService.DocumentsContainer
            ? IFileStorageService.DocumentsContainer
            : IFileStorageService.ImagesContainer;
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var directory = Path.Combine(environment.WebRootPath, "uploads", safeContainer);
        Directory.CreateDirectory(directory);

        await using var output = File.Create(Path.Combine(directory, fileName));
        await content.CopyToAsync(output, ct);
        return $"/uploads/{safeContainer}/{fileName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (!TryGetLocalPath(fileUrl, out var path))
            return Task.CompletedTask;

        File.Delete(path);
        return Task.CompletedTask;
    }

    public string GetReadUrl(string fileUrl, TimeSpan expiresIn) => fileUrl;

    private bool TryGetLocalPath(string fileUrl, out string path)
    {
        path = string.Empty;
        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var uploadsRoot = Path.GetFullPath(Path.Combine(environment.WebRootPath, "uploads"));
        var candidate = Path.GetFullPath(Path.Combine(environment.WebRootPath, relativePath));
        if (!candidate.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return false;

        path = candidate;
        return true;
    }
}
