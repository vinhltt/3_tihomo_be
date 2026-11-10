using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for ExpectedTransactionCreateRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho ExpectedTransactionCreateRequestValidator (VI)
/// </summary>
public class ExpectedTransactionCreateRequestValidatorTests
{
    private readonly ExpectedTransactionCreateRequestValidator _validator = new();

    private static ExpectedTransactionCreateRequest CreateValidRequest() => new()
    {
        RecurringTransactionTemplateId = Guid.NewGuid(),
        AccountId = Guid.NewGuid(),
        ExpectedDate = new DateTime(2026, 10, 9),
        ExpectedAmount = 100m,
        TransactionType = RecurringTransactionType.Expense,
        Status = ExpectedTransactionStatus.Pending
    };

    /// <summary>
    /// A valid request passes, and both references may be omitted (EN)<br />
    /// Request hợp lệ được chấp nhận, và cả hai tham chiếu có thể bỏ qua (VI)
    /// </summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validator_Should_Accept_Valid_Request_With_Optional_References(bool template, bool account)
    {
        // Arrange
        var request = CreateValidRequest();
        if (!template) request.RecurringTransactionTemplateId = null;
        if (!account) request.AccountId = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// An explicit empty template id is rejected with the exact message (EN)<br />
    /// Template id rỗng được truyền rõ ràng bị từ chối với đúng message (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Empty_TemplateId_With_Exact_Message()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RecurringTransactionTemplateId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecurringTransactionTemplateId)
            .WithErrorMessage(CoreFinanceMessages.TemplateIdMustNotBeEmpty);
    }

    /// <summary>
    /// An explicit empty account id is rejected with the exact message (EN)<br />
    /// Account id rỗng được truyền rõ ràng bị từ chối với đúng message (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Empty_AccountId_With_Exact_Message()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AccountId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorMessage(CoreFinanceMessages.AccountIdMustNotBeEmpty);
    }

    /// <summary>
    /// ExpectedDate must not be the default date (EN)<br />
    /// ExpectedDate không được là ngày mặc định (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Default_ExpectedDate()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpectedDate = default;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpectedDate);
    }

    /// <summary>
    /// ExpectedAmount must be greater than zero (EN)<br />
    /// ExpectedAmount phải lớn hơn 0 (VI)
    /// </summary>
    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(0.01, true)]
    public void Validator_Should_Require_Positive_ExpectedAmount(double amount, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpectedAmount = (decimal)amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.ExpectedAmount);
        else result.ShouldHaveValidationErrorFor(x => x.ExpectedAmount);
    }

    /// <summary>
    /// TransactionType must be a defined value (EN)<br />
    /// TransactionType phải là giá trị đã định nghĩa (VI)
    /// </summary>
    [Theory]
    [InlineData(RecurringTransactionType.Income, true)]
    [InlineData(RecurringTransactionType.Transfer, true)]
    [InlineData((RecurringTransactionType)99, false)]
    public void Validator_Should_Enforce_Defined_TransactionType(RecurringTransactionType type, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.TransactionType = type;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.TransactionType);
        else result.ShouldHaveValidationErrorFor(x => x.TransactionType);
    }

    /// <summary>
    /// New periods must be Pending, with the exact message (EN)<br />
    /// Kỳ mới phải ở trạng thái Pending, với đúng message (VI)
    /// </summary>
    [Theory]
    [InlineData(ExpectedTransactionStatus.Confirmed)]
    [InlineData(ExpectedTransactionStatus.Cancelled)]
    [InlineData(ExpectedTransactionStatus.Completed)]
    public void Validator_Should_Reject_Non_Pending_Status_With_Exact_Message(ExpectedTransactionStatus status)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage(CoreFinanceMessages.NewExpectedTransactionsMustBePending);
    }

    /// <summary>
    /// Description, Category and Notes enforce 500, 100 and 1000 character limits (EN)<br />
    /// Description, Category và Notes giới hạn 500, 100 và 1000 ký tự (VI)
    /// </summary>
    [Theory]
    [InlineData(500, 100, 1000, true)]
    [InlineData(501, 100, 1000, false)]
    [InlineData(500, 101, 1000, false)]
    [InlineData(500, 100, 1001, false)]
    public void Validator_Should_Enforce_Text_Length_Limits(int description, int category, int notes, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = new string('d', description);
        request.Category = new string('c', category);
        request.Notes = new string('n', notes);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().Be(valid);
        if (description > 500) result.ShouldHaveValidationErrorFor(x => x.Description);
        if (category > 100) result.ShouldHaveValidationErrorFor(x => x.Category);
        if (notes > 1000) result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
