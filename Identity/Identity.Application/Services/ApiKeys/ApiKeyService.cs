using System.Security.Cryptography;
using Identity.Application.Common.Interfaces;
using Identity.Contracts;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Repositories;

namespace Identity.Application.Services.ApiKeys;

/// <summary>
///     Service for managing API key operations (EN)<br />
///     Dịch vụ quản lý các thao tác khóa API (VI)
/// </summary>
/// <param name="apiKeyRepository">
///     Repository for API key data access (EN)<br />
///     Repository để truy cập dữ liệu khóa API (VI)
/// </param>
/// <param name="userRepository">
///     Repository for user data access (EN)<br />
///     Repository để truy cập dữ liệu người dùng (VI)
/// </param>
/// <param name="apiKeyHasher">
///     Service for API key hashing and verification (EN)<br />
///     Dịch vụ để băm và xác minh khóa API (VI)
/// </param>
public class ApiKeyService(
    IApiKeyRepository apiKeyRepository,
    IUserRepository userRepository,
    IApiKeyHasher apiKeyHasher) : IApiKeyService
{
    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verify user exists
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found");

        // Check user's API key limit
        var userApiKeys = await apiKeyRepository.GetByUserIdAsync(userId, cancellationToken);
        const int maxKeysPerUser = 10; // This should come from configuration
        if (userApiKeys.Count() >= maxKeysPerUser)
            throw new InvalidOperationException(
                $"User has reached the maximum limit of {maxKeysPerUser} API keys");

        // Generate new API key
        var rawApiKey = GenerateApiKey();
        var hashedKey = apiKeyHasher.HashApiKey(rawApiKey);
        var keyPrefix = rawApiKey.Substring(0, Math.Min(10, rawApiKey.Length));

        var apiKey = new ApiKey
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            KeyHash = hashedKey,
            KeyPrefix = keyPrefix,
            Scopes = request.Scopes,
            Status = ApiKeyStatus.Active,
            RateLimitPerMinute = request.RateLimitPerMinute,
            DailyUsageQuota = request.DailyUsageQuota,
            IpWhitelist = request.IpWhitelist,
            SecuritySettings = new ApiKeySecuritySettings
            {
                RequireHttps = request.SecuritySettings.RequireHttps,
                AllowCorsRequests = request.SecuritySettings.AllowCorsRequests,
                AllowedOrigins = request.SecuritySettings.AllowedOrigins,
                EnableUsageAnalytics = request.SecuritySettings.EnableUsageAnalytics,
                MaxRequestsPerSecond = request.SecuritySettings.MaxRequestsPerSecond,
                EnableIpValidation = request.SecuritySettings.EnableIpValidation,
                EnableRateLimiting = request.SecuritySettings.EnableRateLimiting
            },
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastResetDate = DateTime.UtcNow.Date
        };

        await apiKeyRepository.AddAsync(apiKey, cancellationToken);

        return new CreateApiKeyResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Description = apiKey.Description,
            ApiKey = rawApiKey, // Only shown during creation
            KeyPrefix = apiKey.KeyPrefix,
            Scopes = apiKey.Scopes,
            RateLimitPerMinute = apiKey.RateLimitPerMinute,
            DailyUsageQuota = apiKey.DailyUsageQuota,
            IpWhitelist = apiKey.IpWhitelist,
            SecuritySettings = new ApiKeySecuritySettingsDto
            {
                RequireHttps = apiKey.SecuritySettings.RequireHttps,
                AllowCorsRequests = apiKey.SecuritySettings.AllowCorsRequests,
                AllowedOrigins = apiKey.SecuritySettings.AllowedOrigins,
                EnableUsageAnalytics = apiKey.SecuritySettings.EnableUsageAnalytics,
                MaxRequestsPerSecond = apiKey.SecuritySettings.MaxRequestsPerSecond,
                EnableIpValidation = apiKey.SecuritySettings.EnableIpValidation,
                EnableRateLimiting = apiKey.SecuritySettings.EnableRateLimiting
            },
            CreatedAt = apiKey.CreatedAt ?? DateTime.UtcNow,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    public async Task<ApiKeyResponse> GetApiKeyByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null) throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        return MapToApiKeyResponse(apiKey);
    }

    public async Task<IEnumerable<ApiKeyResponse>> GetUserApiKeysAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var apiKeys = await apiKeyRepository.GetByUserIdAsync(userId, cancellationToken);
        return apiKeys.Select(MapToApiKeyResponse);
    }

    public async Task<ApiKeyResponse> UpdateApiKeyAsync(Guid apiKeyId, UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null) throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        // Update properties
        if (!string.IsNullOrEmpty(request.Name))
            apiKey.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Description))
            apiKey.Description = request.Description;

        if (request.Scopes != null)
            apiKey.Scopes = request.Scopes;

        if (request.ExpiresAt.HasValue)
            apiKey.ExpiresAt = request.ExpiresAt;

        if (request.RateLimitPerMinute.HasValue)
            apiKey.RateLimitPerMinute = request.RateLimitPerMinute.Value;

        if (request.DailyUsageQuota.HasValue)
            apiKey.DailyUsageQuota = request.DailyUsageQuota.Value;

        if (request.IpWhitelist != null)
            apiKey.IpWhitelist = request.IpWhitelist;

        if (request.SecuritySettings != null)
        {
            apiKey.SecuritySettings = new ApiKeySecuritySettings
            {
                RequireHttps = request.SecuritySettings.RequireHttps,
                AllowCorsRequests = request.SecuritySettings.AllowCorsRequests,
                AllowedOrigins = request.SecuritySettings.AllowedOrigins,
                EnableUsageAnalytics = request.SecuritySettings.EnableUsageAnalytics,
                MaxRequestsPerSecond = request.SecuritySettings.MaxRequestsPerSecond,
                EnableIpValidation = request.SecuritySettings.EnableIpValidation,
                EnableRateLimiting = request.SecuritySettings.EnableRateLimiting
            };
        }

        apiKey.UpdatedAt = DateTime.UtcNow;
        await apiKeyRepository.UpdateAsync(apiKey, cancellationToken);

        return MapToApiKeyResponse(apiKey);
    }

    public async Task RevokeApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null) throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        apiKey.Status = ApiKeyStatus.Revoked;
        apiKey.UpdatedAt = DateTime.UtcNow;
        await apiKeyRepository.UpdateAsync(apiKey, cancellationToken);
    }

    public async Task DeleteApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null) throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        await apiKeyRepository.DeleteAsync(apiKeyId, cancellationToken);
    }

    public async Task<VerifyApiKeyResponse> VerifyApiKeyAsync(string apiKey,
        CancellationToken cancellationToken = default)
    {
        var hashedKey = apiKeyHasher.HashApiKey(apiKey);
        var apiKeyEntity = await apiKeyRepository.GetActiveKeyByHashAsync(hashedKey, cancellationToken);

        if (apiKeyEntity == null)
                    return new VerifyApiKeyResponse
        {
            IsValid = false,
            UserId = null,
            Scopes = [],
            Message = "Invalid API key",
            ErrorMessage = "Invalid API key"
        };

        // Check if key is expired
        if (apiKeyEntity.ExpiresAt.HasValue && apiKeyEntity.ExpiresAt <= DateTime.UtcNow)
            return new VerifyApiKeyResponse
            {
                IsValid = false,
                UserId = null,
                Scopes = [],
                Message = "API key has expired",
                ErrorMessage = "API key has expired"
            };

        // Check if key is revoked
        if (apiKeyEntity.Status == ApiKeyStatus.Revoked)
            return new VerifyApiKeyResponse
            {
                IsValid = false,
                UserId = null,
                Scopes = [],
                Message = "API key has been revoked",
                ErrorMessage = "API key has been revoked"
            };

        // Update usage
        await apiKeyRepository.UpdateLastUsedAsync(apiKeyEntity.Id, DateTime.UtcNow, cancellationToken);
        await apiKeyRepository.IncrementUsageCountAsync(apiKeyEntity.Id, cancellationToken);

        // Verify user is still active
        var user = await userRepository.GetByIdAsync(apiKeyEntity.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            return new VerifyApiKeyResponse
            {
                IsValid = false,
                UserId = null,
                Scopes = [],
                Message = "User account is disabled or not found",
                ErrorMessage = "User account is disabled or not found"
            };

        return new VerifyApiKeyResponse
        {
            IsValid = true,
            UserId = apiKeyEntity.UserId,
            ApiKeyId = apiKeyEntity.Id,
            Scopes = apiKeyEntity.Scopes,
            Message = "API key is valid"
        };
    }

    private static string GenerateApiKey()
    {
        // Generate a secure random API key
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);

        // Convert to base64 and make it URL safe
        var base64 = Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return $"tihomo_{base64}";
    }

    private static ApiKeyResponse MapToApiKeyResponse(ApiKey apiKey)
    {
        return new ApiKeyResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Description = apiKey.Description,
            KeyPrefix = apiKey.KeyPrefix,
            Scopes = apiKey.Scopes,
            Status = apiKey.Status.ToString(),
            RateLimitPerMinute = apiKey.RateLimitPerMinute,
            DailyUsageQuota = apiKey.DailyUsageQuota,
            TodayUsageCount = apiKey.TodayUsageCount,
            UsageCount = apiKey.UsageCount,
            IpWhitelist = apiKey.IpWhitelist,
            SecuritySettings = new ApiKeySecuritySettingsDto
            {
                RequireHttps = apiKey.SecuritySettings.RequireHttps,
                AllowCorsRequests = apiKey.SecuritySettings.AllowCorsRequests,
                AllowedOrigins = apiKey.SecuritySettings.AllowedOrigins,
                EnableUsageAnalytics = apiKey.SecuritySettings.EnableUsageAnalytics,
                MaxRequestsPerSecond = apiKey.SecuritySettings.MaxRequestsPerSecond,
                EnableIpValidation = apiKey.SecuritySettings.EnableIpValidation,
                EnableRateLimiting = apiKey.SecuritySettings.EnableRateLimiting
            },
            CreatedAt = apiKey.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = apiKey.UpdatedAt ?? (apiKey.CreatedAt ?? DateTime.UtcNow),
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = apiKey.LastUsedAt,
            RevokedAt = apiKey.RevokedAt,
            IsActive = apiKey.IsActive,
            IsExpired = apiKey.IsExpired,
            IsRevoked = apiKey.IsRevoked,
            IsRateLimited = apiKey.IsRateLimited
        };
    }
}