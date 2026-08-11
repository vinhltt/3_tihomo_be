using Identity.Contracts;

namespace Identity.Application.Common.Interfaces;

/// <summary>
/// Interface for Enhanced API Key Service (EN)<br/>
/// Interface cho dịch vụ API Key nâng cao (VI)
/// </summary>
public interface IEnhancedApiKeyService
{
    #region Core CRUD Operations

    /// <summary>
    /// Create simple API key (end users) (EN)<br/>
    /// Tạo API key đơn giản cho end users (VI)
    /// </summary>
    Task<CreateApiKeyResponse> CreateSimpleApiKeyAsync(Guid userId, CreateSimpleApiKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new API key with enhanced security features (EN)<br/>
    /// Tạo API key mới với tính năng bảo mật nâng cao (VI)
    /// </summary>
    Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get API key by ID with enhanced details (EN)<br/>
    /// Lấy API key theo ID với thông tin chi tiết nâng cao (VI)
    /// </summary>
    Task<ApiKeyResponse> GetApiKeyByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's API keys with filtering and pagination (EN)<br/>
    /// Lấy danh sách API key của user với lọc và phân trang (VI)
    /// </summary>
    Task<ListApiKeysResponse> GetUserApiKeysAsync(Guid userId, ListApiKeysQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update API key with enhanced properties (EN)<br/>
    /// Cập nhật API key với thuộc tính nâng cao (VI)
    /// </summary>
    Task<ApiKeyResponse> UpdateApiKeyAsync(Guid apiKeyId, UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke API key (EN)<br/>
    /// Thu hồi API key (VI)
    /// </summary>
    Task RevokeApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete API key permanently (EN)<br/>
    /// Xóa API key vĩnh viễn (VI)
    /// </summary>
    Task DeleteApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default);

    #endregion

    #region Security & Validation

    /// <summary>
    /// Verify API key with enhanced security checks (EN)<br/>
    /// Xác thực API key với kiểm tra bảo mật nâng cao (VI)
    /// </summary>
    Task<VerifyApiKeyResponse> VerifyApiKeyAsync(string rawApiKey, string clientIpAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotate API key (generate new key, keep same settings) (EN)<br/>
    /// Xoay API key (tạo key mới, giữ nguyên cài đặt) (VI)
    /// </summary>
    Task<RotateApiKeyResponse> RotateApiKeyAsync(Guid apiKeyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchange API key for JWT token (EN)<br/>
    /// Trao đổi API key lấy JWT token (VI)
    /// </summary>
    Task<ApiKeyExchangeResponse> ExchangeApiKeyForJwtAsync(string rawApiKey, string clientIpAddress,
        CancellationToken cancellationToken = default);

    #endregion

    #region Usage Analytics

    /// <summary>
    /// Get usage analytics for API key (EN)<br/>
    /// Lấy phân tích sử dụng cho API key (VI)
    /// </summary>
    Task<ApiKeyUsageResponse> GetUsageAnalyticsAsync(Guid apiKeyId, UsageQueryRequest request,
        CancellationToken cancellationToken = default);

    #endregion
} 