using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Notifications;
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

    public async Task<IReadOnlyList<BannerDto>> GetActiveByPlacementAsync(
        BannerPlacement placement,
        DateOnly? asOfDate,
        CancellationToken cancellationToken)
    {
        var effectiveDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var banners = await _repo.GetActiveByPlacementAsync(placement, effectiveDate, cancellationToken);
        return banners.Select(ToDto).ToList();
    }

    public async Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var banner = await _repo.GetByIdAsync(id, cancellationToken);
        return banner is null ? null : ToDto(banner);
    }

    public async Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken)
    {
        ValidateDateRange(dto.StartDate, dto.EndDate);

        var banner = new Ube.Domain.Entities.Content.Banner
        {
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = dto.ImageUrl,
            Placement = (BannerPlacement)dto.Placement,
            DisplayOrder = dto.DisplayOrder,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = (RecordStatus)dto.Status
        };

        await _repo.AddAsync(banner, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        if (IsCurrentlyVisible(banner, DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            await NotifyCustomersAsync(
                NotificationType.CustomerSystemAnnouncement,
                "System announcement",
                banner.Title,
                cancellationToken);
        }

        return ToDto(banner);
    }

    public async Task<BannerDto?> UpdateImageAsync(Guid id, string imageUrl, CancellationToken cancellationToken)
    {
        var banner = await _repo.GetByIdAsync(id, cancellationToken);
        if (banner is null) return null;

        banner.ImageUrl = imageUrl;
        banner.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);

        return ToDto(banner);
    }

    public async Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = await _repo.GetByIdAsync(id, cancellationToken);
        if (banner is null) return null;

        ValidateDateRange(dto.StartDate, dto.EndDate);

        banner.Title = dto.Title;
        banner.Subtitle = dto.Subtitle;
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            banner.ImageUrl = dto.ImageUrl;
        }
        banner.Placement = (BannerPlacement)dto.Placement;
        banner.DisplayOrder = dto.DisplayOrder;
        banner.StartDate = dto.StartDate;
        banner.EndDate = dto.EndDate;
        banner.Status = (RecordStatus)dto.Status;
        banner.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);

        if (IsCurrentlyVisible(banner, DateOnly.FromDateTime(DateTime.UtcNow)))
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
        DisplayOrder = x.DisplayOrder,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        Status = x.Status.ToString(),
        LifecycleStatus = GetLifecycleStatus(x, DateOnly.FromDateTime(DateTime.UtcNow)).ToString(),
        IsVisible = IsCurrentlyVisible(x, DateOnly.FromDateTime(DateTime.UtcNow))
    };

    private static void ValidateDateRange(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
            throw new BusinessRuleException("End date must be after start date");
    }

    private static bool IsCurrentlyVisible(Ube.Domain.Entities.Content.Banner banner, DateOnly asOfDate)
        => banner.Status == RecordStatus.Active &&
           banner.StartDate <= asOfDate &&
           banner.EndDate >= asOfDate;

    private static BannerLifecycleStatus GetLifecycleStatus(Ube.Domain.Entities.Content.Banner banner, DateOnly asOfDate)
        => banner.Status != RecordStatus.Active
            ? BannerLifecycleStatus.Inactive
            : asOfDate < banner.StartDate
                ? BannerLifecycleStatus.Scheduled
                : asOfDate > banner.EndDate
                    ? BannerLifecycleStatus.Expired
                    : BannerLifecycleStatus.Active;

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
