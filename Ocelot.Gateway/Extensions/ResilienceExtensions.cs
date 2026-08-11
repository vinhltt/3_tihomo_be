using Polly;
using Polly.Extensions.Http;

namespace Ocelot.Gateway.Extensions;

/// <summary>
/// Extension methods for adding resilience policies
/// Các phương thức mở rộng để thêm chính sách resilience
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Get retry policy for HTTP requests
    /// Lấy chính sách retry cho HTTP requests
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // HttpRequestException và 5XX, 408 status codes
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    if (logger != null)
                    {
                        logger.LogWarning("Retry {RetryCount} for {OperationKey} in {Delay}ms", 
                            retryCount, context.OperationKey, timespan.TotalMilliseconds);
                    }
                });
    }

    /// <summary>
    /// Get circuit breaker policy for HTTP requests
    /// Lấy chính sách circuit breaker cho HTTP requests
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3, // Cho phép 3 lỗi trước khi ngắt mạch
                durationOfBreak: TimeSpan.FromSeconds(30), // Thời gian ngắt mạch: 30 giây
                onBreak: (exception, duration) =>
                {
                    // Log when circuit breaker opens
                },
                onReset: () =>
                {
                    // Log when circuit breaker closes
                },
                onHalfOpen: () =>
                {
                    // Log when circuit breaker is half-open
                });
    }

    /// <summary>
    /// Get timeout policy for HTTP requests
    /// Lấy chính sách timeout cho HTTP requests
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutMs = 5000)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(timeoutMs));
    }

    private static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue("logger", out var loggerObj) && loggerObj is ILogger logger)
        {
            return logger;
        }
        return null;
    }
}