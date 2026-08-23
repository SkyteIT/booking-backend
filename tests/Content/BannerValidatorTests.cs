using Ube.Application.Features.Content.Banner;
using Ube.Application.Features.Content.Banner.Validators;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;

namespace Ube.Tests.Content;

public class BannerValidatorTests
{
    [Fact]
    public void CreateValidator_Accepts_Valid_Banner()
    {
        var validator = new CreateBannerDtoValidator();
        var dto = new CreateBannerDto
        {
            Title = "banner",
            ImageUrl = "/images/banner.jpg",
            Placement = (int)BannerPlacement.ExplorePage,
            DisplayOrder = 1,
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 31),
            Status = (int)RecordStatus.Active
        };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_Rejects_Invalid_Date_Range()
    {
        var validator = new CreateBannerDtoValidator();
        var dto = new CreateBannerDto
        {
            Title = "banner",
            ImageUrl = "/images/banner.jpg",
            Placement = (int)BannerPlacement.ExplorePage,
            DisplayOrder = 1,
            StartDate = new DateOnly(2026, 8, 31),
            EndDate = new DateOnly(2026, 8, 1),
            Status = (int)RecordStatus.Active
        };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("End date must be after start date"));
    }

    [Fact]
    public void UpdateValidator_Rejects_Invalid_Placement_And_Status()
    {
        var validator = new UpdateBannerDtoValidator();
        var dto = new UpdateBannerDto
        {
            Title = "banner",
            ImageUrl = "/images/banner.jpg",
            Placement = 999,
            DisplayOrder = -1,
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 31),
            Status = 999
        };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Placement is required");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Status is required");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Display order must be zero or a positive number");
    }

    [Fact]
    public void UpdateValidator_Allows_Omitted_ImageUrl_When_Editing_Only_Metadata()
    {
        var validator = new UpdateBannerDtoValidator();
        var dto = new UpdateBannerDto
        {
            Title = "banner",
            ImageUrl = null,
            Placement = (int)BannerPlacement.ExplorePage,
            DisplayOrder = 0,
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 31),
            Status = (int)RecordStatus.Active
        };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}
