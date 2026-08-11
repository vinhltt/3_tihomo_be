using System.Security.Cryptography;
using Identity.Application.Common.Interfaces;
using Identity.Application.Services.Security;
using Identity.Contracts;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Services.ApiKeys;

/// <summary>
/// Enhanced API Key Service with security features and analytics (EN)<br/>
/// Dịch vụ API Key nâng cao với tính năng bảo mật và phân tích (VI)
/// </summary>
public class EnhancedApiKeyService(
    IApiKeyRepository apiKeyRepository,
    IUserRepository userRepository,
    IApiKeyHasher apiKeyHasher,
    IRateLimitingService rateLimitingService,
    IIpValidationService ipValidationService,
    IJwtService jwtService,
    ILogger<EnhancedApiKeyService> logger) : IEnhancedApiKeyService
{
    private const string KeyPrefix = "tihomo_";
    private const int MaxKeysPerUser = 10;
    private const int KeyLength = 43; // Base64 of 32 bytes = 44 chars, minus padding = 43 chars max

    #region Core CRUD Operations

    /// <summary>
    /// Create simple API key with default settings for end users (EN)<br/>
    /// Tạo API key đơn giản với cài đặt mặc định cho end users (VI)
    /// </summary>
    public async Task<CreateApiKeyResponse> CreateSimpleApiKeyAsync(Guid userId, CreateSimpleApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user exists and is active
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException("User not found or inactive");

            // Check user's API key limit
            var userApiKeys = await apiKeyRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userApiKeys.Count() >= MaxKeysPerUser)
                throw new InvalidOperationException($"Maximum limit of {MaxKeysPerUser} API keys exceeded");

            // Generate secure API key
            var (rawApiKey, keyHash, keyPrefix) = GenerateSecureApiKey();

            // Create entity with DEFAULT settings for end users
            var apiKey = new ApiKey
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = request.Name,
                Description = $"API Key created for {request.Name}", // Auto-generated description
                KeyHash = keyHash,
                KeyPrefix = keyPrefix,
                Scopes = ["read", "write"], // Full access default scopes
                Status = ApiKeyStatus.Active,
                RateLimitPerMinute = 50, // Default rate limit
                DailyUsageQuota = 500, // Default daily quota
                IpWhitelist = [], // No IP restrictions by default
                SecuritySettings = new ApiKeySecuritySettings // Default security settings
                {
                    RequireHttps = true, // Always require HTTPS
                    AllowCorsRequests = false, // No CORS by default
                    AllowedOrigins = [],
                    EnableUsageAnalytics = true, // Enable analytics
                    MaxRequestsPerSecond = 5, // Conservative default
                    EnableIpValidation = false, // No IP validation by default
                    EnableRateLimiting = true // Always enable rate limiting
                },
                ExpiresAt = null, // No expiration by default
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastResetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)
            };

            await apiKeyRepository.AddAsync(apiKey, cancellationToken);

            logger.LogInformation("Simple API key {KeyId} created for user {UserId}", apiKey.Id, userId);

            return MapToCreateApiKeyResponse(apiKey, rawApiKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating simple API key for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Create new API key with enhanced security features (EN)<br/>
    /// Tạo API key mới với tính năng bảo mật nâng cao (VI)
    /// </summary>
    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user exists and is active
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException("User not found or inactive");

            // Check user's API key limit
            var userApiKeys = await apiKeyRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userApiKeys.Count() >= MaxKeysPerUser)
                throw new InvalidOperationException($"Maximum limit of {MaxKeysPerUser} API keys exceeded");

            // Validate security settings
            ValidateSecuritySettings(request.SecuritySettings);

            // Validate IP whitelist
            ValidateIpWhitelist(request.IpWhitelist);

            // Generate secure API key
            var (rawApiKey, keyHash, keyPrefix) = GenerateSecureApiKey();

            // Create entity with enhanced properties
            var apiKey = new ApiKey
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                KeyHash = keyHash,
                KeyPrefix = keyPrefix,
                Scopes = request.Scopes,
                Status = ApiKeyStatus.Active,
                RateLimitPerMinute = request.RateLimitPerMinute,
                DailyUsageQuota = request.DailyUsageQuota,
                IpWhitelist = request.IpWhitelist,
                SecuritySettings = MapToSecuritySettings(request.SecuritySettings),
                ExpiresAt = request.ExpiresAt.HasValue ? DateTime.SpecifyKind(request.ExpiresAt.Value, DateTimeKind.Utc) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastResetDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)
            };

            await apiKeyRepository.AddAsync(apiKey, cancellationToken);

            logger.LogInformation("API key {KeyId} created for user {UserId}", apiKey.Id, userId);

            return MapToCreateApiKeyResponse(apiKey, rawApiKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating API key for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get API key by ID with enhanced details (EN)<br/>
    /// Lấy API key theo ID với thông tin chi tiết nâng cao (VI)
    /// </summary>
    public async Task<ApiKeyResponse> GetApiKeyByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null)
            throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        return MapToApiKeyResponse(apiKey);
    }

    /// <summary>
    /// Get user's API keys with filtering and pagination (EN)<br/>
    /// Lấy danh sách API key của user với lọc và phân trang (VI)
    /// </summary>
    public async Task<ListApiKeysResponse> GetUserApiKeysAsync(Guid userId, ListApiKeysQuery query,
        CancellationToken cancellationToken = default)
    {
        var apiKeys = await apiKeyRepository.GetByUserIdAsync(userId, cancellationToken);

        // Apply filters
        var filteredKeys = ApplyFilters(apiKeys, query);

        // Apply pagination
        var paginatedKeys = ApplyPagination(filteredKeys, query);

        var response = new ListApiKeysResponse
        {
            Data = paginatedKeys.Select(MapToApiKeyResponse).ToList(),
            Pagination = new PaginationInfo
            {
                Limit = query.Limit,
                HasMore = filteredKeys.Count() > query.Limit,
                TotalCount = filteredKeys.Count()
            },
            Meta = new ResponseMeta
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.CreateVersion7().ToString()
            }
        };

        return response;
    }

    /// <summary>
    /// Update API key with enhanced properties (EN)<br/>
    /// Cập nhật API key với thuộc tính nâng cao (VI)
    /// </summary>
    public async Task<ApiKeyResponse> UpdateApiKeyAsync(Guid apiKeyId, UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null)
            throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        // Update properties if provided
        if (!string.IsNullOrEmpty(request.Name))
            apiKey.Name = request.Name;

        if (request.Description != null)
            apiKey.Description = request.Description;

        if (request.Scopes != null)
            apiKey.Scopes = request.Scopes;

        if (request.ExpiresAt.HasValue)
            apiKey.ExpiresAt = DateTime.SpecifyKind(request.ExpiresAt.Value, DateTimeKind.Utc);

        if (request.RateLimitPerMinute.HasValue)
            apiKey.RateLimitPerMinute = request.RateLimitPerMinute.Value;

        if (request.DailyUsageQuota.HasValue)
            apiKey.DailyUsageQuota = request.DailyUsageQuota.Value;

        if (request.IpWhitelist != null)
        {
            ValidateIpWhitelist(request.IpWhitelist);
            apiKey.IpWhitelist = request.IpWhitelist;
        }

        if (request.SecuritySettings != null)
        {
            ValidateSecuritySettings(request.SecuritySettings);
            apiKey.SecuritySettings = MapToSecuritySettings(request.SecuritySettings);
        }

        apiKey.UpdatedAt = DateTime.UtcNow;
        await apiKeyRepository.UpdateAsync(apiKey, cancellationToken);

        logger.LogInformation("API key {KeyId} updated", apiKeyId);

        return MapToApiKeyResponse(apiKey);
    }

    /// <summary>
    /// Revoke API key (EN)<br/>
    /// Thu hồi API key (VI)
    /// </summary>
    public async Task RevokeApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null)
            throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        apiKey.Status = ApiKeyStatus.Revoked;
        apiKey.RevokedAt = DateTime.UtcNow;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await apiKeyRepository.UpdateAsync(apiKey, cancellationToken);

        logger.LogInformation("API key {KeyId} revoked", apiKeyId);
    }

    /// <summary>
    /// Delete API key permanently (EN)<br/>
    /// Xóa API key vĩnh viễn (VI)
    /// </summary>
    public async Task DeleteApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null)
            throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        await apiKeyRepository.DeleteAsync(apiKeyId, cancellationToken);

        logger.LogInformation("API key {KeyId} deleted", apiKeyId);
    }

    #endregion

    #region Security & Validation

    /// <summary>
    /// Verify API key with enhanced security checks (EN)<br/>
    /// Xác thực API key với kiểm tra bảo mật nâng cao (VI)
    /// </summary>
    public async Task<VerifyApiKeyResponse> VerifyApiKeyAsync(string rawApiKey, string clientIpAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(rawApiKey) || !rawApiKey.StartsWith(KeyPrefix))
            {
                return CreateInvalidResponse("Invalid API key format");
            }

            var keyHash = apiKeyHasher.HashApiKey(rawApiKey);
            var apiKey = await apiKeyRepository.GetActiveKeyByHashAsync(keyHash, cancellationToken);

            if (apiKey == null)
            {
                return CreateInvalidResponse("API key not found");
            }

            // Check basic validation
            var basicValidation = await ValidateBasicApiKey(apiKey, cancellationToken);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Check IP whitelist
            if (!await ValidateIpWhitelistAsync(apiKey, clientIpAddress))
            {
                return CreateInvalidResponse("IP address not allowed");
            }

            // Check rate limiting
            if (!await ValidateRateLimitAsync(apiKey))
            {
                return CreateInvalidResponse("Rate limit exceeded");
            }

            // Update usage tracking
            await UpdateUsageTrackingAsync(apiKey, cancellationToken);

            // Get user email for the response
            var user = await userRepository.GetByIdAsync(apiKey.UserId, cancellationToken);

            return new VerifyApiKeyResponse
            {
                IsValid = true,
                UserId = apiKey.UserId,
                UserEmail = user?.Email,
                ApiKeyId = apiKey.Id,
                Scopes = apiKey.Scopes,
                Message = "API key is valid"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying API key");
            return CreateInvalidResponse("Internal server error");
        }
    }

    /// <summary>
    /// Rotate API key (generate new key, keep same settings) (EN)<br/>
    /// Xoay API key (tạo key mới, giữ nguyên cài đặt) (VI)
    /// </summary>
    public async Task<RotateApiKeyResponse> RotateApiKeyAsync(Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null)
            throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        var oldKeyPrefix = apiKey.KeyPrefix;

        // Generate new key
        var (newRawKey, newKeyHash, newKeyPrefix) = GenerateSecureApiKey();

        // Update entity
        apiKey.KeyHash = newKeyHash;
        apiKey.KeyPrefix = newKeyPrefix;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await apiKeyRepository.UpdateAsync(apiKey, cancellationToken);

        logger.LogInformation("API key {KeyId} rotated", apiKeyId);

        return new RotateApiKeyResponse
        {
            Id = apiKey.Id,
            NewApiKey = newRawKey,
            NewKeyPrefix = newKeyPrefix,
            OldKeyPrefix = oldKeyPrefix,
            RotatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Usage Analytics

    /// <summary>
    /// Get usage analytics for API key (EN)<br/>
    /// Lấy phân tích sử dụng cho API key (VI)
    /// </summary>
    public async Task<ApiKeyUsageResponse> GetUsageAnalyticsAsync(Guid apiKeyId, UsageQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null)
            throw new KeyNotFoundException($"API key with ID {apiKeyId} not found");

        // Get usage logs
        var usageLogs = await apiKeyRepository.GetUsageLogsAsync(apiKeyId, 
            request.StartDate ?? DateTime.UtcNow.AddDays(-30),
            request.EndDate ?? DateTime.UtcNow,
            cancellationToken);

        // Calculate statistics
        var statistics = CalculateUsageStatistics(usageLogs, apiKey);

        // Group data by time period
        var usageData = GroupUsageData(usageLogs, request.GroupBy);

        // Get recent activities
        var recentActivities = usageLogs
            .OrderByDescending(log => log.Timestamp)
            .Take(request.Limit)
            .Select(MapToUsageLogDto)
            .ToList();

        return new ApiKeyUsageResponse
        {
            ApiKeyId = apiKeyId,
            Statistics = statistics,
            UsageData = usageData,
            RecentActivities = recentActivities
        };
    }

    #endregion

    #region Private Helper Methods

    private (string RawKey, string KeyHash, string KeyPrefix) GenerateSecureApiKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);

        var keyBody = Convert.ToBase64String(bytes);
        keyBody = keyBody.Replace("+", "-");
        keyBody = keyBody.Replace("/", "_");
        keyBody = keyBody.Replace("=", "");
        
        // Safely truncate to prevent substring errors
        var actualLength = Math.Min(keyBody.Length, KeyLength);
        keyBody = keyBody.Substring(0, actualLength);

        var rawKey = KeyPrefix + keyBody;
        var keyPrefix = KeyPrefix + keyBody.Substring(0, 6);
        var keyHash = apiKeyHasher.HashApiKey(rawKey);

        return (rawKey, keyHash, keyPrefix);
    }

    private void ValidateSecuritySettings(ApiKeySecuritySettingsDto settings)
    {
        if (settings is { AllowCorsRequests: true, AllowedOrigins.Count: 0 })
        {
            throw new ArgumentException("Allowed origins must be specified when CORS is enabled");
        }

        if (settings.MaxRequestsPerSecond < 1 || settings.MaxRequestsPerSecond > 1000)
        {
            throw new ArgumentException("Max requests per second must be between 1 and 1000");
        }
    }

    private void ValidateIpWhitelist(List<string> ipWhitelist)
    {
        // Validate parameters to prevent null dereference
        if (ipWhitelist == null || ipWhitelist.Count <= 0) return;
        var (isValid, errors) = ipValidationService.ValidateIpWhitelist(ipWhitelist);
        if (!isValid)
        {
            throw new ArgumentException($"Invalid IP whitelist: {string.Join(", ", errors)}");
        }
    }



    private async Task<VerifyApiKeyResponse> ValidateBasicApiKey(ApiKey apiKey, CancellationToken cancellationToken)
    {
        // Check if expired
        if (apiKey.IsExpired)
        {
            return CreateInvalidResponse("API key has expired");
        }

        // Check if revoked
        if (apiKey.IsRevoked)
        {
            return CreateInvalidResponse("API key has been revoked");
        }

        // Check user is still active
        var user = await userRepository.GetByIdAsync(apiKey.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return CreateInvalidResponse("User account is disabled or not found");
        }

        return new VerifyApiKeyResponse { IsValid = true };
    }

    private Task<bool> ValidateIpWhitelistAsync(ApiKey apiKey, string clientIpAddress)
    {
        if (!apiKey.SecuritySettings.EnableIpValidation || apiKey.IpWhitelist.Count == 0)
        {
            return Task.FromResult(true); // No IP validation enabled
        }

        return Task.FromResult(ipValidationService.IsIpAllowed(clientIpAddress, apiKey.IpWhitelist));
    }



    private async Task<bool> ValidateRateLimitAsync(ApiKey apiKey)
    {
        if (!apiKey.SecuritySettings.EnableRateLimiting)
        {
            return true;
        }

        // Check rate limiting (per minute)
        var rateLimitExceeded = await rateLimitingService.IsRateLimitExceededAsync(
            apiKey.Id, apiKey.RateLimitPerMinute);
        
        if (rateLimitExceeded)
        {
            return false;
        }

        // Check daily quota
        var dailyQuotaExceeded = await rateLimitingService.IsDailyQuotaExceededAsync(
            apiKey.Id, apiKey.DailyUsageQuota);
        
        return !dailyQuotaExceeded;
    }

    private async Task UpdateUsageTrackingAsync(ApiKey apiKey, CancellationToken cancellationToken)
    {
        apiKey.LastUsedAt = DateTime.UtcNow;
        apiKey.UsageCount++;
        apiKey.TodayUsageCount++;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await apiKeyRepository.UpdateAsync(apiKey, cancellationToken);
    }

    private VerifyApiKeyResponse CreateInvalidResponse(string message)
    {
        return new VerifyApiKeyResponse
        {
            IsValid = false,
            Message = message,
            ErrorMessage = message
        };
    }

    #endregion

    #region Mapping Methods

    private CreateApiKeyResponse MapToCreateApiKeyResponse(ApiKey apiKey, string rawKey)
    {
        return new CreateApiKeyResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Description = apiKey.Description,
            ApiKey = rawKey, // Only shown during creation
            KeyPrefix = apiKey.KeyPrefix,
            Scopes = apiKey.Scopes,
            RateLimitPerMinute = apiKey.RateLimitPerMinute,
            DailyUsageQuota = apiKey.DailyUsageQuota,
            IpWhitelist = apiKey.IpWhitelist,
            SecuritySettings = MapToSecuritySettingsDto(apiKey.SecuritySettings),
            CreatedAt = apiKey.CreatedAt ?? DateTime.UtcNow,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    private ApiKeyResponse MapToApiKeyResponse(ApiKey apiKey)
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
            SecuritySettings = MapToSecuritySettingsDto(apiKey.SecuritySettings),
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

    private ApiKeySecuritySettings MapToSecuritySettings(ApiKeySecuritySettingsDto dto)
    {
        return new ApiKeySecuritySettings
        {
            RequireHttps = dto.RequireHttps,
            AllowCorsRequests = dto.AllowCorsRequests,
            AllowedOrigins = dto.AllowedOrigins,
            EnableUsageAnalytics = dto.EnableUsageAnalytics,
            MaxRequestsPerSecond = dto.MaxRequestsPerSecond,
            EnableIpValidation = dto.EnableIpValidation,
            EnableRateLimiting = dto.EnableRateLimiting
        };
    }

    private ApiKeySecuritySettingsDto MapToSecuritySettingsDto(ApiKeySecuritySettings entity)
    {
        return new ApiKeySecuritySettingsDto
        {
            RequireHttps = entity.RequireHttps,
            AllowCorsRequests = entity.AllowCorsRequests,
            AllowedOrigins = entity.AllowedOrigins,
            EnableUsageAnalytics = entity.EnableUsageAnalytics,
            MaxRequestsPerSecond = entity.MaxRequestsPerSecond,
            EnableIpValidation = entity.EnableIpValidation,
            EnableRateLimiting = entity.EnableRateLimiting
        };
    }

    // Simplified analytics methods - full implementation would be more complex
    private UsageStatistics CalculateUsageStatistics(IEnumerable<ApiKeyUsageLog> logs, ApiKey apiKey)
    {
        var logsList = logs.ToList();
        var now = DateTime.UtcNow;

        return new UsageStatistics
        {
            TotalRequests = apiKey.UsageCount,
            SuccessfulRequests = logsList.Count(l => l.IsSuccess),
            FailedRequests = logsList.Count(l => !l.IsSuccess),
            AverageResponseTime = logsList.Any() ? logsList.Average(l => l.ResponseTime) : 0,
            RequestsToday = apiKey.TodayUsageCount,
            RequestsThisWeek = logsList.Count(l => l.Timestamp >= now.AddDays(-7)),
            RequestsThisMonth = logsList.Count(l => l.Timestamp >= now.AddDays(-30)),
            MostUsedEndpoint = logsList.GroupBy(l => l.Endpoint)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key,
            PeakUsageHour = logsList.GroupBy(l => l.Timestamp.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key
        };
    }

    private List<UsageDataPoint> GroupUsageData(IEnumerable<ApiKeyUsageLog> logs, string groupBy)
    {
        var logsList = logs.ToList();
        
        return groupBy.ToLower() switch
        {
            "hour" => logsList.GroupBy(l => new DateTime(l.Timestamp.Year, l.Timestamp.Month, l.Timestamp.Day, l.Timestamp.Hour, 0, 0))
                .Select(g => new UsageDataPoint
                {
                    Date = g.Key,
                    RequestCount = g.Count(),
                    ErrorCount = g.Count(l => !l.IsSuccess),
                    AverageResponseTime = g.Average(l => l.ResponseTime)
                }).ToList(),
            "month" => logsList.GroupBy(l => new DateTime(l.Timestamp.Year, l.Timestamp.Month, 1))
                .Select(g => new UsageDataPoint
                {
                    Date = g.Key,
                    RequestCount = g.Count(),
                    ErrorCount = g.Count(l => !l.IsSuccess),
                    AverageResponseTime = g.Average(l => l.ResponseTime)
                }).ToList(),
            _ => logsList.GroupBy(l => l.Timestamp.Date)
                .Select(g => new UsageDataPoint
                {
                    Date = g.Key,
                    RequestCount = g.Count(),
                    ErrorCount = g.Count(l => !l.IsSuccess),
                    AverageResponseTime = g.Average(l => l.ResponseTime)
                }).ToList()
        };
    }

    private ApiKeyUsageLogDto MapToUsageLogDto(ApiKeyUsageLog log)
    {
        return new ApiKeyUsageLogDto
        {
            Id = log.Id,
            Timestamp = log.Timestamp,
            Method = log.Method,
            Endpoint = log.Endpoint,
            StatusCode = log.StatusCode,
            ResponseTime = log.ResponseTime,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            RequestSize = log.RequestSize,
            ResponseSize = log.ResponseSize,
            ErrorMessage = log.ErrorMessage,
            ScopesUsed = log.ScopesUsed,
            IsSuccess = log.IsSuccess
        };
    }

    private static IEnumerable<ApiKey> ApplyFilters(IEnumerable<ApiKey> apiKeys, ListApiKeysQuery query)
    {
        var result = apiKeys.AsEnumerable();

        if (!string.IsNullOrEmpty(query.Status))
        {
            if (Enum.TryParse<ApiKeyStatus>(query.Status, true, out var status))
            {
                result = result.Where(k => k.Status == status);
            }
        }

        if (!string.IsNullOrEmpty(query.Scope))
        {
            result = result.Where(k => k.Scopes.Contains(query.Scope));
        }

        if (!string.IsNullOrEmpty(query.Search))
        {
            result = result.Where(k => k.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                                      (k.Description != null && k.Description.Contains(query.Search, StringComparison.OrdinalIgnoreCase)));
        }

        return result;
    }

    private static IEnumerable<ApiKey> ApplyPagination(IEnumerable<ApiKey> apiKeys, ListApiKeysQuery query)
    {
        return apiKeys.Take(query.Limit);
    }

    /// <summary>
    /// Exchange API key for JWT token (EN)<br/>
    /// Trao đổi API key lấy JWT token (VI)
    /// </summary>
    public async Task<ApiKeyExchangeResponse> ExchangeApiKeyForJwtAsync(string rawApiKey, string clientIpAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Attempting to exchange API key for JWT token");

            // First verify the API key using existing validation logic
            var verifyResult = await VerifyApiKeyAsync(rawApiKey, clientIpAddress, cancellationToken);
            
            if (!verifyResult.IsValid)
            {
                logger.LogWarning("API key exchange failed: {ErrorMessage}", verifyResult.ErrorMessage);
                throw new UnauthorizedAccessException(verifyResult.ErrorMessage ?? "Invalid API key");
            }

            // Get user information for JWT generation
            var user = await userRepository.GetByIdAsync(verifyResult.UserId!.Value, cancellationToken);
            if (user == null || !user.IsActive)
            {
                logger.LogWarning("User not found or inactive for API key exchange: {UserId}", verifyResult.UserId);
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Generate JWT token using existing JWT service
            var accessToken = jwtService.GenerateAccessToken(user);
            var expiresAt = jwtService.GetTokenExpiration();

            logger.LogInformation("Successfully exchanged API key for JWT token for user: {UserId}", user.Id);

            return new ApiKeyExchangeResponse
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                TokenType = "Bearer",
                UserId = user.Id,
                UserEmail = user.Email
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw authorization exceptions as-is
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during API key to JWT exchange");
            throw new InvalidOperationException("Failed to exchange API key for JWT token", ex);
        }
    }

    #endregion
} 