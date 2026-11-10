using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for RecordPaymentRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho RecordPaymentRequestValidator (VI)
/// </summary>
public class RecordPaymentRequestValidatorTests
{
    private readonly RecordPaymentRequestValidator _validator = new();

    /// <summary>
    /// A null or non-empty account id passes (EN)<br />
    /// AccountId null hoặc khác rỗng được chấp nhận (VI)
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Validator_Should_Accept_Null_Or_Non_Empty_AccountId(bool provided)
    {
        // Arrange
        var request = new RecordPaymentRequest { AccountId = provided ? Guid.NewGuid() : null };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// An explicit empty account id is rejected with the exact message (EN)<br />
    /// AccountId rỗng được truyền rõ ràng bị từ chối với đúng message (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Empty_AccountId_With_Exact_Message()
    {
        // Arrange
        var request = new RecordPaymentRequest { AccountId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorMessage(CoreFinanceMessages.AccountIdMustNotBeEmpty);
    }
}
