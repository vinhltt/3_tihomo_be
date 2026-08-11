using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Identity.Application.Services.Security;

/// <summary>
/// Rate Limiting Service - Dịch vụ giới hạn tốc độ cho API keys (EN)<br/>
/// Dịch vụ giới hạn tốc độ cho khóa API (VI)
/// </summary>
public class RateLimitingService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitingService> _logger;
    
    /// <summary>
    /// Constructor for RateLimitingService (EN)<br/>
    /// Constructor cho RateLimitingService (VI)
    /// </summary>
    /// <param name="cache">Distributed cache service (EN)<br/>Dịch vụ cache phân tán (VI)</param>
    /// <param name="logger">Logger service (EN)<br/>Dịch vụ logger (VI)</param>
    public RateLimitingService(
        IDistributedCache cache,
        ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Check if API key has exceeded rate limit (EN)<br/>
    /// Kiểm tra API key có vượt giới hạn tốc độ không (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="limitPerMinute">Limit per minute (EN)<br/>Giới hạn mỗi phút (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>True if rate limit exceeded (EN)<br/>True nếu vượt giới hạn tốc độ (VI)</returns>
    public async Task<bool> IsRateLimitExceededAsync(
        Guid apiKeyId, 
        int limitPerMinute, 
        CancellationToken cancellationToken = default)
    {
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var cacheKey = $"rate_limit:{apiKeyId}:{currentMinute}";
        
        try
        {
            var currentCountJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            var currentCount = 0;
            
            if (!string.IsNullOrEmpty(currentCountJson))
            {
                currentCount = JsonSerializer.Deserialize<int>(currentCountJson);
            }
            
            if (currentCount >= limitPerMinute)
            {
                _logger.LogWarning("API Key {ApiKeyId} exceeded rate limit. Current: {CurrentCount}, Limit: {Limit}",
                    apiKeyId, currentCount, limitPerMinute);
                return true;
            }
            
            // Increment counter
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(currentCount + 1),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                },
                cancellationToken);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for API key {ApiKeyId}", apiKeyId);
            // Fail open - allow request if cache is unavailable
            return false;
        }
    }
    
    /// <summary>
    /// Check daily usage quota (EN)<br/>
    /// Kiểm tra hạn ngạch sử dụng hàng ngày (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="dailyQuota">Daily quota limit (EN)<br/>Giới hạn hạn ngạch hàng ngày (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>True if daily quota exceeded (EN)<br/>True nếu vượt hạn ngạch hàng ngày (VI)</returns>
    public async Task<bool> IsDailyQuotaExceededAsync(
        Guid apiKeyId, 
        int dailyQuota, 
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var cacheKey = $"daily_quota:{apiKeyId}:{today}";
        
        try
        {
            var currentCountJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            var currentCount = 0;
            
            if (!string.IsNullOrEmpty(currentCountJson))
            {
                currentCount = JsonSerializer.Deserialize<int>(currentCountJson);
            }
            
            if (currentCount >= dailyQuota)
            {
                _logger.LogWarning("API Key {ApiKeyId} exceeded daily quota. Current: {CurrentCount}, Quota: {Quota}",
                    apiKeyId, currentCount, dailyQuota);
                return true;
            }
            
            // Increment counter
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(currentCount + 1),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                },
                cancellationToken);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking daily quota for API key {ApiKeyId}", apiKeyId);
            // Fail open - allow request if cache is unavailable
            return false;
        }
    }
    
    /// <summary>
    /// Get current rate limit usage (EN)<br/>
    /// Lấy mức sử dụng giới hạn tốc độ hiện tại (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>Current minute usage count (EN)<br/>Số lần sử dụng trong phút hiện tại (VI)</returns>
    public async Task<int> GetCurrentRateLimitUsageAsync(
        Guid apiKeyId, 
        CancellationToken cancellationToken = default)
    {
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var cacheKey = $"rate_limit:{apiKeyId}:{currentMinute}";
        
        try
        {
            var currentCountJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            return string.IsNullOrEmpty(currentCountJson) ? 0 : JsonSerializer.Deserialize<int>(currentCountJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rate limit usage for API key {ApiKeyId}", apiKeyId);
            return 0;
        }
    }
    
    /// <summary>
    /// Get current daily quota usage (EN)<br/>
    /// Lấy mức sử dụng hạn ngạch hàng ngày hiện tại (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>Current daily usage count (EN)<br/>Số lần sử dụng hôm nay (VI)</returns>
    public async Task<int> GetCurrentDailyQuotaUsageAsync(
        Guid apiKeyId, 
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var cacheKey = $"daily_quota:{apiKeyId}:{today}";
        
        try
        {
            var currentCountJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            return string.IsNullOrEmpty(currentCountJson) ? 0 : JsonSerializer.Deserialize<int>(currentCountJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily quota usage for API key {ApiKeyId}", apiKeyId);
            return 0;
        }
    }
    
    /// <summary>
    /// Reset rate limit counter (EN)<br/>
    /// Reset bộ đếm giới hạn tốc độ (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    public async Task ResetRateLimitAsync(
        Guid apiKeyId, 
        CancellationToken cancellationToken = default)
    {
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var cacheKey = $"rate_limit:{apiKeyId}:{currentMinute}";
        
        try
        {
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogInformation("Reset rate limit for API key {ApiKeyId}", apiKeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for API key {ApiKeyId}", apiKeyId);
        }
    }
    
    /// <summary>
    /// Reset daily quota counter (EN)<br/>
    /// Reset bộ đếm hạn ngạch hàng ngày (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    public async Task ResetDailyQuotaAsync(
        Guid apiKeyId, 
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var cacheKey = $"daily_quota:{apiKeyId}:{today}";
        
        try
        {
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogInformation("Reset daily quota for API key {ApiKeyId}", apiKeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting daily quota for API key {ApiKeyId}", apiKeyId);
        }
    }
} 