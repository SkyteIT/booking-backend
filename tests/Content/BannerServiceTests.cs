using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Content.Banner;
using Ube.Application.Features.Notifications;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;
using Ube.Domain.Enums.Users;

namespace Ube.Tests.Content;

public class BannerServiceTests
{
    private sealed class InMemoryBannerRepository : IBannerRepository
    {
        private readonly List<Banner> _items = new();

        public Task<IReadOnlyList<Banner>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Banner>>(_items.OrderByDescending(x => x.CreatedAt).ToList());

        public Task<IReadOnlyList<Banner>> GetActiveByPlacementAsync(BannerPlacement placement, DateOnly asOfDate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Banner>>(_items
                .Where(x => x.Status == RecordStatus.Active
                            && x.Placement == placement
                            && x.StartDate <= asOfDate
                            && x.EndDate >= asOfDate)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.CreatedAt)
                .ToList());

        public Task<Banner?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(Banner banner, CancellationToken ct = default)
        {
            if (banner.Id == Guid.Empty)
                banner.Id = Guid.NewGuid();

            if (banner.CreatedAt == default)
                banner.CreatedAt = DateTime.UtcNow;

            _items.Add(banner);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Banner banner, CancellationToken ct = default)
        {
            _items.Remove(banner);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;

        public void Seed(params Banner[] banners) => _items.AddRange(banners);
    }

    private static BannerService BuildService(
        InMemoryBannerRepository repo,
        Mock<IUserRepository> userRepo,
        Mock<INotificationService> notificationService)
    {
        // Default so NotifyCustomersAsync's foreach has a real (empty) list
        // to iterate instead of Moq's null default - individual tests can
        // still override this setup if they need specific customers.
        userRepo.Setup(r => r.GetByRoleAsync(It.IsAny<UserRole>()))
            .ReturnsAsync(new List<Ube.Domain.Entities.Users.User>());

        return new(repo, userRepo.Object, notificationService.Object);
    }

    private static Banner MakeBanner(
        string title,
        BannerPlacement placement,
        int displayOrder,
        DateOnly startDate,
        DateOnly endDate,
        RecordStatus status = RecordStatus.Active,
        DateTime? createdAt = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Subtitle = null,
            ImageUrl = $"/images/{title}.jpg",
            Placement = placement,
            DisplayOrder = displayOrder,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };

    [Fact]
    public async Task GetActiveByPlacementAsync_Returns_Only_Visible_Banner_For_Selected_Placement()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repo = new InMemoryBannerRepository();
        repo.Seed(
            MakeBanner("active", BannerPlacement.ExplorePage, 1, today.AddDays(-1), today.AddDays(1)),
            MakeBanner("wrong-placement", BannerPlacement.SearchResults, 1, today.AddDays(-1), today.AddDays(1)),
            MakeBanner("future", BannerPlacement.ExplorePage, 1, today.AddDays(1), today.AddDays(10)));

        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var result = await service.GetActiveByPlacementAsync(BannerPlacement.ExplorePage, today, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("active", result[0].Title);
        Assert.Equal("Active", result[0].LifecycleStatus);
        Assert.True(result[0].IsVisible);
    }

    [Fact]
    public async Task GetActiveByPlacementAsync_Returns_Banners_In_Display_Order_For_Rotation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repo = new InMemoryBannerRepository();
        repo.Seed(
            MakeBanner("summer-sale", BannerPlacement.ExplorePage, 2, today.AddDays(-1), today.AddDays(1), RecordStatus.Active, DateTime.UtcNow.AddMinutes(-10)),
            MakeBanner("special-discount", BannerPlacement.ExplorePage, 1, today.AddDays(-1), today.AddDays(1), RecordStatus.Active, DateTime.UtcNow));

        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var result = await service.GetActiveByPlacementAsync(BannerPlacement.ExplorePage, today, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("special-discount", result[0].Title);
        Assert.Equal("summer-sale", result[1].Title);
    }

    [Fact]
    public async Task GetAllAsync_Shows_Scheduled_And_Expired_Statuses()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repo = new InMemoryBannerRepository();
        repo.Seed(
            MakeBanner("scheduled", BannerPlacement.LandingPage, 1, today.AddDays(1), today.AddDays(5)),
            MakeBanner("expired", BannerPlacement.CategoryPage, 1, today.AddDays(-10), today.AddDays(-1)));

        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Contains(result, x => x.Title == "scheduled" && x.LifecycleStatus == "Scheduled" && !x.IsVisible);
        Assert.Contains(result, x => x.Title == "expired" && x.LifecycleStatus == "Expired" && !x.IsVisible);
    }

    [Fact]
    public async Task CreateAsync_Persists_Banner_With_Selected_Placement_And_Status()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repo = new InMemoryBannerRepository();
        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var dto = new CreateBannerDto
        {
            Title = "seasonal",
            ImageUrl = "/images/seasonal.jpg",
            Placement = (int)BannerPlacement.ExplorePage,
            DisplayOrder = 2,
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(1),
            Status = (int)RecordStatus.Active
        };

        var result = await service.CreateAsync(dto, CancellationToken.None);

        Assert.Equal("seasonal", result.Title);
        Assert.Equal("ExplorePage", result.Placement);
        Assert.Equal(2, result.DisplayOrder);
        Assert.Equal("Active", result.LifecycleStatus);
        Assert.True(result.IsVisible);
        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task CreateAsync_Rejects_Invalid_Date_Range()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repo = new InMemoryBannerRepository();
        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var dto = new CreateBannerDto
        {
            Title = "bad-range",
            ImageUrl = "/images/bad.jpg",
            Placement = (int)BannerPlacement.LandingPage,
            DisplayOrder = 0,
            StartDate = today,
            EndDate = today,
            Status = (int)RecordStatus.Active
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Updates_Placement_And_Status_For_Existing_Banner()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = MakeBanner("existing", BannerPlacement.LandingPage, 1, today.AddDays(-10), today.AddDays(-5), RecordStatus.Inactive);
        var repo = new InMemoryBannerRepository();
        repo.Seed(existing);
        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var dto = new UpdateBannerDto
        {
            Title = "updated",
            ImageUrl = "/images/updated.jpg",
            Placement = (int)BannerPlacement.SearchResults,
            DisplayOrder = 3,
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(2),
            Status = (int)RecordStatus.Active
        };

        var result = await service.UpdateAsync(existing.Id, dto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("updated", result!.Title);
        Assert.Equal("SearchResults", result.Placement);
        Assert.Equal(3, result.DisplayOrder);
        Assert.Equal("Active", result.LifecycleStatus);
        Assert.True(result.IsVisible);
        Assert.Equal(BannerPlacement.SearchResults, existing.Placement);
        Assert.Equal(RecordStatus.Active, existing.Status);
        Assert.NotNull(existing.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_Without_ImageUrl_Keeps_Existing_Image()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = MakeBanner("existing", BannerPlacement.LandingPage, 1, today.AddDays(-2), today.AddDays(2), RecordStatus.Active);
        var originalImage = existing.ImageUrl;
        var repo = new InMemoryBannerRepository();
        repo.Seed(existing);
        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var dto = new UpdateBannerDto
        {
            Title = "updated",
            ImageUrl = null,
            Placement = (int)BannerPlacement.ExplorePage,
            DisplayOrder = 4,
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(3),
            Status = (int)RecordStatus.Active
        };

        var result = await service.UpdateAsync(existing.Id, dto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(originalImage, existing.ImageUrl);
        Assert.Equal(originalImage, result!.ImageUrl);
    }

    [Fact]
    public async Task UpdateImageAsync_Replaces_Only_The_ImageUrl()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = MakeBanner("existing", BannerPlacement.LandingPage, 1, today.AddDays(-2), today.AddDays(2), RecordStatus.Active);
        var repo = new InMemoryBannerRepository();
        repo.Seed(existing);
        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var result = await service.UpdateImageAsync(existing.Id, "/images/banners/new-image.jpg", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("/images/banners/new-image.jpg", existing.ImageUrl);
        Assert.Equal("/images/banners/new-image.jpg", result!.ImageUrl);
    }

    [Fact]
    public async Task GetActiveByPlacementAsync_Returns_Empty_For_Wrong_Placement()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repo = new InMemoryBannerRepository();
        repo.Seed(MakeBanner("landing", BannerPlacement.LandingPage, 1, today.AddDays(-1), today.AddDays(1)));

        var userRepo = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = BuildService(repo, userRepo, notificationService);

        var result = await service.GetActiveByPlacementAsync(BannerPlacement.EventPage, today, CancellationToken.None);

        Assert.Empty(result);
    }
}
