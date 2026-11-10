using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for CancelTransactionRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho CancelTransactionRequestValidator (VI)
/// </summary>
public class CancelTransactionRequestValidatorTests
{
    private readonly CancelTransactionRequestValidator _validator = new();

    /// <summary>
    /// A reason of 1 to 500 characters passes (EN)<br />
    /// Lý do từ 1 đến 500 ký tự được chấp nhận (VI)
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(500)]
    public void Validator_Should_Accept_Reason_Within_Length_Limit(int length)
    {
        // Arrange
        var request = new CancelTransactionRequest { Reason = new string('r', length) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// A missing, empty or whitespace-only reason is rejected with the lifecycle code and message (EN)<br />
    /// Lý do thiếu, rỗng hoặc toàn khoảng trắng bị từ chối với mã lỗi và message của vòng đời (VI)
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" \t ")]
    public void Validator_Should_Reject_Blank_Reason_With_Code_And_Message(string? reason)
    {
        // Arrange
        var request = new CancelTransactionRequest { Reason = reason! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorCode(CoreFinanceErrorCodes.InvalidReason)
            .WithErrorMessage(CoreFinanceMessages.ReasonRequired);
        result.Errors[0].ErrorCode.Should().Be(CoreFinanceErrorCodes.InvalidReason);
        result.Errors[0].ErrorMessage.Should().Be(CoreFinanceMessages.ReasonRequired);
    }

    /// <summary>
    /// A reason of 501 characters is rejected with the same lifecycle code and message (EN)<br />
    /// Lý do 501 ký tự bị từ chối với cùng mã lỗi và message của vòng đời (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Reason_Over_500_Characters_With_Code_And_Message()
    {
        // Arrange
        var request = new CancelTransactionRequest { Reason = new string('r', 501) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorCode(CoreFinanceErrorCodes.InvalidReason)
            .WithErrorMessage(CoreFinanceMessages.ReasonRequired);
        result.Errors[0].ErrorCode.Should().Be(CoreFinanceErrorCodes.InvalidReason);
        result.Errors[0].ErrorMessage.Should().Be(CoreFinanceMessages.ReasonRequired);
    }
}
