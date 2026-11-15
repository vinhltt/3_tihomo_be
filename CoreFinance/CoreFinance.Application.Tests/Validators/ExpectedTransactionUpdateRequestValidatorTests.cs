using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for ExpectedTransactionUpdateRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho ExpectedTransactionUpdateRequestValidator (VI)
/// </summary>
public class ExpectedTransactionUpdateRequestValidatorTests
{
    private readonly ExpectedTransactionUpdateRequestValidator _validator = new();

    private static ExpectedTransactionUpdateRequest CreateValidRequest() => new()
    {
        Id = Guid.NewGuid()
    };

    /// <summary>
    /// A request carrying only the Id passes: every other field is optional (EN)<br />
    /// Request chỉ có Id hợp lệ: mọi trường khác là tùy chọn (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Request_With_Only_Id()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// A fully populated valid request passes (EN)<br />
    /// Request đầy đủ và hợp lệ được chấp nhận (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Fully_Populated_Request()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpectedDate = new DateTime(2026, 10, 9);
        request.ExpectedAmount = 120m;
        request.Status = ExpectedTransactionStatus.Pending;
        request.Description = "Electricity";
        request.Category = "Utilities";
        request.AdjustmentReason = "New reading";
        request.Notes = "Keep the receipt";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Id must not be Guid.Empty (EN)<br />
    /// Id không được là Guid.Empty (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Empty_Id()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Id = Guid.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// ExpectedDate is accepted as provided, including the minimum date (EN)<br />
    /// ExpectedDate được chấp nhận như đã gửi, kể cả ngày nhỏ nhất (VI)
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Validator_Should_Accept_Omitted_Or_Minimum_ExpectedDate(bool minimum)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpectedDate = minimum ? DateTime.MinValue : null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpectedDate);
    }

    /// <summary>
    /// ExpectedAmount must be greater than zero when provided (EN)<br />
    /// ExpectedAmount phải lớn hơn 0 khi có giá trị (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(0.01, true)]
    [InlineData(0d, false)]
    [InlineData(-1d, false)]
    public void Validator_Should_Require_Positive_ExpectedAmount_When_Provided(double? amount, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpectedAmount = amount.HasValue ? (decimal)amount.Value : null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.ExpectedAmount);
        else result.ShouldHaveValidationErrorFor(x => x.ExpectedAmount);
    }

    /// <summary>
    /// Status may be omitted; undefined values are rejected (EN)<br />
    /// Status có thể bỏ qua; giá trị không định nghĩa bị từ chối (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(ExpectedTransactionStatus.Pending, true)]
    [InlineData((ExpectedTransactionStatus)99, false)]
    public void Validator_Should_Enforce_Defined_Status_When_Provided(ExpectedTransactionStatus? status, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Status);
        else result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    /// <summary>
    /// Description, Category, AdjustmentReason and Notes enforce 500, 100, 500 and 1000 character limits (EN)<br />
    /// Description, Category, AdjustmentReason và Notes giới hạn 500, 100, 500 và 1000 ký tự (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Text_Fields_At_Their_Limits()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = new string('d', 500);
        request.Category = new string('c', 100);
        request.AdjustmentReason = new string('r', 500);
        request.Notes = new string('n', 1000);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Each text field rejects one character over its limit (EN)<br />
    /// Mỗi trường văn bản từ chối vượt giới hạn một ký tự (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Text_Fields_Over_Their_Limits()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = new string('d', 501);
        request.Category = new string('c', 101);
        request.AdjustmentReason = new string('r', 501);
        request.Notes = new string('n', 1001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Category);
        result.ShouldHaveValidationErrorFor(x => x.AdjustmentReason);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
