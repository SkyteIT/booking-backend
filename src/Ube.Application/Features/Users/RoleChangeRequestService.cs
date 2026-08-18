using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Admin.Dashboard;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

public class RoleChangeRequestService : IRoleChangeRequestService
{
    private readonly IRoleChangeRequestRepository _requestRepo;
    private readonly IAdminRepository _adminRepo;

    public RoleChangeRequestService(IRoleChangeRequestRepository requestRepo, IAdminRepository adminRepo)
    {
        _requestRepo = requestRepo;
        _adminRepo = adminRepo;
    }

    public async Task<RoleChangeRequestDto> CreateAsync(Guid targetUserId, UserRole requestedRole, Guid requestedByUserId, string? reason, CancellationToken ct = default)
    {
        var target = await _adminRepo.GetUserByIdAsync(targetUserId)
            ?? throw new NotFoundException($"User {targetUserId} not found.");

        var request = new RoleChangeRequest
        {
            Id = Guid.NewGuid(),
            TargetUserId = target.Id,
            RequestedByUserId = requestedByUserId,
            CurrentRole = target.Role,
            RequestedRole = requestedRole,
            Reason = reason,
            Status = RoleChangeRequestStatus.Pending
        };

        await _requestRepo.AddAsync(request, ct);
        request.TargetUser = target;
        return ToDto(request);
    }

    public async Task<PagedResult<RoleChangeRequestDto>> GetPagedAsync(RoleChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _requestRepo.GetPagedAsync(status, pageNumber, pageSize, ct);

        return new PagedResult<RoleChangeRequestDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<IReadOnlyList<RoleChangeRequestDto>> GetMineAsync(Guid requestedByUserId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.GetByRequesterAsync(requestedByUserId, ct);
        return requests.Select(ToDto).ToList();
    }

    public async Task<RoleChangeRequestDto> ApproveAsync(Guid requestId, Guid actorUserId, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException("Role change request not found");

        if (request.Status != RoleChangeRequestStatus.Pending)
            throw new BusinessRuleException("Only a pending request can be approved.");

        var adminOrSuperAdminCount = await _adminRepo.CountByRolesAsync(new[] { UserRole.Admin, UserRole.SuperAdmin });
        var check = RoleChangeRules.CanChangeRole(actorUserId, request.TargetUser, request.RequestedRole, adminOrSuperAdminCount);
        if (!check.IsSuccess)
            throw new BusinessRuleException(check.ErrorMessage);

        request.TargetUser.Role = request.RequestedRole;
        request.TargetUser.UpdatedAt = DateTime.UtcNow;
        await _adminRepo.UpdateUserAsync(request.TargetUser);
        await _adminRepo.SaveChangesAsync();

        request.Status = RoleChangeRequestStatus.Approved;
        request.ReviewedByUserId = actorUserId;
        request.ReviewedAt = DateTime.UtcNow;
        await _requestRepo.UpdateAsync(request, ct);

        return ToDto(request);
    }

    public async Task<RoleChangeRequestDto> RejectAsync(Guid requestId, Guid actorUserId, string? notes, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException("Role change request not found");

        if (request.Status != RoleChangeRequestStatus.Pending)
            throw new BusinessRuleException("Only a pending request can be rejected.");

        request.Status = RoleChangeRequestStatus.Rejected;
        request.ReviewedByUserId = actorUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = notes;
        await _requestRepo.UpdateAsync(request, ct);

        return ToDto(request);
    }

    private static RoleChangeRequestDto ToDto(RoleChangeRequest r) => new()
    {
        Id = r.Id,
        TargetUserId = r.TargetUserId,
        TargetUserName = r.TargetUser != null ? $"{r.TargetUser.FirstName} {r.TargetUser.LastName}" : string.Empty,
        TargetUserEmail = r.TargetUser?.Email ?? string.Empty,
        RequestedByUserId = r.RequestedByUserId,
        RequestedByUserName = r.RequestedByUser != null ? $"{r.RequestedByUser.FirstName} {r.RequestedByUser.LastName}" : string.Empty,
        CurrentRole = r.CurrentRole,
        RequestedRole = r.RequestedRole,
        Reason = r.Reason,
        Status = r.Status,
        ReviewedByUserId = r.ReviewedByUserId,
        ReviewedAt = r.ReviewedAt,
        ReviewNotes = r.ReviewNotes,
        CreatedAt = r.CreatedAt
    };
}
