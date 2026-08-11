using FluentAssertions;
using Identity.Application.Services.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace Identity.Application.Tests.Services.Security;

/// <summary>
/// Unit tests for RateLimitingService - Kiểm thử đơn vị cho RateLimitingService (EN)<br/>
/// Kiểm thử đơn vị cho RateLimitingService (VI)
/// </summary>
public class RateLimitingServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<RateLimitingService>> _mockLogger;
    private readonly RateLimitingService _rateLimitingService;
    private readonly Guid _testApiKeyId = Guid.CreateVersion7();

    public RateLimitingServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<RateLimitingService>>();
        _rateLimitingService = new RateLimitingService(_mockCache.Object, _mockLogger.Object);
    }

    #region Helper Methods

    private byte[] SerializeToBytes(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return Encoding.UTF8.GetBytes(json);
    }

    #endregion

    #region IsRateLimitExceededAsync Tests

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenNoCacheEntry_ShouldReturnFalse()
    {
        // Arrange
        var limitPerMinute = 10;
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var expectedCacheKey = $"rate_limit:{_testApiKeyId}:{currentMinute}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _rateLimitingService.IsRateLimitExceededAsync(_testApiKeyId, limitPerMinute);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenBelowLimit_ShouldReturnFalse()
    {
        // Arrange
        var limitPerMinute = 10;
        var currentCount = 5;
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var expectedCacheKey = $"rate_limit:{_testApiKeyId}:{currentMinute}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(SerializeToBytes(currentCount));

        // Act
        var result = await _rateLimitingService.IsRateLimitExceededAsync(_testApiKeyId, limitPerMinute);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenAtLimit_ShouldReturnTrue()
    {
        // Arrange
        var limitPerMinute = 10;
        var currentCount = 10;
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var expectedCacheKey = $"rate_limit:{_testApiKeyId}:{currentMinute}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(SerializeToBytes(currentCount));

        // Act
        var result = await _rateLimitingService.IsRateLimitExceededAsync(_testApiKeyId, limitPerMinute);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenCacheThrowsException_ShouldReturnFalse()
    {
        // Arrange
        var limitPerMinute = 10;
        
        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("Cache unavailable"));

        // Act
        var result = await _rateLimitingService.IsRateLimitExceededAsync(_testApiKeyId, limitPerMinute);

        // Assert
        result.Should().BeFalse(); // Fail open approach
    }

    #endregion

    #region IsDailyQuotaExceededAsync Tests

    [Fact]
    public async Task IsDailyQuotaExceededAsync_WhenNoCacheEntry_ShouldReturnFalse()
    {
        // Arrange
        var dailyQuota = 1000;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var expectedCacheKey = $"daily_quota:{_testApiKeyId}:{today}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _rateLimitingService.IsDailyQuotaExceededAsync(_testApiKeyId, dailyQuota);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsDailyQuotaExceededAsync_WhenBelowQuota_ShouldReturnFalse()
    {
        // Arrange
        var dailyQuota = 1000;
        var currentCount = 500;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var expectedCacheKey = $"daily_quota:{_testApiKeyId}:{today}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(SerializeToBytes(currentCount));

        // Act
        var result = await _rateLimitingService.IsDailyQuotaExceededAsync(_testApiKeyId, dailyQuota);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsDailyQuotaExceededAsync_WhenAtQuota_ShouldReturnTrue()
    {
        // Arrange
        var dailyQuota = 1000;
        var currentCount = 1000;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var expectedCacheKey = $"daily_quota:{_testApiKeyId}:{today}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(SerializeToBytes(currentCount));

        // Act
        var result = await _rateLimitingService.IsDailyQuotaExceededAsync(_testApiKeyId, dailyQuota);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Usage Query Tests

    [Fact]
    public async Task GetCurrentRateLimitUsageAsync_WhenNoCacheEntry_ShouldReturnZero()
    {
        // Arrange
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var expectedCacheKey = $"rate_limit:{_testApiKeyId}:{currentMinute}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _rateLimitingService.GetCurrentRateLimitUsageAsync(_testApiKeyId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentRateLimitUsageAsync_WhenCacheHasValue_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 25;
        var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var expectedCacheKey = $"rate_limit:{_testApiKeyId}:{currentMinute}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(SerializeToBytes(expectedCount));

        // Act
        var result = await _rateLimitingService.GetCurrentRateLimitUsageAsync(_testApiKeyId);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetCurrentDailyQuotaUsageAsync_WhenNoCacheEntry_ShouldReturnZero()
    {
        // Arrange
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var expectedCacheKey = $"daily_quota:{_testApiKeyId}:{today}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _rateLimitingService.GetCurrentDailyQuotaUsageAsync(_testApiKeyId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentDailyQuotaUsageAsync_WhenCacheHasValue_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 750;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var expectedCacheKey = $"daily_quota:{_testApiKeyId}:{today}";
        
        _mockCache.Setup(x => x.GetAsync(expectedCacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(SerializeToBytes(expectedCount));

        // Act
        var result = await _rateLimitingService.GetCurrentDailyQuotaUsageAsync(_testApiKeyId);

        // Assert
        result.Should().Be(expectedCount);
    }

    #endregion
} 