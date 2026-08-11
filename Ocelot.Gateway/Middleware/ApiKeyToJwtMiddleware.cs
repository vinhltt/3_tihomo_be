using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ocelot.Gateway.Configuration;
using Ocelot.Gateway.Services;
using System.Diagnostics;

namespace Ocelot.Gateway.Middleware;

/// <summary>
/// Simple middleware to convert API Key to JWT and forward to Ocelot
/// Middleware đơn giản để chuyển đổi API Key thành JWT và forward đến Ocelot
/// </summary>
public class ApiKeyToJwtMiddleware(RequestDelegate next, ILogger<ApiKeyToJwtMiddleware> logger, IHttpClientFactory httpClientFactory, IOptions<ApiKeyToJwtSettings> settings, AuthMetricsService metrics)
{
    private readonly ApiKeyToJwtSettings _settings = settings.Value;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        logger.LogInformation("ApiKeyToJwtMiddleware: ENTRY - Processing request {Path}", path ?? "NULL");
        
        // Check if path should be skipped
        if (string.IsNullOrEmpty(path) || _settings.SkippedPaths.Any(skip => path.StartsWith(skip)))
        {
            logger.LogInformation("ApiKeyToJwtMiddleware: Skipping request {Path}", path ?? "NULL");
            await next(context);
            return;
        }
        
        // Check if path should be processed
        if (!_settings.ProcessedPaths.Any(processed => path.StartsWith(processed)))
        {
            logger.LogInformation("ApiKeyToJwtMiddleware: Path {Path} not in processed paths", path ?? "NULL");
            await next(context);
            return;
        }

        logger.LogInformation("ApiKeyToJwtMiddleware: Processing API request {Path}", path);

        // Check for API Key
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        var hasApiKey = !string.IsNullOrEmpty(apiKey);
        
        // Record auth request metrics
        metrics.RecordAuthenticationRequest(path ?? "", hasApiKey);
        
        logger.LogInformation("ApiKeyToJwtMiddleware: API Key found = {HasApiKey}", hasApiKey);
        
        if (hasApiKey)
        {
            logger.LogInformation("ApiKeyToJwtMiddleware: Found API key, exchanging for JWT");
            
            var stopwatch = Stopwatch.StartNew();
            var jwtToken = await ExchangeApiKeyForJwt(apiKey!);
            stopwatch.Stop();
            
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            
            if (!string.IsNullOrEmpty(jwtToken))
            {
                logger.LogInformation("ApiKeyToJwtMiddleware: Successfully exchanged API key for JWT");
                metrics.RecordApiKeyExchangeSuccess(durationSeconds);
                
                // Remove API Key and add JWT Bearer token
                context.Request.Headers.Remove("X-API-Key");
                context.Request.Headers.Authorization = $"Bearer {jwtToken}";
                
                logger.LogInformation("ApiKeyToJwtMiddleware: Added JWT Bearer token to request - Auth header = {AuthHeader}", 
                    context.Request.Headers.Authorization.FirstOrDefault());
            }
            else
            {
                logger.LogWarning("ApiKeyToJwtMiddleware: API key validation failed");
                metrics.RecordApiKeyExchangeFailure(durationSeconds, "invalid_key");
                
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var errorResponse = JsonSerializer.Serialize(new 
                {
                    error = "Authentication failed",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path.Value
                });
                await context.Response.WriteAsync(errorResponse);
                return;
            }
        }
        else
        {
            logger.LogInformation("ApiKeyToJwtMiddleware: No API key found, continuing without authentication");
        }

        logger.LogInformation("ApiKeyToJwtMiddleware: Calling next middleware");
        // Continue to next middleware (Ocelot will handle routing)
        await next(context);
    }

    private async Task<string?> ExchangeApiKeyForJwt(string apiKey)
    {
        metrics.RecordApiKeyExchangeAttempt();
        
        try
        {
            var identityClient = httpClientFactory.CreateClient("IdentityService");
            
            // Identity API expects [FromBody] string apiKey
            var requestJson = JsonSerializer.Serialize(apiKey);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            
            var response = await identityClient.PostAsync("/api/v1/api-keys/exchange", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var exchangeResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                if (exchangeResponse.TryGetProperty("accessToken", out var tokenElement))
                {
                    var token = tokenElement.GetString();
                    logger.LogInformation("ApiKeyToJwtMiddleware: Successfully exchanged API key for JWT token");
                    return token;
                }
                else
                {
                    logger.LogWarning("ApiKeyToJwtMiddleware: Invalid response format - missing accessToken property");
                }
            }
            else
            {
                logger.LogWarning("ApiKeyToJwtMiddleware: API key validation failed - Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ApiKeyToJwtMiddleware: Error exchanging API key for JWT");
        }
        
        return null;
    }
}