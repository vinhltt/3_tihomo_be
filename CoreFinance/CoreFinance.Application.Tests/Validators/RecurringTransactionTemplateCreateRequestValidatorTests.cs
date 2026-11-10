using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for RecurringTransactionTemplateCreateRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho RecurringTransactionTemplateCreateRequestValidator (VI)
/// </summary>
public class RecurringTransactionTemplateCreateRequestValidatorTests
{
    private readonly RecurringTransactionTemplateCreateRequestValidator _validator = new();

    private static RecurringTransactionTemplateCreateRequest CreateValidRequest() => new()
    {
        AccountId = Guid.NewGuid(),
        Name = "Monthly rent",
        Amount = 5000m,
        TransactionType = RecurringTransactionType.Expense,
        Frequency = RecurrenceFrequency.Monthly,
        StartDate = new DateTime(2026, 10, 9),
        DaysInAdvance = 30
    };

    /// <summary>
    /// A valid request passes (EN)<br />
    /// Request hợp lệ được chấp nhận (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Valid_Request()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Frontend-shaped payload (DaysInAdvance 30, Custom frequency with 5 day interval) passes (EN)<br />
    /// Payload dạng frontend (DaysInAdvance 30, tần suất Custom với chu kỳ 5 ngày) hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Frontend_Shaped_Custom_Frequency_Payload()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DaysInAdvance = 30;
        request.Frequency = RecurrenceFrequency.Custom;
        request.CustomIntervalDays = 5;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// AccountId must not be Guid.Empty (EN)<br />
    /// AccountId không được là Guid.Empty (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Empty_AccountId()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AccountId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    /// <summary>
    /// Name is required and not blank (EN)<br />
    /// Name bắt buộc và không được trống (VI)
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_Should_Reject_Missing_Name(string? name)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = name;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Name follows the 200 character storage limit, not the former 100 (EN)<br />
    /// Name theo giới hạn lưu trữ 200 ký tự, không còn là 100 (VI)
    /// </summary>
    [Theory]
    [InlineData(100, true)]
    [InlineData(101, true)]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void Validator_Should_Enforce_Name_Max_Length_Of_200(int length, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = new string('n', length);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Name);
        else result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Amount must be greater than zero (EN)<br />
    /// Amount phải lớn hơn 0 (VI)
    /// </summary>
    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(0.01, true)]
    public void Validator_Should_Require_Positive_Amount(double amount, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = (decimal)amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        else result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    /// <summary>
    /// StartDate must not be the default date (EN)<br />
    /// StartDate không được là ngày mặc định (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Default_StartDate()
    {
        // Arrange
        var request = CreateValidRequest();
        request.StartDate = default;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    /// <summary>
    /// Frequency accepts every defined value, including Custom (value 0) (EN)<br />
    /// Frequency chấp nhận mọi giá trị đã định nghĩa, kể cả Custom (giá trị 0) (VI)
    /// </summary>
    [Theory]
    [InlineData(RecurrenceFrequency.Custom)]
    [InlineData(RecurrenceFrequency.Daily)]
    [InlineData(RecurrenceFrequency.Monthly)]
    public void Validator_Should_Accept_Defined_Frequency(RecurrenceFrequency frequency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Frequency = frequency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Frequency);
    }

    /// <summary>
    /// Frequency rejects undefined values (EN)<br />
    /// Frequency từ chối giá trị không định nghĩa (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Undefined_Frequency()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Frequency = (RecurrenceFrequency)99;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Frequency);
    }

    /// <summary>
    /// TransactionType rejects undefined values and accepts defined ones (EN)<br />
    /// TransactionType từ chối giá trị không định nghĩa và chấp nhận giá trị đã định nghĩa (VI)
    /// </summary>
    [Theory]
    [InlineData(RecurringTransactionType.Income, true)]
    [InlineData(RecurringTransactionType.Expense, true)]
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
    /// CustomIntervalDays is optional but must be positive when provided (EN)<br />
    /// CustomIntervalDays là tùy chọn nhưng phải dương khi có giá trị (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void Validator_Should_Require_Positive_CustomIntervalDays_When_Provided(int? interval, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.CustomIntervalDays = interval;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.CustomIntervalDays);
        else result.ShouldHaveValidationErrorFor(x => x.CustomIntervalDays);
    }

    /// <summary>
    /// DaysInAdvance must be between 1 and 365 inclusive (EN)<br />
    /// DaysInAdvance phải nằm trong khoảng 1 đến 365 (VI)
    /// </summary>
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(30, true)]
    [InlineData(365, true)]
    [InlineData(366, false)]
    public void Validator_Should_Enforce_DaysInAdvance_Range(int days, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.DaysInAdvance = days;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.DaysInAdvance);
        else result.ShouldHaveValidationErrorFor(x => x.DaysInAdvance);
    }
}
