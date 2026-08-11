using System.Diagnostics;

namespace Ocelot.Gateway.Middleware;

/// <summary>
///     Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.CreateVersion7().ToString();

        // Add request ID to context for correlation
        context.Items["RequestId"] = requestId;

        // Log incoming request
        logger.LogInformation(
            "Request {RequestId} started: {Method} {Path} from {RemoteIpAddress}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Request {RequestId} failed: {Method} {Path} - {ErrorMessage}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Log completed request
            logger.LogInformation(
                "Request {RequestId} completed: {Method} {Path} {StatusCode} in {ElapsedMilliseconds}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

/// <summary>
///     Extension methods for registering request logging middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}