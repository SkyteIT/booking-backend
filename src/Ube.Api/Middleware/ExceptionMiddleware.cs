using System.Net;
using System.Text.Json;
using Ube.Application.Common.Exceptions;

namespace Ube.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
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
    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Console.WriteLine($"Exception: {ex.GetType().Name} - {ex.Message}");
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
            statusCode = (int)HttpStatusCode.InternalServerError;
            message = ex.Message;
        }
        var result = JsonSerializer.Serialize(new
        {
            message
        });
        response.StatusCode = statusCode;
        return response.WriteAsync(result);

        
    }
    
}