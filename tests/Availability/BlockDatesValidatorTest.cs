using Xunit;

namespace Ube.Tests.Availability;

public class BlockDatesValidatorTests
{
    [Fact]
    public void Should_Fail_When_Dates_Are_Empty()
    {
        var validator = new BlockDatesValidator();

        var request = new BlockDatesRequest
        {
            Dates = new List<DateTime>()
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Fail_When_Date_Is_In_Past()
    {
        var validator = new BlockDatesValidator();

        var request = new BlockDatesRequest
        {
            Dates = new List<DateTime>
            {
                DateTime.UtcNow.Date.AddDays(-1)
            }
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains("past", result.Errors[0].ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Should_Succeed_When_Dates_Are_Valid()
    {
        var validator = new BlockDatesValidator();

        var request = new BlockDatesRequest
        {
            Dates = new List<DateTime>
            {
                DateTime.UtcNow.Date.AddDays(5)
            }
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }
}