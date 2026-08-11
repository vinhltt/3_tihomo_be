using System.Net;
using System.Text.Json;

namespace Ocelot.Gateway.Middleware;

/// <summary>
///     Global exception handling middleware
/// </summary>
public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.Items["RequestId"]?.ToString() ?? Guid.CreateVersion7().ToString();

        logger.LogError(exception,
            "Unhandled exception occurred for request {RequestId}: {ExceptionMessage}",
            requestId,
            exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new
        {
            RequestId = requestId,
            Message = "An error occurred while processing your request.",
            Details = environment.IsDevelopment() ? exception.Message : null,
            StackTrace = environment.IsDevelopment() ? exception.StackTrace : null,
            Timestamp = DateTime.UtcNow
        };

        response.StatusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}

/// <summary>
///     Extension methods for registering global exception handling middleware
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}