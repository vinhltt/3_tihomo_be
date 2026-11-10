using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for ConfirmTransactionRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho ConfirmTransactionRequestValidator (VI)
/// </summary>
public class ConfirmTransactionRequestValidatorTests
{
    private readonly ConfirmTransactionRequestValidator _validator = new();

    /// <summary>
    /// A non-empty actual transaction id passes (EN)<br />
    /// ID giao dịch thực khác rỗng được chấp nhận (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Non_Empty_ActualTransactionId()
    {
        // Arrange
        var request = new ConfirmTransactionRequest { ActualTransactionId = Guid.NewGuid() };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// An empty actual transaction id is rejected with the lifecycle error code and message (EN)<br />
    /// ID giao dịch thực rỗng bị từ chối với mã lỗi và message của vòng đời (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Empty_ActualTransactionId_With_Code_And_Message()
    {
        // Arrange
        var request = new ConfirmTransactionRequest { ActualTransactionId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ActualTransactionId)
            .WithErrorCode(CoreFinanceErrorCodes.InvalidActualTransaction)
            .WithErrorMessage(CoreFinanceMessages.ActualTransactionIdRequired);
        result.Errors.Should().ContainSingle();
    }
}
