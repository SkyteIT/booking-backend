
namespace Ube.Application.Common.Exceptions;
public abstract class AppException : Exception
{
    public int StatusCode { get; set; }
    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base(message, 400) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, 404) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(message, 403) { }
}