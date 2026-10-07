using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Identity.Application.Common.Interfaces;
using Identity.Contracts;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Identity.Api.Tests.Integration;

/// <summary>
///     Integration tests for API Key CRUD operations with authentication (EN)<br/>
///     Tests tích hợp cho các thao tác CRUD API Key với authentication (VI)
/// </summary>
public class ApiKeysIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly string _testDatabaseName;
    
    // Test user data
    private readonly User _testUser;
    private string? _jwtToken;
    private string? _testApiKey;

    public ApiKeysIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _testDatabaseName = $"TestDb_{Guid.CreateVersion7():N}";
        
        // Identity.Api Program.cs reads these eagerly while building the host (before ConfigureAppConfiguration overrides apply),
        // so they must be process-level environment variables.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "test_secret_key_for_testing_minimum_32_characters_long");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove every registration of the Npgsql-configured DbContext (EF Core 9 keeps provider config in IDbContextOptionsConfiguration)
                services.RemoveAll<DbContextOptions<IdentityDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<IdentityDbContext>>();

                // Add test database
                services.AddDbContext<IdentityDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_testDatabaseName);
                    options.EnableSensitiveDataLogging();
                });

            });
        });

        _client = _factory.CreateClient();
        
        // Create test user
        _testUser = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "testuser@tihomo.local",
            Username = "testuser",
            Name = "Test User",
            FullName = "Test User",
            PasswordHash = "test_hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #region Setup and Helpers

    /// <summary>
    ///     Initialize test database and authenticate user (EN)<br/>
    ///     Khởi tạo test database và authenticate user (VI)
    /// </summary>
    private async Task InitializeTestAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Add test user
        context.Users.Add(_testUser);
        await context.SaveChangesAsync();
        
        // Generate JWT token for test user
        _jwtToken = await GenerateJwtTokenAsync(_testUser.Id);
        
        // Set default authorization header
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        
        _output.WriteLine($"Test initialized with user: {_testUser.Email}");
    }

    /// <summary>
    ///     Generate JWT token for test user (EN)<br/>
    ///     Tạo JWT token cho test user (VI)
    /// </summary>
    private Task<string> GenerateJwtTokenAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();

        return Task.FromResult(jwtService.GenerateAccessToken(_testUser));
    }

    /// <summary>
    ///     Create HTTP content from object (EN)<br/>
    ///     Tạo HTTP content từ object (VI)
    /// </summary>
    private static StringContent CreateJsonContent<T>(T obj)
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
    private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    #endregion

    #region API Key CRUD Tests

    [Fact]
    public async Task CreateApiKey_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        await InitializeTestAsync();
        
        var request = new CreateApiKeyRequest
        {
            Name = "Test API Key",
            Description = "Test API key for integration testing",
            Scopes = ["read", "write"],
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RateLimitPerMinute = 100
        };

        // Act
        var response = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(request));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiKeyResponse = await DeserializeResponseAsync<CreateApiKeyResponse>(response);
        apiKeyResponse.Should().NotBeNull();
        apiKeyResponse!.Id.Should().NotBeEmpty();
        apiKeyResponse.Name.Should().Be(request.Name);
        apiKeyResponse.ApiKey.Should().NotBeNullOrEmpty();
        apiKeyResponse.ApiKey.Should().StartWith("tihomo_");
        
        // Store for cleanup
        _testApiKey = apiKeyResponse.ApiKey;
        
        _output.WriteLine($"Created API Key: {apiKeyResponse.Id} with key: {apiKeyResponse.ApiKey[..20]}...");
    }

    [Fact]
    public async Task CreateApiKey_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await InitializeTestAsync();
        
        var request = new CreateApiKeyRequest
        {
            Name = "", // Invalid: empty name
            Description = "Test description",
            Scopes = ["read"]
        };

        // Act
        var response = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(request));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        _output.WriteLine($"Bad request returned as expected: {response.StatusCode}");
    }

    [Fact]
    public async Task GetApiKeys_WithValidAuth_ShouldReturnUserKeys()
    {
        // Arrange
        await InitializeTestAsync();
        
        // First create an API key
        var createRequest = new CreateApiKeyRequest
        {
            Name = "Test Key for Get",
            Description = "Test key for retrieval test",
            Scopes = ["read"]
        };
        
        var createResponse = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(createRequest));
        createResponse.EnsureSuccessStatusCode();

        // Act
        var getResponse = await _client.GetAsync("/api/v1/api-keys");
        
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiKeys = await DeserializeResponseAsync<ListApiKeysResponse>(getResponse);
        apiKeys.Should().NotBeNull();
        apiKeys!.Data.Should().HaveCountGreaterThan(0);
        apiKeys.Data.First().Name.Should().Be("Test Key for Get");
        
        _output.WriteLine($"Retrieved {apiKeys.Data.Count} API keys for user");
    }

    [Fact]
    public async Task GetApiKey_WithValidId_ShouldReturnApiKeyResponse()
    {
        // Arrange
        await InitializeTestAsync();
        
        // Create API key first
        var createRequest = new CreateApiKeyRequest
        {
            Name = "Test Key for Individual Get",
            Description = "Test key for individual retrieval",
            Scopes = ["read"]
        };
        
        var createResponse = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(createRequest));
        var createdKey = await DeserializeResponseAsync<CreateApiKeyResponse>(createResponse);

        // Act
        var getResponse = await _client.GetAsync($"/api/v1/api-keys/{createdKey!.Id}");
        
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiKeyInfo = await DeserializeResponseAsync<ApiKeyResponse>(getResponse);
        apiKeyInfo.Should().NotBeNull();
        apiKeyInfo!.Id.Should().Be(createdKey.Id);
        apiKeyInfo.Name.Should().Be("Test Key for Individual Get");
        
        _output.WriteLine($"Retrieved API key info: {apiKeyInfo.Id}");
    }

    [Fact]
    public async Task GetApiKey_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await InitializeTestAsync();
        var invalidId = Guid.CreateVersion7();

        // Act
        var response = await _client.GetAsync($"/api/v1/api-keys/{invalidId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        _output.WriteLine($"Not found returned as expected for invalid ID: {invalidId}");
    }

    [Fact]
    public async Task RevokeApiKey_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        await InitializeTestAsync();
        
        // Create API key first
        var createRequest = new CreateApiKeyRequest
        {
            Name = "Test Key for Revocation",
            Description = "Test key for revocation test",
            Scopes = ["read"]
        };
        
        var createResponse = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(createRequest));
        var createdKey = await DeserializeResponseAsync<CreateApiKeyResponse>(createResponse);

        // Act
        var revokeResponse = await _client.DeleteAsync($"/api/v1/api-keys/{createdKey!.Id}");
        
        // Assert
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify key is actually revoked by trying to get it
        var getResponse = await _client.GetAsync($"/api/v1/api-keys/{createdKey.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        _output.WriteLine($"Successfully revoked API key: {createdKey.Id}");
    }

    [Fact]
    public async Task RevokeApiKey_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await InitializeTestAsync();
        var invalidId = Guid.CreateVersion7();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/api-keys/{invalidId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        _output.WriteLine($"Not found returned as expected for revocation with invalid ID: {invalidId}");
    }

    [Fact]
    public async Task ValidateApiKey_WithValidKey_ShouldReturnSuccess()
    {
        // Arrange
        await InitializeTestAsync();
        
        // Create API key first
        var createRequest = new CreateApiKeyRequest
        {
            Name = "Test Key for Validation",
            Description = "Test key for validation test",
            Scopes = ["read"]
        };
        
        var createResponse = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(createRequest));
        var createdKey = await DeserializeResponseAsync<CreateApiKeyResponse>(createResponse);

        // Act
        var validateResponse = await _client.PostAsync("/api/v1/api-keys/verify", 
            CreateJsonContent(createdKey!.ApiKey));
        
        // Assert
        validateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var validationResult = await validateResponse.Content.ReadAsStringAsync();
        validationResult.Should().Contain("\"isValid\":true");
        validationResult.Should().Contain(_testUser.Id.ToString());
        
        _output.WriteLine($"API key validation successful: {createdKey.ApiKey[..20]}...");
    }

    [Fact]  
    public async Task ValidateApiKey_WithInvalidKey_ShouldReturnIsValidFalse()
    {
        // Arrange
        await InitializeTestAsync();
        var invalidApiKey = "tihomo_invalid_key_12345";

        // Act
        var response = await _client.PostAsync("/api/v1/api-keys/verify", 
            CreateJsonContent(invalidApiKey));
        
        // Assert
        // Contract: /verify answers 200 with IsValid=false (EnhancedApiKeysController has no 401 path)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<VerifyApiKeyResponse>(response);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
        result.UserId.Should().BeNull();
        
        _output.WriteLine("IsValid=false returned as expected for invalid API key");
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task ApiKeyEndpoints_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        await InitializeTestAsync();
        
        // Remove authorization header
        _client.DefaultRequestHeaders.Authorization = null;

        // Act & Assert - Test multiple endpoints
        var endpoints = new[]
        {
            "/api/v1/api-keys",
            $"/api/v1/api-keys/{Guid.CreateVersion7()}"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _output.WriteLine($"Unauthorized returned for {endpoint} without auth");
        }
    }

    [Fact]
    public async Task ApiKeyEndpoints_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        await InitializeTestAsync();
        
        // Set invalid token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token");

        // Act
        var response = await _client.GetAsync("/api/v1/api-keys");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        _output.WriteLine("Unauthorized returned for invalid JWT token");
    }

    #endregion

    #region Rate Limiting and Security Tests

    [Fact]
    public async Task CreateApiKey_WithRateLimit_ShouldRespectLimits()
    {
        // Arrange
        await InitializeTestAsync();
        
        var request = new CreateApiKeyRequest
        {
            Name = "Rate Limited Key",
            Description = "Test key with rate limiting",
            Scopes = ["read"],
            RateLimitPerMinute = 1 // Very low limit for testing
        };

        // Act
        var response = await _client.PostAsync("/api/v1/api-keys", CreateJsonContent(request));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiKeyResponse = await DeserializeResponseAsync<CreateApiKeyResponse>(response);
        apiKeyResponse.Should().NotBeNull();
        apiKeyResponse!.RateLimitPerMinute.Should().Be(1);
        
        _output.WriteLine($"Created rate-limited API key: {apiKeyResponse.RateLimitPerMinute} req/min");
    }

    #endregion

    #region Cleanup

    public async ValueTask DisposeAsync()
    {
        try
        {
            // Clean up test database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await context.Database.EnsureDeletedAsync();
            
            _client.Dispose();
            
            _output.WriteLine($"Test cleanup completed for database: {_testDatabaseName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error during cleanup: {ex.Message}");
        }
    }

    #endregion
}