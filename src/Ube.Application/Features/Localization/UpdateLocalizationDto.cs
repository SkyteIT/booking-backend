namespace Ube.Application.Features.Localization;
public class UpdateLocalizationDto
{
    public string Language { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}