using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Notifications;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Content;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Content.Banner;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notificationService;

    public BannerService(
        IBannerRepository repo,
        IUserRepository userRepo,
        INotificationService notificationService)
    {
        _repo = repo;
        _userRepo = userRepo;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<BannerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var banners = await _repo.GetAllAsync(cancellationToken);
        return banners.Select(ToDto).ToList();
    }

    public async Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var banner = await _repo.GetByIdAsync(id, cancellationToken);
        return banner is null ? null : ToDto(banner);
    }

    public async Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = new Ube.Domain.Entities.Content.Banner
        {
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = dto.ImageUrl,
            Placement = (BannerPlacement)dto.Placement,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = RecordStatus.Active
        };

        await _repo.AddAsync(banner, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        await NotifyCustomersAsync(
            NotificationType.CustomerSystemAnnouncement,
            "System announcement",
            banner.Title,
            cancellationToken);

        return ToDto(banner);
    }

    public async Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = await _repo.GetByIdAsync(id, cancellationToken);
        if (banner is null) return null;

        banner.Title = dto.Title;
        banner.Subtitle = dto.Subtitle;
        banner.ImageUrl = dto.ImageUrl;
        banner.Placement = (BannerPlacement)dto.Placement;
        banner.StartDate = dto.StartDate;
        banner.EndDate = dto.EndDate;
        banner.Status = (RecordStatus)dto.Status;
        banner.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);

        if (banner.Status == RecordStatus.Active)
        {
            await NotifyCustomersAsync(
                NotificationType.CustomerSystemAnnouncement,
                "System announcement",
                banner.Title,
                cancellationToken);
        }

        return ToDto(banner);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var banner = await _repo.GetByIdAsync(id, cancellationToken);
        if (banner is null) return false;

        await _repo.DeleteAsync(banner, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static BannerDto ToDto(Ube.Domain.Entities.Content.Banner x) => new()
    {
        Id = x.Id,
        Title = x.Title,
        Subtitle = x.Subtitle,
        ImageUrl = x.ImageUrl,
        Placement = x.Placement.ToString(),
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        Status = x.Status.ToString()
    };

    private async Task NotifyCustomersAsync(
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken)
    {
        var customers = await _userRepo.GetByRoleAsync(UserRole.User);

        foreach (var customer in customers)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = customer.Id,
                    Title = title,
                    Message = message,
                    Type = (int)type
                }, cancellationToken);
            }
            catch
            {
                // best-effort only
            }
        }
    }
}
