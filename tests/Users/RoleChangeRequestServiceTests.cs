using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Admin.Dashboard;
using Ube.Application.Features.Users;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Tests.Users;

public class RoleChangeRequestServiceTests
{
    private sealed record Ctx(
        Mock<IRoleChangeRequestRepository> RequestRepo,
        Mock<IAdminRepository> AdminRepo,
        RoleChangeRequestService Service);

    private static Ctx Build()
    {
        var requestRepo = new Mock<IRoleChangeRequestRepository>();
        var adminRepo = new Mock<IAdminRepository>();
        var service = new RoleChangeRequestService(requestRepo.Object, adminRepo.Object);
        return new Ctx(requestRepo, adminRepo, service);
    }

    private static User MakeUser(UserRole role) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Test",
        LastName = "User",
        Email = "test@ube.local",
        Role = role
    };

    [Fact]
    public async Task CreateAsync_Creates_Pending_Request_Without_Touching_The_Role()
    {
        var ctx = Build();
        var target = MakeUser(UserRole.Vendor);
        ctx.AdminRepo.Setup(r => r.GetUserByIdAsync(target.Id)).ReturnsAsync(target);

        var dto = await ctx.Service.CreateAsync(target.Id, UserRole.Admin, Guid.NewGuid(), "please promote");

        Assert.Equal(RoleChangeRequestStatus.Pending, dto.Status);
        Assert.Equal(UserRole.Vendor, dto.CurrentRole);
        Assert.Equal(UserRole.Admin, dto.RequestedRole);
        Assert.Equal(UserRole.Vendor, target.Role); // unchanged
        ctx.RequestRepo.Verify(r => r.AddAsync(It.IsAny<RoleChangeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        ctx.AdminRepo.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    private static RoleChangeRequest MakePendingRequest(User target, UserRole requestedRole) => new()
    {
        Id = Guid.NewGuid(),
        TargetUserId = target.Id,
        TargetUser = target,
        RequestedByUserId = Guid.NewGuid(),
        CurrentRole = target.Role,
        RequestedRole = requestedRole,
        Status = RoleChangeRequestStatus.Pending
    };

    [Fact]
    public async Task ApproveAsync_Applies_The_Role_Change()
    {
        var ctx = Build();
        var target = MakeUser(UserRole.Vendor);
        var request = MakePendingRequest(target, UserRole.Admin);
        ctx.RequestRepo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        ctx.AdminRepo.Setup(r => r.CountByRolesAsync(It.IsAny<IEnumerable<UserRole>>())).ReturnsAsync(2);

        var dto = await ctx.Service.ApproveAsync(request.Id, Guid.NewGuid());

        Assert.Equal(RoleChangeRequestStatus.Approved, dto.Status);
        Assert.Equal(UserRole.Admin, target.Role);
        ctx.AdminRepo.Verify(r => r.UpdateUserAsync(target), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_Rejects_Self_Approval_Of_Own_Demotion()
    {
        var ctx = Build();
        var actorId = Guid.NewGuid();
        var target = MakeUser(UserRole.SuperAdmin);
        target.Id = actorId; // approver is the request's own target
        var request = MakePendingRequest(target, UserRole.Admin);
        ctx.RequestRepo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        ctx.AdminRepo.Setup(r => r.CountByRolesAsync(It.IsAny<IEnumerable<UserRole>>())).ReturnsAsync(2);

        await Assert.ThrowsAsync<BusinessRuleException>(() => ctx.Service.ApproveAsync(request.Id, actorId));
        Assert.Equal(UserRole.SuperAdmin, target.Role); // unchanged
    }

    [Fact]
    public async Task ApproveAsync_Rejects_Demoting_The_Last_Admin_Or_SuperAdmin()
    {
        var ctx = Build();
        var target = MakeUser(UserRole.Admin);
        var request = MakePendingRequest(target, UserRole.Vendor);
        ctx.RequestRepo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        ctx.AdminRepo.Setup(r => r.CountByRolesAsync(It.IsAny<IEnumerable<UserRole>>())).ReturnsAsync(1);

        await Assert.ThrowsAsync<BusinessRuleException>(() => ctx.Service.ApproveAsync(request.Id, Guid.NewGuid()));
        Assert.Equal(UserRole.Admin, target.Role); // unchanged
    }

    [Fact]
    public async Task RejectAsync_Leaves_The_Role_Untouched()
    {
        var ctx = Build();
        var target = MakeUser(UserRole.Vendor);
        var request = MakePendingRequest(target, UserRole.Admin);
        ctx.RequestRepo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var dto = await ctx.Service.RejectAsync(request.Id, Guid.NewGuid(), "not needed");

        Assert.Equal(RoleChangeRequestStatus.Rejected, dto.Status);
        Assert.Equal("not needed", dto.ReviewNotes);
        Assert.Equal(UserRole.Vendor, target.Role);
        ctx.AdminRepo.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAsync_Rejects_Already_Reviewed_Request()
    {
        var ctx = Build();
        var target = MakeUser(UserRole.Vendor);
        var request = MakePendingRequest(target, UserRole.Admin);
        request.Status = RoleChangeRequestStatus.Rejected;
        ctx.RequestRepo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);

        await Assert.ThrowsAsync<BusinessRuleException>(() => ctx.Service.ApproveAsync(request.Id, Guid.NewGuid()));
    }
}
