using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Exceptions;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Application.Features.Vendors;

public record CreateCategoryRequestDto(Guid CategoryId, string? Reason);
public record ReviewCategoryRequestDto(CategoryRequestStatus Status, string? AdminNote);

/// <summary>
/// Returned to both vendor (my requests) and admin (all requests).
/// VendorName / VendorEmail are populated from the Users join – the
/// admin page uses them to show who made the request.
/// </summary>
public record CategoryRequestDto(
    Guid Id,
    Guid UserId,
    string VendorName,
    string VendorEmail,
    Guid CategoryId,
    string CategoryName,
    CategoryRequestStatus Status,
    string? Reason,
    string? AdminNote,
    DateTime CreatedAt
);

public interface IVendorCategoryRequestService
{
    Task<CategoryRequestDto> CreateAsync(Guid userId, CreateCategoryRequestDto dto, CancellationToken ct);
    Task<IReadOnlyList<CategoryRequestDto>> GetMineAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<CategoryRequestDto>> GetAllAsync(CategoryRequestStatus? status, CancellationToken ct);
    Task ReviewAsync(Guid id, Guid adminId, ReviewCategoryRequestDto dto, CancellationToken ct);
}

public class VendorCategoryRequestService(IAppDbContext db) : IVendorCategoryRequestService
{
    public async Task<CategoryRequestDto> CreateAsync(Guid userId, CreateCategoryRequestDto dto, CancellationToken ct)
    {
        var c = await db.Categories
            .SingleOrDefaultAsync(x => x.Id == dto.CategoryId && x.Status == RecordStatus.Active && x.Type.HasValue, ct)
            ?? throw new BusinessRuleException("The selected category is not active.");

        var a = await db.VendorApplications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync(ct);

        if (a?.Status != VendorApplicationStatus.Approved)
            throw new BusinessRuleException("Only approved vendors can request additional categories.");

        var s = (a.Categories ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (s.Contains(c.Id.ToString(), StringComparer.OrdinalIgnoreCase) || s.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
            throw new BusinessRuleException("This category is already approved for your account.");

        if (await db.VendorCategoryRequests.AnyAsync(x => x.UserId == userId && x.CategoryId == dto.CategoryId && x.Status == CategoryRequestStatus.Pending, ct))
            throw new BusinessRuleException("A request for this category is already pending.");

        var r = new VendorCategoryRequest { UserId = userId, CategoryId = dto.CategoryId, Reason = dto.Reason };
        db.VendorCategoryRequests.Add(r);
        await db.SaveChangesAsync(ct);

        var user = await db.Users.FindAsync([userId], ct);
        var vendorName = user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim();
        var vendorEmail = user?.Email ?? "";

        return new CategoryRequestDto(r.Id, r.UserId, vendorName, vendorEmail, r.CategoryId, c.Name, r.Status, r.Reason, r.AdminNote, r.CreatedAt);
    }

    public async Task<IReadOnlyList<CategoryRequestDto>> GetMineAsync(Guid id, CancellationToken ct) =>
        await (from r in db.VendorCategoryRequests
               where r.UserId == id
               join c in db.Categories on r.CategoryId equals c.Id
               join u in db.Users on r.UserId equals u.Id
               orderby r.CreatedAt descending
               select new CategoryRequestDto(
                   r.Id,
                   r.UserId,
                   u.FirstName + " " + u.LastName,
                   u.Email,
                   r.CategoryId,
                   c.Name,
                   r.Status,
                   r.Reason,
                   r.AdminNote,
                   r.CreatedAt
               )).ToListAsync(ct);

    public async Task<IReadOnlyList<CategoryRequestDto>> GetAllAsync(CategoryRequestStatus? s, CancellationToken ct) =>
        await (from r in db.VendorCategoryRequests
               where !s.HasValue || r.Status == s
               join c in db.Categories on r.CategoryId equals c.Id
               join u in db.Users on r.UserId equals u.Id
               orderby r.CreatedAt descending
               select new CategoryRequestDto(
                   r.Id,
                   r.UserId,
                   u.FirstName + " " + u.LastName,
                   u.Email,
                   r.CategoryId,
                   c.Name,
                   r.Status,
                   r.Reason,
                   r.AdminNote,
                   r.CreatedAt
               )).ToListAsync(ct);

    public async Task ReviewAsync(Guid id, Guid adminId, ReviewCategoryRequestDto dto, CancellationToken ct)
    {
        if (dto.Status == CategoryRequestStatus.Pending)
            throw new BusinessRuleException("Review status must be Approved or Rejected.");

        var r = await db.VendorCategoryRequests.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Category request not found.");

        if (r.Status != CategoryRequestStatus.Pending)
            throw new BusinessRuleException("This category request has already been reviewed.");

        r.Status = dto.Status;
        r.AdminNote = dto.AdminNote;
        r.ReviewedBy = adminId;
        r.ReviewedAt = DateTime.UtcNow;

        if (dto.Status == CategoryRequestStatus.Approved)
        {
            var a = await db.VendorApplications
                .Where(x => x.UserId == r.UserId)
                .OrderByDescending(x => x.SubmittedAt)
                .FirstOrDefaultAsync(ct)
                ?? throw new BusinessRuleException("Vendor application not found.");

            a.Categories = string.Join(',',
                (a.Categories ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Append(r.CategoryId.ToString())
                    .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        await db.SaveChangesAsync(ct);
    }
}
