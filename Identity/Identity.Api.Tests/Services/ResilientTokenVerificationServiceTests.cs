using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using Identity.Api.Services;
using Identity.Contracts;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Identity.Api.Tests.Services;

/// <summary>
/// Comprehensive tests for ResilientTokenVerificationService
/// Tests resilience patterns: circuit breaker, retry, timeout, fallback mechanisms
/// </summary>
public class ResilientTokenVerificationServiceTests
{    private readonly Mock<ITokenVerificationService> _mockEnhancedService;
    private readonly Mock<ILogger<ResilientTokenVerificationService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly Mock<TelemetryService> _mockTelemetryService;

    public ResilientTokenVerificationServiceTests()
    {
        _mockEnhancedService = new Mock<ITokenVerificationService>();        _mockLogger = new Mock<ILogger<ResilientTokenVerificationService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockDistributedCache = new Mock<IDistributedCache>();
        _mockTelemetryService = new Mock<TelemetryService>();
    }

    /// <summary>
    /// Helper method to create ResilientTokenVerificationService with all mocked dependencies
    /// Phương thức trợ giúp để tạo ResilientTokenVerificationService với tất cả dependencies được mock
    /// </summary>
    private ResilientTokenVerificationService CreateResilientService()
    {
        return new ResilientTokenVerificationService(
            _mockEnhancedService.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockMemoryCache.Object,
            _mockDistributedCache.Object,
            _mockTelemetryService.Object
        );
    }

    [Fact]
    public async Task VerifyTokenAsync_WhenEnhancedServiceSucceeds_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = new SocialUserInfo 
        { 
            Id = "test-id", 
            Email = "test@example.com", 
            Name = "Test User",
            Provider = "google"
        };
        
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "valid-token"))
            .ReturnsAsync(expectedResult);

        var resilientService = CreateResilientService();        // Act
        var result = await resilientService.VerifyTokenAsync("google", "valid-token");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        _mockEnhancedService.Verify(x => x.VerifyTokenAsync("google", "valid-token"), Times.Once);
    }

    [Fact]
    public async Task VerifyTokenAsync_WhenEnhancedServiceFails_ShouldUseFallback()
    {
        // Arrange
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "failing-token"))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Setup distributed cache to return a fallback result
        var fallbackData = """
            {
                "Id": "fallback-id",
                "Email": "fallback@example.com",
                "Name": "Fallback User",
                "Provider": "google"
            }
            """;
          _mockDistributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(fallbackData));

        var resilientService = CreateResilientService();        // Act
        var result = await resilientService.VerifyTokenAsync("google", "failing-token");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("fallback-id");
        result.Provider.Should().Be("google_fallback");
        
        // Verify fallback mechanism was used
        _mockDistributedCache.Verify(x => x.GetAsync(It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task VerifyGoogleTokenAsync_ShouldCallVerifyTokenAsync()
    {
        // Arrange
        var expectedResult = new SocialUserInfo 
        { 
            Id = "google-id", 
            Email = "google@example.com", 
            Name = "Google User",
            Provider = "google"
        };
        
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "google-token"))
            .ReturnsAsync(expectedResult);

        var resilientService = CreateResilientService();        // Act
        var result = await resilientService.VerifyGoogleTokenAsync("google-token");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        _mockEnhancedService.Verify(x => x.VerifyTokenAsync("google", "google-token"), Times.Once);
    }

    [Fact]
    public async Task VerifyFacebookTokenAsync_ShouldCallVerifyTokenAsync()
    {
        // Arrange
        var expectedResult = new SocialUserInfo 
        { 
            Id = "facebook-id", 
            Email = "facebook@example.com", 
            Name = "Facebook User",
            Provider = "facebook"
        };
        
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("facebook", "facebook-token"))
            .ReturnsAsync(expectedResult);

        var resilientService = CreateResilientService();        // Act
        var result = await resilientService.VerifyFacebookTokenAsync("facebook-token");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        _mockEnhancedService.Verify(x => x.VerifyTokenAsync("facebook", "facebook-token"), Times.Once);
    }

    [Fact]
    public async Task VerifyTokenAsync_WithCircuitBreakerOpen_ShouldUseFallback()
    {
        // Arrange
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "circuit-breaker-token"))
            .ThrowsAsync(new BrokenCircuitException("Circuit breaker is open"));

        var fallbackData = """
            {
                "Id": "circuit-fallback-id",
                "Email": "circuit@example.com",
                "Name": "Circuit Fallback User",
                "Provider": "google"
            }
            """;
          _mockDistributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(fallbackData));

        var resilientService = CreateResilientService();

        // Act
        var result = await resilientService.VerifyTokenAsync("google", "circuit-breaker-token");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("circuit-fallback-id");
        result.Provider.Should().Be("google_fallback");
        
        // Verify warning was logged for circuit breaker
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("circuit breaker is open")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyTokenAsync_WithTimeoutException_ShouldRetryAndFallback()
    {
        // Arrange
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "timeout-token"))
            .ThrowsAsync(new TimeoutRejectedException("Request timed out"));        _mockDistributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null); // No cached fallback

        var resilientService = CreateResilientService();

        // Act
        var result = await resilientService.VerifyTokenAsync("google", "timeout-token");

        // Assert
        result.Should().BeNull(); // No fallback available, should return null
        
        // Verify enhanced service was called (retry mechanism)
        _mockEnhancedService.Verify(x => x.VerifyTokenAsync("google", "timeout-token"), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("google")]
    [InlineData("facebook")]
    [InlineData("twitter")]
    public async Task VerifyTokenAsync_WithDifferentProviders_ShouldHandleAllProviders(string provider)
    {
        // Arrange
        var token = $"{provider}_token";
        var expectedUserInfo = new SocialUserInfo
        {
            Id = $"{provider}_123",
            Email = $"{provider}@example.com",
            Name = $"{provider} User",
            Provider = provider
        };

        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync(provider, token))
            .ReturnsAsync(expectedUserInfo);

        var resilientService = CreateResilientService();

        // Act
        var result = await resilientService.VerifyTokenAsync(provider, token);

        // Assert
        result.Should().NotBeNull();
        result!.Provider.Should().Be(provider);
        result.Should().BeEquivalentTo(expectedUserInfo);
        _mockEnhancedService.Verify(x => x.VerifyTokenAsync(provider, token), Times.Once);
    }

    [Fact]
    public async Task VerifyTokenAsync_WithNullTokenResponse_ShouldReturnNull()
    {
        // Arrange
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "null-token"))
            .ReturnsAsync((SocialUserInfo?)null);

        var resilientService = CreateResilientService();

        // Act
        var result = await resilientService.VerifyTokenAsync("google", "null-token");

        // Assert
        result.Should().BeNull();
        _mockEnhancedService.Verify(x => x.VerifyTokenAsync("google", "null-token"), Times.Once);
    }

    [Fact]
    public async Task VerifyTokenAsync_WithCriticalException_ShouldLogErrorAndFallback()
    {
        // Arrange
        var criticalException = new InvalidOperationException("Critical system error");
        
        _mockEnhancedService
            .Setup(x => x.VerifyTokenAsync("google", "critical-error-token"))
            .ThrowsAsync(criticalException);        _mockDistributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null);

        var resilientService = CreateResilientService();

        // Act
        var result = await resilientService.VerifyTokenAsync("google", "critical-error-token");

        // Assert
        result.Should().BeNull();
        
        // Verify error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Critical error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
