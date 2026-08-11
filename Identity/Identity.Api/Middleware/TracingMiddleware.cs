using System.Diagnostics;

namespace Identity.Api.Middleware;

/// <summary>
/// Middleware for distributed tracing with OpenTelemetry
/// Middleware cho distributed tracing với OpenTelemetry
/// </summary>
public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<TracingMiddleware> _logger;

    public TracingMiddleware(
        RequestDelegate next,
        ActivitySource activitySource,
        ILogger<TracingMiddleware> logger)
    {
        _next = next;
        _activitySource = activitySource;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip health check endpoints from tracing
        // Bỏ qua health check endpoints khỏi tracing
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var operationName = $"{context.Request.Method} {context.Request.Path}";
        
        using var activity = _activitySource.StartActivity(operationName);
        
        if (activity != null)
        {
            // Set standard HTTP tags
            // Đặt các tags HTTP chuẩn
            activity.SetTag("http.method", context.Request.Method);
            activity.SetTag("http.url", GetFullUrl(context.Request));
            activity.SetTag("http.scheme", context.Request.Scheme);
            activity.SetTag("http.host", context.Request.Host.Value);
            activity.SetTag("http.target", context.Request.Path + context.Request.QueryString);
            activity.SetTag("http.user_agent", context.Request.Headers.UserAgent.ToString());
            
            // Set service information
            // Đặt thông tin service
            activity.SetTag("service.name", "TiHoMo.Identity");
            activity.SetTag("service.version", "1.0.0");
            
            // Extract or generate correlation ID
            // Extract hoặc generate correlation ID
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                               ?? context.Items["CorrelationId"]?.ToString()
                               ?? Guid.CreateVersion7().ToString();
            
            activity.SetTag("correlation.id", correlationId);
            
            // Set user information if available
            // Đặt thông tin user nếu có
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("sub")?.Value 
                            ?? context.User.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    activity.SetTag("user.id", userId);
                }
            }
            
            // Add custom attributes
            // Thêm attributes tùy chỉnh
            activity.SetTag("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development");
            activity.SetTag("machine.name", Environment.MachineName);
        }

        try
        {
            await _next(context);

            // Set response information
            // Đặt thông tin response
            activity?.SetTag("http.status_code", context.Response.StatusCode);
            
            // Set status based on HTTP status code
            // Đặt status dựa trên HTTP status code
            if (context.Response.StatusCode >= 400)
            {
                activity?.SetStatus(ActivityStatusCode.Error, GetStatusDescription(context.Response.StatusCode));
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
        }
        catch (Exception ex)
        {
            // Record exception in span
            // Ghi lại exception trong span
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error", true);
            activity?.SetTag("error.type", ex.GetType().FullName);
            activity?.SetTag("error.message", ex.Message);
            
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                activity?.SetTag("error.stack", ex.StackTrace);
            }

            // Record exception using OpenTelemetry
            // Ghi lại exception bằng OpenTelemetry
            activity?.AddException(ex);

            _logger.LogError(ex, "Unhandled exception occurred during request processing");
            
            throw;
        }
    }

    private static string GetFullUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
    }

    private static string GetStatusDescription(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized", 
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            408 => "Request Timeout",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => $"HTTP {statusCode}"
        };
    }
}

/// <summary>
/// Extension methods for adding tracing middleware
/// Extension methods để thêm tracing middleware
/// </summary>
public static class TracingMiddlewareExtensions
{
    public static IApplicationBuilder UseDistributedTracing(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TracingMiddleware>();
    }
}