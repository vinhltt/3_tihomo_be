using System.Diagnostics;
using Serilog.Context;

namespace CoreFinance.Api.Middleware;

/// <summary>
///     Middleware to handle correlation ID tracking and structured logging
///     Middleware để xử lý correlation ID tracking và structured logging
/// </summary>
public class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Use TraceId as primary correlation identifier - more standards-compliant
        // Sử dụng TraceId làm identifier chính - tuân thủ chuẩn W3C
        var traceId = GetOrCreateTraceId(context);
        
        // Add TraceId to Serilog context for all logs in this request
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        {
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Request started: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            try
            {
                await next(context);

                stopwatch.Stop();
                logger.LogInformation(
                    "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Request failed: {Method} {Path} - Duration: {ElapsedMs}ms - Error: {ErrorMessage}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }

    private string GetOrCreateTraceId(HttpContext context)
    {
        // First try to get TraceId from current OpenTelemetry Activity
        var currentTraceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrEmpty(currentTraceId) && currentTraceId != "00000000000000000000000000000000")
        {
            // Add to response header for client reference
            context.Response.Headers["X-Trace-ID"] = currentTraceId;
            return currentTraceId;
        }

        // Try to get from incoming header (for external clients)
        if (context.Request.Headers.TryGetValue("X-Trace-ID", out var headerValue))
        {
            var traceId = headerValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(traceId))
            {
                // Add to response header
                context.Response.Headers["X-Trace-ID"] = traceId;
                return traceId;
            }
        }

        // Generate new TraceId as fallback
        var newTraceId = Guid.CreateVersion7().ToString("N"); // 32 chars hex, similar to OpenTelemetry format
        context.Response.Headers["X-Trace-ID"] = newTraceId;
        context.Items["TraceId"] = newTraceId;

        return newTraceId;
    }
}