using Identity.Api.Services;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Identity.Api.Middleware;

/// <summary>
/// Middleware for monitoring API Key performance metrics - Middleware giám sát hiệu suất API Key (EN)<br/>
/// Middleware giám sát hiệu suất API Key (VI)
/// </summary>
public class ApiKeyPerformanceMiddleware(RequestDelegate next, ILogger<ApiKeyPerformanceMiddleware> logger)
{

    // Performance counters using Meter
    private static readonly Meter _meter = new("TiHoMo.Identity.ApiKey", "1.0.0");
    private static readonly Counter<long> _apiKeyRequestsCounter = 
        _meter.CreateCounter<long>("api_key_requests_total", "requests", "Total number of API key requests");
    
    private static readonly Histogram<double> _apiKeyAuthDuration = 
        _meter.CreateHistogram<double>("api_key_auth_duration", "milliseconds", "API key authentication duration");

    private static readonly Counter<long> _apiKeyValidationErrorsCounter = 
        _meter.CreateCounter<long>("api_key_validation_errors_total", "errors", "Total number of API key validation errors");

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip non-API requests
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var hasApiKey = HasApiKeyInRequest(context);
        
        if (!hasApiKey)
        {
            await next(context);
            return;
        }

        try
        {
            // Start activity for API key authentication
            using var activity = TelemetryService.ActivitySource.StartActivity("api_key_authentication");
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.path", context.Request.Path);
            activity?.SetTag("client.ip", GetClientIpAddress(context));

            await next(context);

            stopwatch.Stop();

            // Record successful authentication metrics
            var authDuration = stopwatch.Elapsed.TotalMilliseconds;
            var isAuthenticated = context.Items.ContainsKey("IsApiKeyAuthenticated");
            var apiKeyId = context.Items["ApiKeyId"] as Guid?;

            _apiKeyRequestsCounter.Add(1, new KeyValuePair<string, object?>("status", "success"),
                new KeyValuePair<string, object?>("authenticated", isAuthenticated));

            _apiKeyAuthDuration.Record(authDuration, 
                new KeyValuePair<string, object?>("status", "success"));

            // Add performance headers
            context.Response.Headers.Append("X-API-Key-Auth-Time", authDuration.ToString("F2"));
            
            if (apiKeyId.HasValue)
            {
                context.Response.Headers.Append("X-API-Key-ID", apiKeyId.Value.ToString());
            }

            // Log slow requests
            if (authDuration > 1000) // More than 1 second
            {
                logger.LogWarning("Slow API key authentication detected: {Duration}ms for {Method} {Path} from {ClientIP}",
                    authDuration, context.Request.Method, context.Request.Path, GetClientIpAddress(context));
            }

            activity?.SetTag("auth.duration_ms", authDuration);
            activity?.SetTag("auth.success", isAuthenticated);
            if (apiKeyId.HasValue)
            {
                activity?.SetTag("api_key.id", apiKeyId.Value.ToString());
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record error metrics
            var errorType = ex.GetType().Name;
            _apiKeyValidationErrorsCounter.Add(1, 
                new KeyValuePair<string, object?>("error_type", errorType));

            _apiKeyRequestsCounter.Add(1, 
                new KeyValuePair<string, object?>("status", "error"));

            _apiKeyAuthDuration.Record(stopwatch.Elapsed.TotalMilliseconds, 
                new KeyValuePair<string, object?>("status", "error"));

            logger.LogError(ex, "API key authentication error after {Duration}ms: {ErrorType}",
                stopwatch.Elapsed.TotalMilliseconds, errorType);

            throw;
        }
    }

    private bool HasApiKeyInRequest(HttpContext context)
    {
        // Check X-API-Key header
        if (context.Request.Headers.ContainsKey("X-API-Key"))
            return true;

        // Check Authorization header with ApiKey scheme
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer tihomo_"))
            return true;

        // Check query parameter
        if (context.Request.Query.ContainsKey("api_key"))
            return true;

        return false;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "unknown";
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Extension methods for registering API Key performance middleware
/// </summary>
public static class ApiKeyPerformanceMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyPerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyPerformanceMiddleware>();
    }
}