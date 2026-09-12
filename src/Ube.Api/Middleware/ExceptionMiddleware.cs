using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Exceptions;

namespace Ube.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        IEnumerable<string>? errors = null;

        switch (ex)
        {
            case ValidationException validationEx:
                statusCode = validationEx.StatusCode;
                message = validationEx.Message;
                errors = validationEx.Errors;
                break;

            case AppException appEx:
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                break;

            case DbUpdateConcurrencyException:
                statusCode = (int)HttpStatusCode.Conflict;
                message = "The record was modified by another user. Please reload and try again.";
                break;

            case DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx):
                statusCode = (int)HttpStatusCode.Conflict;
                message = "A record with this value already exists.";
                break;

            case DbUpdateException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Database update failed. Check your input values.";
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            case ArgumentException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.Conflict;
                message = ex.Message;
                break;

            default:
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred. Please try again later.";
                break;
        }

        var payload = new
        {
            message,
            errors = errors?.ToArray()
        };

        var result = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(result);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? string.Empty;
        return inner.Contains("UNIQUE") ||
               inner.Contains("duplicate key") ||
               inner.Contains("Violation of UNIQUE KEY constraint") ||
               inner.Contains("Cannot insert duplicate key");
    }
}
