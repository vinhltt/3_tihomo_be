namespace Identity.Application.Services.Security;

/// <summary>
/// Interface for Rate Limiting Service - Interface cho dịch vụ giới hạn tốc độ (EN)<br/>
/// Interface cho dịch vụ giới hạn tốc độ (VI)
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Check if API key has exceeded rate limit (EN)<br/>
    /// Kiểm tra API key có vượt giới hạn tốc độ không (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="limitPerMinute">Limit per minute (EN)<br/>Giới hạn mỗi phút (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>True if rate limit exceeded (EN)<br/>True nếu vượt giới hạn tốc độ (VI)</returns>
    Task<bool> IsRateLimitExceededAsync(Guid apiKeyId, int limitPerMinute, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check daily usage quota (EN)<br/>
    /// Kiểm tra hạn ngạch sử dụng hàng ngày (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="dailyQuota">Daily quota limit (EN)<br/>Giới hạn hạn ngạch hàng ngày (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>True if daily quota exceeded (EN)<br/>True nếu vượt hạn ngạch hàng ngày (VI)</returns>
    Task<bool> IsDailyQuotaExceededAsync(Guid apiKeyId, int dailyQuota, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current rate limit usage (EN)<br/>
    /// Lấy mức sử dụng giới hạn tốc độ hiện tại (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>Current minute usage count (EN)<br/>Số lần sử dụng trong phút hiện tại (VI)</returns>
    Task<int> GetCurrentRateLimitUsageAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current daily quota usage (EN)<br/>
    /// Lấy mức sử dụng hạn ngạch hàng ngày hiện tại (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    /// <returns>Current daily usage count (EN)<br/>Số lần sử dụng hôm nay (VI)</returns>
    Task<int> GetCurrentDailyQuotaUsageAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset rate limit counter (EN)<br/>
    /// Reset bộ đếm giới hạn tốc độ (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    Task ResetRateLimitAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset daily quota counter (EN)<br/>
    /// Reset bộ đếm hạn ngạch hàng ngày (VI)
    /// </summary>
    /// <param name="apiKeyId">API key ID (EN)<br/>ID khóa API (VI)</param>
    /// <param name="cancellationToken">Cancellation token (EN)<br/>Token hủy bỏ (VI)</param>
    Task ResetDailyQuotaAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
} 