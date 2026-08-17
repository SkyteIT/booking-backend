using Ube.Application.Common.Models;

namespace Ube.Application.Features.Questions;

public class AskQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
}

public class AnswerQuestionDto
{
    public string AnswerText { get; set; } = string.Empty;
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

// Paging only - no rating-style filter, unlike ReviewRequest.
public class QuestionRequest : QueryOptions
{
}
