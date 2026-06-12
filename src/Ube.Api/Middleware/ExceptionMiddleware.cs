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

        switch (ex)
        {
            // Our own typed application exceptions — highest priority
            case AppException appEx:
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                break;

            // Optimistic concurrency conflict (RowVersion on Booking etc.)
            case DbUpdateConcurrencyException:
                statusCode = (int)HttpStatusCode.Conflict;
                message = "The record was modified by another user. Please reload and try again.";
                break;

            // Unique-constraint violation from DB
            case DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx):
                statusCode = (int)HttpStatusCode.Conflict;
                message = "A record with this value already exists.";
                break;

            // Other EF save failures
            case DbUpdateException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Database update failed. Check your input values.";
                break;

            // Resource not found (thrown by services without using NotFoundException)
            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            // Bad arguments from caller code
            case ArgumentException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            // Business rule violations expressed as InvalidOperationException
            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.Conflict;
                message = ex.Message;
                break;

            // Anything else — log internally, never leak details to client
            default:
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred. Please try again later.";
                break;
        }

        var result = JsonSerializer.Serialize(new { message }, new JsonSerializerOptions
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
