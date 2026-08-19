using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Admin.Dashboard;
using Ube.Application.Features.Users;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Tests.Admin;

public class AdminServiceTests
{
    private sealed record Ctx(
        Mock<IAdminRepository> AdminRepo,
        Mock<IRoleChangeRequestService> RoleChangeRequestService,
        Mock<IVendorProfileRepository> VendorProfileRepo,
        AdminService Service);

    private static Ctx Build()
    {
        var adminRepo = new Mock<IAdminRepository>();
        var roleChangeRequestService = new Mock<IRoleChangeRequestService>();
        var vendorProfileRepo = new Mock<IVendorProfileRepository>();
        var service = new AdminService(adminRepo.Object, roleChangeRequestService.Object, vendorProfileRepo.Object);
        return new Ctx(adminRepo, roleChangeRequestService, vendorProfileRepo, service);
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
    public async Task UpdateUserRoleAsync_SuperAdmin_Actor_Applies_Immediately()
    {
        var ctx = Build();
        var actor = MakeUser(UserRole.SuperAdmin);
        var target = MakeUser(UserRole.Vendor);
        ctx.AdminRepo.Setup(r => r.GetUserByIdAsync(target.Id)).ReturnsAsync(target);
        ctx.AdminRepo.Setup(r => r.GetUserByIdAsync(actor.Id)).ReturnsAsync(actor);
        ctx.AdminRepo.Setup(r => r.CountByRolesAsync(It.IsAny<IEnumerable<UserRole>>())).ReturnsAsync(2);

        var outcome = await ctx.Service.UpdateUserRoleAsync(target.Id, UserRole.Admin, actor.Id);

        Assert.True(outcome.AppliedImmediately);
        Assert.NotNull(outcome.User);
        Assert.Null(outcome.Request);
        Assert.Equal(UserRole.Admin, target.Role);
        ctx.RoleChangeRequestService.Verify(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<UserRole>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_PlainAdmin_Actor_Creates_A_Request_Instead()
    {
        var ctx = Build();
        var actor = MakeUser(UserRole.Admin);
        var target = MakeUser(UserRole.Vendor);
        ctx.AdminRepo.Setup(r => r.GetUserByIdAsync(target.Id)).ReturnsAsync(target);
        ctx.AdminRepo.Setup(r => r.GetUserByIdAsync(actor.Id)).ReturnsAsync(actor);
        ctx.RoleChangeRequestService
            .Setup(s => s.CreateAsync(target.Id, UserRole.Admin, actor.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleChangeRequestDto { Status = Ube.Domain.Enums.Users.RoleChangeRequestStatus.Pending });

        var outcome = await ctx.Service.UpdateUserRoleAsync(target.Id, UserRole.Admin, actor.Id);

        Assert.False(outcome.AppliedImmediately);
        Assert.NotNull(outcome.Request);
        Assert.Null(outcome.User);
        Assert.Equal(UserRole.Vendor, target.Role); // unchanged
        ctx.AdminRepo.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_SuperAdmin_Actor_Rejects_Self_Demotion()
    {
        var ctx = Build();
        var actor = MakeUser(UserRole.SuperAdmin);
        ctx.AdminRepo.Setup(r => r.GetUserByIdAsync(actor.Id)).ReturnsAsync(actor);
        ctx.AdminRepo.Setup(r => r.CountByRolesAsync(It.IsAny<IEnumerable<UserRole>>())).ReturnsAsync(2);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.UpdateUserRoleAsync(actor.Id, UserRole.Admin, actor.Id));
    }
}
