using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CoreFinance.Application.Tests.Entities;

/// <summary>
/// Unit tests for Account entity Code property validation (EN)<br/>
/// Kiểm thử đơn vị cho validation thuộc tính Code của entity Account (VI)
/// </summary>
public class AccountEntityTests
{
    [Theory]
    [InlineData("techcombank_debit", true)]   // Valid snake_case
    [InlineData("bidv_checking_001", true)]   // Valid with numbers
    [InlineData("mb_bank_savings", true)]     // Valid multiple underscores
    [InlineData("Techcombank_Debit", false)]  // Invalid - uppercase
    [InlineData("techcombank-debit", false)]  // Invalid - hyphen
    [InlineData("techcombank debit", false)]  // Invalid - space
    [InlineData("techcombank@debit", false)]  // Invalid - special char
    [InlineData("", true)]                    // Valid - empty string treated as null (backward compat)
    [InlineData(null, true)]                  // Valid - nullable
    public void Code_Validation_Should_Match_SnakeCase_Pattern(string? code, bool isValid)
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Test Account",
            Code = code,
            Type = AccountType.DebitCard,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 0,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(account);
        var result = Validator.TryValidateObject(account, context, validationResults, true);

        // Assert
        result.Should().Be(isValid, $"Code '{code}' should be {(isValid ? "valid" : "invalid")}");
        if (!isValid && code != null && code != "")
        {
            validationResults.Should().Contain(v => v.MemberNames.Contains("Code"),
                "validation should fail on Code property");
        }
    }

    [Fact]
    public void Code_Length_Should_Not_Exceed_50_Characters()
    {
        // Arrange
        var longCode = new string('a', 51); // 51 characters
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Test Account",
            Code = longCode,
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 0,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(account);
        var result = Validator.TryValidateObject(account, context, validationResults, true);

        // Assert
        result.Should().BeFalse("Code with 51 characters should be invalid");
        validationResults.Should().Contain(v => v.MemberNames.Contains("Code"),
            "validation should fail on Code property for exceeding max length");
    }

    [Fact]
    public void Code_Should_Be_Nullable()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Test Account Without Code",
            Code = null, // Explicitly null
            Type = AccountType.Wallet,
            Currency = "USD",
            InitialBalance = 100,
            CurrentBalance = 100,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(account);
        var result = Validator.TryValidateObject(account, context, validationResults, true);

        // Assert
        result.Should().BeTrue("Code should be nullable for backward compatibility");
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("a")]           // 1 char
    [InlineData("ab")]          // 2 chars
    [InlineData("test_code_123_very_long_but_exactly_fifty_char")]  // 50 chars exactly
    public void Code_Valid_Lengths_Should_Pass(string code)
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            Name = "Test",
            Code = code,
            Type = AccountType.Cash,
            Currency = "VND",
            InitialBalance = 0,
            CurrentBalance = 0,
            IsActive = true,
            UserId = Guid.CreateVersion7()
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(account);
        var result = Validator.TryValidateObject(account, context, validationResults, true);

        // Assert
        result.Should().BeTrue($"Code '{code}' with length {code.Length} should be valid");
        validationResults.Should().BeEmpty();
    }
}
