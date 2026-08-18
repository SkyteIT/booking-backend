namespace Ube.Application.Common.Models;

public class QueryOptions
{
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}