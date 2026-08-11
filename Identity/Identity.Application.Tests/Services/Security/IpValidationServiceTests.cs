using FluentAssertions;
using Identity.Application.Services.Security;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Application.Tests.Services.Security;

/// <summary>
/// Unit tests for IpValidationService - Kiểm thử đơn vị cho IpValidationService (EN)<br/>
/// Kiểm thử đơn vị cho IpValidationService (VI)
/// </summary>
public class IpValidationServiceTests
{
    private readonly Mock<ILogger<IpValidationService>> _mockLogger;
    private readonly IpValidationService _ipValidationService;

    public IpValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<IpValidationService>>();
        _ipValidationService = new IpValidationService(_mockLogger.Object);
    }

    #region IsIpAllowed Tests

    [Fact]
    public void IsIpAllowed_WhenWhitelistIsEmpty_ShouldReturnTrue()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string>();

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WhenWhitelistIsNull_ShouldReturnTrue()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        List<string>? ipWhitelist = null;

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WhenClientIpIsNull_ShouldReturnFalse()
    {
        // Arrange
        string? clientIp = null;
        var ipWhitelist = new List<string> { "192.168.1.100" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WhenClientIpIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var clientIp = "";
        var ipWhitelist = new List<string> { "192.168.1.100" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WhenExactIpMatch_ShouldReturnTrue()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string> { "192.168.1.100", "10.0.0.1" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WhenNoIpMatch_ShouldReturnFalse()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string> { "192.168.1.101", "10.0.0.1" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WhenCidrMatch_ShouldReturnTrue()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string> { "192.168.1.0/24" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WhenCidrNoMatch_ShouldReturnFalse()
    {
        // Arrange
        var clientIp = "192.168.2.100";
        var ipWhitelist = new List<string> { "192.168.1.0/24" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WhenWildcardMatch_ShouldReturnTrue()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string> { "192.168.1.*" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WhenWildcardNoMatch_ShouldReturnFalse()
    {
        // Arrange
        var clientIp = "192.168.2.100";
        var ipWhitelist = new List<string> { "192.168.1.*" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WhenInvalidClientIp_ShouldReturnFalse()
    {
        // Arrange
        var clientIp = "invalid-ip";
        var ipWhitelist = new List<string> { "192.168.1.100" };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsValidIpAddress Tests

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", true)]
    [InlineData("::1", true)]
    [InlineData("::ffff:192.168.1.1", true)]
    [InlineData("256.1.1.1", false)]
    [InlineData("192.168.1", false)]
    [InlineData("192.168.1.1.1", false)]
    [InlineData("invalid-ip", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidIpAddress_ShouldReturnExpectedResult(string? ipAddress, bool expected)
    {
        // Act
        var result = _ipValidationService.IsValidIpAddress(ipAddress);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region IsValidCidrRange Tests

    [Theory]
    [InlineData("192.168.1.0/24", true)]
    [InlineData("10.0.0.0/8", true)]
    [InlineData("172.16.0.0/12", true)]
    [InlineData("192.168.1.0/32", true)]
    [InlineData("0.0.0.0/0", true)]
    [InlineData("2001:db8::/32", true)]
    [InlineData("::1/128", true)]
    [InlineData("192.168.1.0/33", false)] // Invalid prefix for IPv4
    [InlineData("192.168.1.0/-1", false)] // Negative prefix
    [InlineData("192.168.1.0/", false)] // Missing prefix
    [InlineData("192.168.1.0", false)] // No slash
    [InlineData("invalid-ip/24", false)] // Invalid IP
    [InlineData("192.168.1.0/abc", false)] // Invalid prefix
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidCidrRange_ShouldReturnExpectedResult(string? cidrRange, bool expected)
    {
        // Act
        var result = _ipValidationService.IsValidCidrRange(cidrRange);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ValidateIpWhitelist Tests

    [Fact]
    public void ValidateIpWhitelist_WhenEmptyList_ShouldReturnValid()
    {
        // Arrange
        var ipWhitelist = new List<string>();

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeTrue();
        Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenNullList_ShouldReturnValid()
    {
        // Arrange
        List<string>? ipWhitelist = null;

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeTrue();
        Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenValidIpAddresses_ShouldReturnValid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.1",
            "10.0.0.1",
            "127.0.0.1",
            "2001:0db8:85a3:0000:0000:8a2e:0370:7334"
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeTrue();
        Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenValidCidrRanges_ShouldReturnValid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.0/24",
            "10.0.0.0/8",
            "172.16.0.0/12",
            "2001:db8::/32"
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeTrue();
        Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenValidWildcardPatterns_ShouldReturnValid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.*",
            "10.0.*.*",
            "172.16.100.*"
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeTrue();
        Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenMixedValidEntries_ShouldReturnValid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.100",      // Valid IP
            "10.0.0.0/8",         // Valid CIDR
            "172.16.*.*"          // Valid wildcard
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeTrue();
        Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenInvalidIpAddresses_ShouldReturnInvalid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "256.1.1.1",         // Invalid IP
            "192.168.1",         // Incomplete IP
            "invalid-ip"         // Invalid format
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeFalse();
        Errors.Should().HaveCount(3);
        Errors.Should().Contain(error => error.Contains("256.1.1.1"));
        Errors.Should().Contain(error => error.Contains("192.168.1"));
        Errors.Should().Contain(error => error.Contains("invalid-ip"));
    }

    [Fact]
    public void ValidateIpWhitelist_WhenInvalidCidrRanges_ShouldReturnInvalid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.0/33",    // Invalid prefix for IPv4
            "invalid-ip/24",     // Invalid IP in CIDR
            "192.168.1.0/abc"    // Invalid prefix format
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeFalse();
        Errors.Should().HaveCount(3);
    }

    [Fact]
    public void ValidateIpWhitelist_WhenInvalidWildcardPatterns_ShouldReturnInvalid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.*.*.*",   // Too many wildcards
            "256.*.1.1",         // Invalid number with wildcard
            "192.168.1.256"      // Invalid last octet
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeFalse();
        Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateIpWhitelist_WhenEmptyOrWhitespaceEntries_ShouldReturnInvalid()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "",                  // Empty string
            "   ",               // Whitespace only
            "192.168.1.1"        // Valid IP
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeFalse();
        Errors.Should().Contain(error => error.Contains("Empty or whitespace"));
    }

    [Fact]
    public void ValidateIpWhitelist_WhenMixedValidAndInvalidEntries_ShouldReturnInvalidWithSpecificErrors()
    {
        // Arrange
        var ipWhitelist = new List<string>
        {
            "192.168.1.1",       // Valid IP
            "10.0.0.0/8",        // Valid CIDR
            "256.1.1.1",         // Invalid IP
            "192.168.1.0/33",    // Invalid CIDR
            "172.16.*.*"         // Valid wildcard
        };

        // Act
        var (IsValid, Errors) = _ipValidationService.ValidateIpWhitelist(ipWhitelist);

        // Assert
        IsValid.Should().BeFalse();
        Errors.Should().HaveCount(2);
        Errors.Should().Contain(error => error.Contains("256.1.1.1"));
        Errors.Should().Contain(error => error.Contains("192.168.1.0/33"));
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [Theory]
    [InlineData("192.168.1.100", "192.168.1.0/24", true)]
    [InlineData("192.168.1.255", "192.168.1.0/24", true)]
    [InlineData("192.168.1.0", "192.168.1.0/24", true)]
    [InlineData("192.168.2.1", "192.168.1.0/24", false)]
    [InlineData("10.0.0.1", "10.0.0.0/8", true)]
    [InlineData("11.0.0.1", "10.0.0.0/8", false)]
    [InlineData("172.16.100.50", "172.16.0.0/12", true)]
    [InlineData("172.32.0.1", "172.16.0.0/12", false)]
    public void IsIpAllowed_CidrRangeEdgeCases_ShouldReturnExpectedResult(
        string clientIp, string cidrRange, bool expected)
    {
        // Arrange
        var ipWhitelist = new List<string> { cidrRange };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("192.168.1.100", "192.168.1.*", true)]
    [InlineData("192.168.1.255", "192.168.1.*", true)]
    [InlineData("192.168.1.0", "192.168.1.*", true)]
    [InlineData("192.168.2.1", "192.168.1.*", false)]
    [InlineData("10.20.30.40", "10.*.*.*", true)]
    [InlineData("11.20.30.40", "10.*.*.*", false)]
    public void IsIpAllowed_WildcardPatternEdgeCases_ShouldReturnExpectedResult(
        string clientIp, string wildcardPattern, bool expected)
    {
        // Arrange
        var ipWhitelist = new List<string> { wildcardPattern };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsIpAllowed_WithMultipleWhitelistEntries_ShouldReturnTrueIfAnyMatch()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string>
        {
            "10.0.0.0/8",        // No match
            "172.16.0.0/12",     // No match
            "192.168.1.0/24",    // Match!
            "203.0.113.0/24"     // No match
        };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WithWhitespaceInEntries_ShouldHandleCorrectly()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var ipWhitelist = new List<string>
        {
            "  192.168.1.100  ",  // Should be trimmed and match
            "",                   // Empty entry should be skipped
            "   "                 // Whitespace only should be skipped
        };

        // Act
        var result = _ipValidationService.IsIpAllowed(clientIp, ipWhitelist);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
} 