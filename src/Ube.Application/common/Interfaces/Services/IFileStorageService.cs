namespace Ube.Application.Common.Interfaces.Services;

public interface IFileStorageService
{
    // Public: profile images, banners - anything shown to any visitor.
    const string ImagesContainer = "images";
    // Private: vendor KYC documents (business license, insurance, tax) - not anonymously readable.
    const string DocumentsContainer = "documents";

    Task<string> UploadAsync(Stream content, string extension, string contentType, string containerName, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);

    // Signs a temporary read URL for a private blob (e.g. vendor KYC documents) -
    // the stored path is never itself a valid, permanently-reachable URL.
    string GetReadUrl(string fileUrl, TimeSpan expiresIn);
}
