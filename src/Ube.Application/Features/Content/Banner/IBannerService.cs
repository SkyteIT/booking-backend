
namespace Ube.Application.Features.Content.Banner;

public interface IBannerService
{
    Task<IReadOnlyList<BannerDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BannerDto>> GetActiveByPlacementAsync(
        Ube.Domain.Enums.Content.BannerPlacement placement,
        DateOnly? asOfDate,
        CancellationToken cancellationToken);
    Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken);
    Task<BannerDto?> UpdateImageAsync(Guid id, string imageUrl, CancellationToken cancellationToken);
    Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
