namespace Ube.Application.Features.Reviews;
public class CreateReviewDto
{
    public Guid BookingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}