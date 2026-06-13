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
            // FIX: InvalidOperationException is our deliberate duplicate-code guard
            // in PromotionService — must return 409 so the frontend shows the
            // "already exists" error message instead of a generic crash.
            InvalidOperationException
                => (HttpStatusCode.Conflict, exception.Message),

            // Unique constraint / DB conflict — fallback in case a duplicate
            // somehow slips past our pre-check (e.g. race condition)
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx)
                => (HttpStatusCode.Conflict, ExtractDuplicateMessage(dbEx)),

            // Other DB errors
            DbUpdateException
                => (HttpStatusCode.BadRequest, "Database update failed. Check your input values."),

            // Not found
            KeyNotFoundException
                => (HttpStatusCode.NotFound, exception.Message),

            // Validation / bad arguments
            ArgumentException
                => (HttpStatusCode.BadRequest, exception.Message),

            // Default: don't leak internals
            _
                => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            error = message,
            // Only expose raw detail for non-500 errors (500 hides stack trace)
            detail = statusCode == HttpStatusCode.InternalServerError ? null : exception.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? "";
        return inner.Contains("UNIQUE") ||
               inner.Contains("duplicate key") ||
               inner.Contains("Violation of UNIQUE KEY constraint") ||
               inner.Contains("Cannot insert duplicate key");
    }

    // FIX: extract a user-friendly message from the SQL error instead of
    // returning the generic "A record with this name already exists."
    // The PromotionService pre-check will normally handle this, but if a
    // race condition hits the DB, this gives a cleaner message.
    private static string ExtractDuplicateMessage(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? "";

        // SQL Server puts the duplicate value in parentheses: "The duplicate key value is (PROMO123)"
        var start = inner.LastIndexOf('(');
        var end = inner.LastIndexOf(')');

        if (start >= 0 && end > start)
        {
            var value = inner.Substring(start + 1, end - start - 1).Trim();
            if (!string.IsNullOrEmpty(value))
                return $"Promo code \"{value}\" already exists. Please use a different code.";
        }

        return "A record with this value already exists.";
    }
}