using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for AdjustTransactionRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho AdjustTransactionRequestValidator (VI)
/// </summary>
public class AdjustTransactionRequestValidatorTests
{
    private readonly AdjustTransactionRequestValidator _validator = new();

    private static AdjustTransactionRequest CreateValidRequest() => new()
    {
        NewAmount = 120m,
        Reason = "Reading correction"
    };

    /// <summary>
    /// A positive amount with a reason of 1 to 500 characters passes (EN)<br />
    /// Số tiền dương kèm lý do từ 1 đến 500 ký tự được chấp nhận (VI)
    /// </summary>
    [Theory]
    [InlineData(0.01, 1)]
    [InlineData(120, 20)]
    [InlineData(1000000, 500)]
    public void Validator_Should_Accept_Positive_Amount_And_Reason_Within_Limit(double amount, int reasonLength)
    {
        // Arrange
        var request = new AdjustTransactionRequest
        {
            NewAmount = (decimal)amount,
            Reason = new string('r', reasonLength)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// A zero or negative amount is rejected with the lifecycle code and message (EN)<br />
    /// Số tiền bằng 0 hoặc âm bị từ chối với mã lỗi và message của vòng đời (VI)
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Validator_Should_Reject_Non_Positive_Amount_With_Code_And_Message(double amount)
    {
        // Arrange
        var request = CreateValidRequest();
        request.NewAmount = (decimal)amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewAmount)
            .WithErrorCode(CoreFinanceErrorCodes.InvalidAmount)
            .WithErrorMessage(CoreFinanceMessages.ExpectedAmountMustBePositive);
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    /// <summary>
    /// A missing or blank reason is rejected with the lifecycle code and message (EN)<br />
    /// Lý do thiếu hoặc trống bị từ chối với mã lỗi và message của vòng đời (VI)
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" \t ")]
    public void Validator_Should_Reject_Blank_Reason_With_Code_And_Message(string? reason)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Reason = reason!;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorCode(CoreFinanceErrorCodes.InvalidReason)
            .WithErrorMessage(CoreFinanceMessages.ReasonRequired);
        result.ShouldNotHaveValidationErrorFor(x => x.NewAmount);
    }

    /// <summary>
    /// A reason of 501 characters is rejected with the same lifecycle code and message (EN)<br />
    /// Lý do 501 ký tự bị từ chối với cùng mã lỗi và message của vòng đời (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Reason_Over_500_Characters_With_Code_And_Message()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Reason = new string('r', 501);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorCode(CoreFinanceErrorCodes.InvalidReason)
            .WithErrorMessage(CoreFinanceMessages.ReasonRequired);
    }

    /// <summary>
    /// When both amount and reason are invalid the amount error comes first, as in the service (EN)<br />
    /// Khi cả số tiền và lý do đều sai, lỗi số tiền đứng đầu như trong service (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Report_Amount_Error_First_When_Amount_And_Reason_Are_Invalid()
    {
        // Arrange
        var request = new AdjustTransactionRequest { NewAmount = 0, Reason = "   " };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.Errors.Should().HaveCount(2);
        result.Errors[0].PropertyName.Should().Be(nameof(AdjustTransactionRequest.NewAmount));
        result.Errors[0].ErrorCode.Should().Be(CoreFinanceErrorCodes.InvalidAmount);
        result.Errors[0].ErrorMessage.Should().Be(CoreFinanceMessages.ExpectedAmountMustBePositive);
        result.Errors[1].PropertyName.Should().Be(nameof(AdjustTransactionRequest.Reason));
        result.Errors[1].ErrorCode.Should().Be(CoreFinanceErrorCodes.InvalidReason);
        result.Errors[1].ErrorMessage.Should().Be(CoreFinanceMessages.ReasonRequired);
    }
}
