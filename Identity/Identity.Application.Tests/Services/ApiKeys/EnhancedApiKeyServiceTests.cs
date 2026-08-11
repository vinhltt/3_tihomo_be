using FluentAssertions;
using Identity.Application.Common.Interfaces;
using Identity.Application.Services.ApiKeys;
using Identity.Application.Services.Security;
using Identity.Contracts;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Application.Tests.Services.ApiKeys;

/// <summary>
/// Unit tests for EnhancedApiKeyService - Kiểm thử đơn vị cho EnhancedApiKeyService (EN)<br/>
/// Kiểm thử đơn vị cho EnhancedApiKeyService (VI)
/// </summary>
public class EnhancedApiKeyServiceTests
{
    private readonly Mock<IApiKeyRepository> _mockApiKeyRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IApiKeyHasher> _mockApiKeyHasher;
    private readonly Mock<IRateLimitingService> _mockRateLimitingService;
    private readonly Mock<IIpValidationService> _mockIpValidationService;
    private readonly Mock<IJwtService> _mockIJwtService;
    private readonly Mock<ILogger<EnhancedApiKeyService>> _mockLogger;
    private readonly EnhancedApiKeyService _service;

    private readonly Guid _testUserId = Guid.CreateVersion7();
    private readonly Guid _testApiKeyId = Guid.CreateVersion7();

    public EnhancedApiKeyServiceTests()
    {
        _mockApiKeyRepository = new Mock<IApiKeyRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockApiKeyHasher = new Mock<IApiKeyHasher>();
        _mockRateLimitingService = new Mock<IRateLimitingService>();
        _mockIpValidationService = new Mock<IIpValidationService>();
        _mockIJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<EnhancedApiKeyService>>();

        _service = new EnhancedApiKeyService(
            _mockApiKeyRepository.Object,
            _mockUserRepository.Object,
            _mockApiKeyHasher.Object,
            _mockRateLimitingService.Object,
            _mockIpValidationService.Object,
            _mockIJwtService.Object,
            _mockLogger.Object);
    }

    #region CreateApiKeyAsync Tests

    [Fact]
    public async Task CreateApiKeyAsync_WhenUserExists_ShouldCreateApiKey()
    {
        // Arrange
        var user = new User { Id = _testUserId, IsActive = true };
        var request = new CreateApiKeyRequest
        {
            Name = "Test API Key",
            Description = "Test Description"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockApiKeyRepository.Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mockApiKeyHasher.Setup(x => x.HashApiKey(It.IsAny<string>()))
            .Returns("hashedkey");
        _mockIpValidationService.Setup(x => x.ValidateIpWhitelist(It.IsAny<List<string>>()))
            .Returns((true, new List<string>()));

        // Act
        var result = await _service.CreateApiKeyAsync(_testUserId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.ApiKey.Should().StartWith("tihomo_");
        _mockApiKeyRepository.Verify(x => x.AddAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateApiKeyAsync_WhenUserNotFound_ShouldThrowException()
    {
        // Arrange
        var request = new CreateApiKeyRequest { Name = "Test" };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _service.Invoking(x => x.CreateApiKeyAsync(_testUserId, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found or inactive");
    }

    [Fact]
    public async Task CreateApiKeyAsync_WhenUserInactive_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = _testUserId, IsActive = false };
        var request = new CreateApiKeyRequest { Name = "Test" };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await _service.Invoking(x => x.CreateApiKeyAsync(_testUserId, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found or inactive");
    }

    [Fact]
    public async Task CreateApiKeyAsync_WhenUserExceedsKeyLimit_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = _testUserId, IsActive = true };
        var request = new CreateApiKeyRequest { Name = "Test" };
        var existingKeys = Enumerable.Range(1, 10).Select(_ => new ApiKey()).ToList();

        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockApiKeyRepository.Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingKeys);

        // Act & Assert
        await _service.Invoking(x => x.CreateApiKeyAsync(_testUserId, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Maximum limit of 10 API keys exceeded");
    }

    #endregion

    #region VerifyApiKeyAsync Tests

    [Fact]
    public async Task VerifyApiKeyAsync_WhenValidKey_ShouldReturnValidResponse()
    {
        // Arrange
        var rawApiKey = "tihomo_validkey12345";
        var hashedKey = "hashedkey";
        var clientIp = "192.168.1.1";
        var user = new User { Id = _testUserId, IsActive = true };
        var apiKey = new ApiKey
        {
            Id = _testApiKeyId,
            UserId = _testUserId,
            KeyHash = hashedKey,
            Status = Domain.Enums.ApiKeyStatus.Active,
            User = user,
            SecuritySettings = new ApiKeySecuritySettings { EnableIpValidation = false, EnableRateLimiting = false }
        };

        _mockApiKeyHasher.Setup(x => x.HashApiKey(rawApiKey)).Returns(hashedKey);
        _mockApiKeyRepository.Setup(x => x.GetActiveKeyByHashAsync(hashedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.VerifyApiKeyAsync(rawApiKey, clientIp);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be(_testUserId);
        result.ApiKeyId.Should().Be(_testApiKeyId);
    }

    [Fact]
    public async Task VerifyApiKeyAsync_WhenInvalidFormat_ShouldReturnInvalidResponse()
    {
        // Arrange
        var rawApiKey = "invalid_key_format";
        var clientIp = "192.168.1.1";

        // Act
        var result = await _service.VerifyApiKeyAsync(rawApiKey, clientIp);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("Invalid API key format");
    }

    [Fact]
    public async Task VerifyApiKeyAsync_WhenKeyNotFound_ShouldReturnInvalidResponse()
    {
        // Arrange
        var rawApiKey = "tihomo_notfoundkey12345";
        var hashedKey = "hashedkey";
        var clientIp = "192.168.1.1";

        _mockApiKeyHasher.Setup(x => x.HashApiKey(rawApiKey)).Returns(hashedKey);
        _mockApiKeyRepository.Setup(x => x.GetActiveKeyByHashAsync(hashedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKey?)null);

        // Act
        var result = await _service.VerifyApiKeyAsync(rawApiKey, clientIp);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("API key not found");
    }

    [Fact]
    public async Task VerifyApiKeyAsync_WhenRateLimitExceeded_ShouldReturnInvalidResponse()
    {
        // Arrange
        var rawApiKey = "tihomo_validkey12345";
        var hashedKey = "hashedkey";
        var clientIp = "192.168.1.1";
        var user = new User { Id = _testUserId, IsActive = true };
        var apiKey = new ApiKey
        {
            Id = _testApiKeyId,
            UserId = _testUserId,
            KeyHash = hashedKey,
            Status = Domain.Enums.ApiKeyStatus.Active,
            User = user,
            SecuritySettings = new ApiKeySecuritySettings { EnableRateLimiting = true }
        };

        _mockApiKeyHasher.Setup(x => x.HashApiKey(rawApiKey)).Returns(hashedKey);
        _mockApiKeyRepository.Setup(x => x.GetActiveKeyByHashAsync(hashedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRateLimitingService.Setup(x => x.IsRateLimitExceededAsync(_testApiKeyId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VerifyApiKeyAsync(rawApiKey, clientIp);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("Rate limit exceeded");
    }

    #endregion

    #region GetApiKeyByIdAsync Tests

    [Fact]
    public async Task GetApiKeyByIdAsync_WhenKeyExists_ShouldReturnApiKey()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = _testApiKeyId,
            Name = "Test Key",
            Status = Domain.Enums.ApiKeyStatus.Active
        };

        _mockApiKeyRepository.Setup(x => x.GetByIdAsync(_testApiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);

        // Act
        var result = await _service.GetApiKeyByIdAsync(_testApiKeyId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testApiKeyId);
        result.Name.Should().Be("Test Key");
    }

    [Fact]
    public async Task GetApiKeyByIdAsync_WhenKeyNotFound_ShouldThrowException()
    {
        // Arrange
        _mockApiKeyRepository.Setup(x => x.GetByIdAsync(_testApiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKey?)null);

        // Act & Assert
        await _service.Invoking(x => x.GetApiKeyByIdAsync(_testApiKeyId))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"API key with ID {_testApiKeyId} not found");
    }

    #endregion

    #region RevokeApiKeyAsync Tests

    [Fact]
    public async Task RevokeApiKeyAsync_WhenKeyExists_ShouldRevokeKey()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = _testApiKeyId,
            Status = Domain.Enums.ApiKeyStatus.Active
        };

        _mockApiKeyRepository.Setup(x => x.GetByIdAsync(_testApiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);

        // Act
        await _service.RevokeApiKeyAsync(_testApiKeyId);

        // Assert
        apiKey.Status.Should().Be(Domain.Enums.ApiKeyStatus.Revoked);
        apiKey.RevokedAt.Should().NotBeNull();
        apiKey.UpdatedAt.Should().NotBeNull();
        _mockApiKeyRepository.Verify(x => x.UpdateAsync(apiKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeApiKeyAsync_WhenKeyNotFound_ShouldThrowException()
    {
        // Arrange
        _mockApiKeyRepository.Setup(x => x.GetByIdAsync(_testApiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKey?)null);

        // Act & Assert
        await _service.Invoking(x => x.RevokeApiKeyAsync(_testApiKeyId))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"API key with ID {_testApiKeyId} not found");
    }

    #endregion

    #region RotateApiKeyAsync Tests

    [Fact]
    public async Task RotateApiKeyAsync_WhenKeyExists_ShouldRotateKey()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = _testApiKeyId,
            KeyPrefix = "tihomo_oldkey",
            KeyHash = "oldhash"
        };

        _mockApiKeyRepository.Setup(x => x.GetByIdAsync(_testApiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);
        _mockApiKeyHasher.Setup(x => x.HashApiKey(It.IsAny<string>()))
            .Returns("newhash");

        // Act
        var result = await _service.RotateApiKeyAsync(_testApiKeyId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testApiKeyId);
        result.NewApiKey.Should().StartWith("tihomo_");
        result.OldKeyPrefix.Should().Be("tihomo_oldkey");
        apiKey.KeyHash.Should().Be("newhash");
        _mockApiKeyRepository.Verify(x => x.UpdateAsync(apiKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}