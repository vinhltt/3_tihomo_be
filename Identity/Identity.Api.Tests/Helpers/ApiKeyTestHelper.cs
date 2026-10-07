using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Identity.Application.Common.Interfaces;
using Identity.Contracts;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Api.Tests.Helpers;

/// <summary>
///     Helper class for API Key testing operations (EN)<br/>
///     Lớp helper cho các thao tác testing API Key (VI)
/// </summary>
public class ApiKeyTestHelper
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApiKeyTestHelper> _logger;

    public ApiKeyTestHelper(HttpClient httpClient, IServiceProvider serviceProvider, ILogger<ApiKeyTestHelper> logger)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    #region Test User Management

    /// <summary>
    ///     Create a test user in the database (EN)<br/>
    ///     Tạo test user trong database (VI)
    /// </summary>
    public async Task<User> CreateTestUserAsync(string email = "testuser@tihomo.local", string username = "testuser")
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            Username = username,
            Name = "Test User",
            FullName = "Test User",
            PasswordHash = "test_hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created test user: {Email} with ID: {UserId}", email, user.Id);
        return user;
    }

    /// <summary>
    ///     Generate JWT token for test user (EN)<br/>
    ///     Tạo JWT token cho test user (VI)
    /// </summary>
    public Task<string> GenerateJwtTokenAsync(User user)
    {
        using var scope = _serviceProvider.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        
        var token = jwtService.GenerateAccessToken(user);
        _logger.LogInformation("Generated JWT token for user: {UserId}", user.Id);
        return Task.FromResult(token);
    }

    #endregion

    #region API Key Management

    /// <summary>
    ///     Create a test API key via HTTP API (EN)<br/>
    ///     Tạo test API key qua HTTP API (VI)
    /// </summary>
    public async Task<CreateApiKeyResponse?> CreateTestApiKeyAsync(
        string jwtToken,
        string name = "Test API Key",
        string? description = null,
        string[]? scopes = null,
        DateTime? expiresAt = null)
    {
        var request = new CreateApiKeyRequest
        {
            Name = name,
            Description = description ?? "Test API key for integration testing",
            Scopes = (scopes ?? new[] { "read", "write" }).ToList(),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30),
            RateLimitPerMinute = 100
        };

        // Set JWT authorization
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await _httpClient.PostAsync("/api/v1/api-keys", CreateJsonContent(request));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create API key: {Error}", error);
            return null;
        }

        var apiKeyResponse = await DeserializeResponseAsync<CreateApiKeyResponse>(response);
        _logger.LogInformation("Created test API key: {ApiKeyId} with key: {ApiKey}", 
            apiKeyResponse?.Id, apiKeyResponse?.ApiKey?[..20] + "...");
        
        return apiKeyResponse;
    }

    /// <summary>
    ///     Create API key directly via service (for faster testing) (EN)<br/>
    ///     Tạo API key trực tiếp qua service (cho testing nhanh hơn) (VI)
    /// </summary>
    public async Task<CreateApiKeyResponse?> CreateTestApiKeyDirectAsync(
        Guid userId,
        string name = "Test API Key Direct",
        string? description = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var apiKeyService = scope.ServiceProvider.GetRequiredService<IEnhancedApiKeyService>();

        var request = new CreateApiKeyRequest
        {
            Name = name,
            Description = description ?? "Test API key created directly",
            Scopes = ["read", "write"],
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RateLimitPerMinute = 100
        };

        var result = await apiKeyService.CreateApiKeyAsync(userId, request);
        _logger.LogInformation("Created test API key directly: {ApiKeyId}", result?.Id);
        
        return result;
    }

    /// <summary>
    ///     Validate API key via HTTP API (EN)<br/>
    ///     Validate API key qua HTTP API (VI)
    /// </summary>
    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        var response = await _httpClient.PostAsync("/api/v1/api-keys/verify", CreateJsonContent(apiKey));
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API key validation result: {Result}", result);
            return result.Contains("\"isValid\":true");
        }

        _logger.LogWarning("API key validation failed with status: {Status}", response.StatusCode);
        return false;
    }

    #endregion

    #region HTTP Client Configuration

    /// <summary>
    ///     Set JWT authentication header (EN)<br/>
    ///     Set JWT authentication header (VI)
    /// </summary>
    public void SetJwtAuthentication(string jwtToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        _logger.LogDebug("Set JWT authentication header");
    }

    /// <summary>
    ///     Set API Key authentication header (EN)<br/>
    ///     Set API Key authentication header (VI)
    /// </summary>
    public void SetApiKeyAuthentication(string apiKey)
    {
        _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        _logger.LogDebug("Set API Key authentication header: {ApiKey}", apiKey[..20] + "...");
    }

    /// <summary>
    ///     Clear all authentication headers (EN)<br/>
    ///     Xóa tất cả authentication headers (VI)
    /// </summary>
    public void ClearAuthentication()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
        _logger.LogDebug("Cleared all authentication headers");
    }

    #endregion

    #region Test Data Generation

    /// <summary>
    ///     Generate random test API key request (EN)<br/>
    ///     Tạo random test API key request (VI)
    /// </summary>
    public CreateApiKeyRequest GenerateRandomApiKeyRequest(string prefix = "Test")
    {
        var random = new Random();
        var uniqueId = random.Next(1000, 9999);
        
        return new CreateApiKeyRequest
        {
            Name = $"{prefix} API Key {uniqueId}",
            Description = $"Generated test API key for testing purposes - {uniqueId}",
            Scopes = ["read", "write", "admin"],
            ExpiresAt = DateTime.UtcNow.AddDays(random.Next(7, 365)),
            RateLimitPerMinute = random.Next(10, 1000)
        };
    }

    /// <summary>
    ///     Generate multiple test users (EN)<br/>
    ///     Tạo nhiều test users (VI)
    /// </summary>
    public async Task<List<User>> CreateMultipleTestUsersAsync(int count)
    {
        var users = new List<User>();
        
        for (int i = 0; i < count; i++)
        {
            var user = await CreateTestUserAsync(
                email: $"testuser{i}@tihomo.local",
                username: $"testuser{i}"
            );
            users.Add(user);
        }
        
        _logger.LogInformation("Created {Count} test users", count);
        return users;
    }

    #endregion

    #region Cleanup Helpers

    /// <summary>
    ///     Clean up test data from database (EN)<br/>
    ///     Dọn dẹp test data từ database (VI)
    /// </summary>
    public async Task CleanupTestDataAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        // Remove test users and their API keys
        var testUsers = context.Users
            .Where(u => u.Email.Contains("test") || u.Username.Contains("test"))
            .ToList();

        if (testUsers.Any())
        {
            // Remove associated API keys first
            var userIds = testUsers.Select(u => u.Id).ToList();
            var apiKeys = context.ApiKeys.Where(k => userIds.Contains(k.UserId)).ToList();
            context.ApiKeys.RemoveRange(apiKeys);

            // Remove users
            context.Users.RemoveRange(testUsers);
            
            await context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {UserCount} test users and {ApiKeyCount} API keys", 
                testUsers.Count, apiKeys.Count);
        }
    }

    #endregion

    #region HTTP Helpers

    /// <summary>
    ///     Create HTTP content from object (EN)<br/>
    ///     Tạo HTTP content từ object (VI)
    /// </summary>
    public static StringContent CreateJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    ///     Deserialize HTTP response content (EN)<br/>
    ///     Deserialize nội dung HTTP response (VI)
    /// </summary>
    public static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    #endregion

    #region Validation Helpers

    /// <summary>
    ///     Validate API key format (EN)<br/>
    ///     Validate format của API key (VI)
    /// </summary>
    public static bool IsValidApiKeyFormat(string apiKey)
    {
        // API keys should start with "tihomo_" and be at least 40 characters
        return !string.IsNullOrEmpty(apiKey) && 
               apiKey.StartsWith("tihomo_") && 
               apiKey.Length >= 40;
    }

    /// <summary>
    ///     Validate JWT token format (EN)<br/>
    ///     Validate format của JWT token (VI)
    /// </summary>
    public static bool IsValidJwtFormat(string jwtToken)
    {
        // JWT tokens should have 3 parts separated by dots
        return !string.IsNullOrEmpty(jwtToken) && 
               jwtToken.Split('.').Length == 3;
    }

    #endregion
}