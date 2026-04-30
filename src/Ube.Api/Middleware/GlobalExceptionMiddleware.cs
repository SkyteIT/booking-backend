using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Ube.Api.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns a structured JSON error response.
/// Prevents raw 500 stack-traces from leaking to the client.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            // Duplicate name (our own guard)
            InvalidOperationException
                => (HttpStatusCode.Conflict, exception.Message),

            // Unique constraint / DB conflict
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx)
                => (HttpStatusCode.Conflict, "A record with this name already exists."),

            // Other DB errors
            DbUpdateException
                => (HttpStatusCode.BadRequest, "Database update failed. Check your input values."),

            // Not found
            KeyNotFoundException
                => (HttpStatusCode.NotFound, exception.Message),

            // Validation
            ArgumentException
                => (HttpStatusCode.BadRequest, exception.Message),

            // Default
            _
                => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            error = message,
            // Only expose detail in development
            detail = (statusCode == HttpStatusCode.InternalServerError)
                ? null
                : exception.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? "";
        // SQL Server unique constraint messages
        return inner.Contains("UNIQUE") || 
               inner.Contains("duplicate key") || 
               inner.Contains("Violation of UNIQUE KEY constraint") ||
               inner.Contains("Cannot insert duplicate key");
    }
}
