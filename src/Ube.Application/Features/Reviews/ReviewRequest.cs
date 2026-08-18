using Ube.Application.Common.Models;
namespace Ube.Application.Features.Reviews;
public class ReviewRequest : QueryOptions
{
    public int? Rating { get; set; }
    
}