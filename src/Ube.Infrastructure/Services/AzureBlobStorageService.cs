using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Ube.Application.Common.Interfaces.Services;

namespace Ube.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(Stream content, string extension, string contentType, string containerName, CancellationToken ct = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobName = $"{Guid.NewGuid()}{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            ct);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (!TryParseBlobUrl(fileUrl, out var containerName, out var blobName)) return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: ct);
    }

    public string GetReadUrl(string fileUrl, TimeSpan expiresIn)
    {
        // Pre-migration/seeded rows can carry a path that isn't one of our
        // blobs at all (e.g. a placeholder https://example.com/... URL) -
        // there's nothing to sign, so hand it back unchanged rather than
        // throwing on a URL shape we don't own.
        if (!TryParseBlobUrl(fileUrl, out var containerName, out var blobName))
            return fileUrl;

        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("Cannot generate a SAS URL - the storage client was not created with an account key.");

        return blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiresIn)).ToString();
    }

    private bool TryParseBlobUrl(string fileUrl, out string containerName, out string blobName)
    {
        containerName = string.Empty;
        blobName = string.Empty;

        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri)) return false;
        if (!string.Equals(uri.Host, _blobServiceClient.Uri.Host, StringComparison.OrdinalIgnoreCase)) return false;
        if (uri.Segments.Length < 3) return false;

        // /{container}/{blobName} - segments[0] is "/".
        containerName = uri.Segments[1].TrimEnd('/');
        blobName = Uri.UnescapeDataString(string.Concat(uri.Segments[2..]));
        return true;
    }
}
