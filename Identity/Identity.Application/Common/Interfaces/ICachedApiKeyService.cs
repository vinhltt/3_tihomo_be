using Identity.Contracts;

namespace Identity.Application.Common.Interfaces;

/// <summary>
/// Cached API Key Service Interface for improved performance - Interface dịch vụ API Key có cache để cải thiện hiệu suất (EN)<br/>
/// Interface dịch vụ API Key có cache để cải thiện hiệu suất (VI)
/// </summary>
public interface ICachedApiKeyService
{
    /// <summary>
    /// Quickly verify API key using cache - Xác thực nhanh API key bằng cache (EN)<br/>
    /// Xác thực nhanh API key bằng cache (VI)
    /// </summary>
    Task<VerifyApiKeyResponse> VerifyApiKeyCachedAsync(string rawApiKey, string clientIpAddress, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate API key cache - Vô hiệu hóa cache API key (EN)<br/>
    /// Vô hiệu hóa cache API key (VI)
    /// </summary>
    Task InvalidateApiKeyCacheAsync(Guid apiKeyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Warm up cache with frequently used API keys - Làm nóng cache với các API key thường dùng (EN)<br/>
    /// Làm nóng cache với các API key thường dùng (VI)
    /// </summary>
    Task WarmupCacheAsync(CancellationToken cancellationToken = default);
}