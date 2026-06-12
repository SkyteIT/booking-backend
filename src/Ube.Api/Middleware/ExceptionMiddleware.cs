using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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
        var response = context.Response;
        response.ContentType = "application/json";

        int statusCode;
        string message;

        if (ex is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            message = appEx.Message;
        }
        else
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
            statusCode = (int)HttpStatusCode.InternalServerError;
            message = "An unexpected error occurred. Please try again later.";
        }

        var result = JsonSerializer.Serialize(new { message });
        response.StatusCode = statusCode;
        return response.WriteAsync(result);
    }
}