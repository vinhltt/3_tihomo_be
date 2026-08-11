using System.Diagnostics;
using Identity.Api.Services;
using Serilog.Context;

namespace Identity.Api.Middleware;

/// <summary>
///     Middleware for request correlation and observability
///     Middleware cho correlation và observability của request
/// </summary>
public class ObservabilityMiddleware(
    RequestDelegate next,
    TelemetryService telemetryService,
    ILogger<ObservabilityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or extract correlation ID
        // Tạo hoặc extract correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.CreateVersion7().ToString();

        // Add correlation ID to response headers
        // Thêm correlation ID vào response headers
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        // Add correlation ID to HttpContext for logging
        // Thêm correlation ID vào HttpContext cho logging
        context.Items["CorrelationId"] = correlationId;

        // Push properties to log context for Serilog
        // Đẩy properties vào log context cho Serilog
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        using (LogContext.PushProperty("ConnectionId", context.Connection.Id))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        {
            // Start activity for distributed tracing
            // Bắt đầu activity cho distributed tracing
            using var activity = TelemetryService.ActivitySource.StartActivity("http_request");
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.url", context.Request.Path);
            activity?.SetTag("correlation_id", correlationId);

            // Track active requests
            // Theo dõi active requests
            telemetryService.IncrementActiveRequests();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Continue to next middleware
                // Tiếp tục với middleware tiếp theo
                await next(context);

                // Record successful request
                // Ghi lại request thành công
                activity?.SetTag("http.status_code", context.Response.StatusCode);
                activity?.SetStatus(
                    context.Response.StatusCode >= 400
                        ? ActivityStatusCode.Error
                        : ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                // Record failed request
                // Ghi lại request thất bại
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error.type", ex.GetType().Name);
                activity?.SetTag("error.message", ex.Message);

                logger.LogError(ex,
                    "Request failed for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                throw;
            }
            finally
            {
                // Always decrement active requests
                // Luôn giảm active requests
                telemetryService.DecrementActiveRequests();

                stopwatch.Stop();

                // Log request completion with timing
                // Log hoàn thành request với timing
                logger.LogInformation(
                    "Request {Method} {Path} completed in {Duration}ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
        }
    }
}