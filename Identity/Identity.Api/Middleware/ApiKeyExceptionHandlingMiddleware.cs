using Identity.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Identity.Api.Middleware;

/// <summary>
/// Middleware for handling API Key specific exceptions - Middleware xử lý các ngoại lệ cụ thể của API Key (EN)<br/>
/// Middleware xử lý các ngoại lệ cụ thể của API Key (VI)
/// </summary>
public class ApiKeyExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiKeyExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, GetOptions());
        }
    }

    private JsonSerializerOptions GetOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, JsonSerializerOptions options)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = exception switch
        {
            ApiKeyLimitExceededException ex => new ProblemDetails
            {
                Title = "API Key Limit Exceeded",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.Conflict,
                Extensions = new Dictionary<string, object?>
                {
                    ["currentCount"] = ex.CurrentCount,
                    ["maxAllowed"] = ex.MaxAllowed,
                    ["traceId"] = context.TraceIdentifier
                }
            },

            InvalidApiKeyException ex => new ProblemDetails
            {
                Title = "Invalid API Key",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.Unauthorized,
                Extensions = new Dictionary<string, object?>
                {
                    ["reason"] = ex.Reason,
                    ["keyPrefix"] = ex.ApiKeyPrefix,
                    ["traceId"] = context.TraceIdentifier
                }
            },

            ApiKeyValidationException ex => new ProblemDetails
            {
                Title = "API Key Validation Failed",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.Forbidden,
                Extensions = new Dictionary<string, object?>
                {
                    ["validationStep"] = ex.ValidationStep,
                    ["context"] = ex.ValidationContext,
                    ["traceId"] = context.TraceIdentifier
                }
            },

            RateLimitExceededException ex => new ProblemDetails
            {
                Title = "Rate Limit Exceeded",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.TooManyRequests,
                Extensions = new Dictionary<string, object?>
                {
                    ["apiKeyId"] = ex.ApiKeyId,
                    ["currentUsage"] = ex.CurrentUsage,
                    ["limit"] = ex.Limit,
                    ["retryAfter"] = ex.RetryAfter.TotalSeconds,
                    ["traceId"] = context.TraceIdentifier
                }
            },

            ApiKeyException ex => new ProblemDetails
            {
                Title = "API Key Error",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Extensions = new Dictionary<string, object?>
                {
                    ["traceId"] = context.TraceIdentifier
                }
            },

            KeyNotFoundException ex => new ProblemDetails
            {
                Title = "Resource Not Found",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.NotFound,
                Extensions = new Dictionary<string, object?>
                {
                    ["traceId"] = context.TraceIdentifier
                }
            },

            ArgumentException ex => new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Extensions = new Dictionary<string, object?>
                {
                    ["traceId"] = context.TraceIdentifier
                }
            },

            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = (int)HttpStatusCode.InternalServerError,
                Extensions = new Dictionary<string, object?>
                {
                    ["traceId"] = context.TraceIdentifier
                }
            }
        };

        response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        // Add rate limiting headers for RateLimitExceededException
        if (exception is RateLimitExceededException rateLimitEx)
        {
            response.Headers.Append("Retry-After", rateLimitEx.RetryAfter.TotalSeconds.ToString());
            response.Headers.Append("X-RateLimit-Limit", rateLimitEx.Limit.ToString());
            response.Headers.Append("X-RateLimit-Remaining", Math.Max(0, rateLimitEx.Limit - rateLimitEx.CurrentUsage).ToString());
            response.Headers.Append("X-RateLimit-Reset", DateTimeOffset.UtcNow.Add(rateLimitEx.RetryAfter).ToUnixTimeSeconds().ToString());
        }

        // Log the exception with appropriate level
        var logLevel = exception switch
        {
            ApiKeyLimitExceededException => LogLevel.Warning,
            InvalidApiKeyException => LogLevel.Warning,
            ApiKeyValidationException => LogLevel.Information,
            RateLimitExceededException => LogLevel.Information,
            ApiKeyException => LogLevel.Warning,
            KeyNotFoundException => LogLevel.Information,
            ArgumentException => LogLevel.Information,
            _ => LogLevel.Error
        };

        logger.Log(logLevel, exception, "API Key exception handled: {ExceptionType} - {Message}", 
            exception.GetType().Name, exception.Message);

        var jsonResponse = JsonSerializer.Serialize(problemDetails, options);

        await response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension methods for registering API Key exception handling middleware
/// </summary>
public static class ApiKeyExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyExceptionHandlingMiddleware>();
    }
}