using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Banner;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;

namespace Ube.Application.Services;

public class BannerService : IBannerService
{
    private readonly IAppDbContext _context;

    public BannerService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BannerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Banners
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new BannerDto
            {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Subtitle,
                ImageUrl = x.ImageUrl,
                Placement = x.Placement.ToString(),
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Banners
            .Where(x => x.Id == id)
            .Select(x => new BannerDto
            {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Subtitle,
                ImageUrl = x.ImageUrl,
                Placement = x.Placement.ToString(),
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = new Banner
        {
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = dto.ImageUrl,
            Placement = (BannerPlacement)dto.Placement,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = RecordStatus.Active
        };

        _context.Banners.Add(banner);
        await _context.SaveChangesAsync(cancellationToken);

        return new BannerDto
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            ImageUrl = banner.ImageUrl,
            Placement = banner.Placement.ToString(),
            StartDate = banner.StartDate,
            EndDate = banner.EndDate,
            Status = banner.Status.ToString()
        };
    }

    public async Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (banner is null) return null;

        banner.Title = dto.Title;
        banner.Subtitle = dto.Subtitle;
        banner.ImageUrl = dto.ImageUrl;
        banner.Placement = (BannerPlacement)dto.Placement;
        banner.StartDate = dto.StartDate;
        banner.EndDate = dto.EndDate;
        banner.Status = (RecordStatus)dto.Status;
        banner.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new BannerDto
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            ImageUrl = banner.ImageUrl,
            Placement = banner.Placement.ToString(),
            StartDate = banner.StartDate,
            EndDate = banner.EndDate,
            Status = banner.Status.ToString()
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (banner is null) return false;

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}