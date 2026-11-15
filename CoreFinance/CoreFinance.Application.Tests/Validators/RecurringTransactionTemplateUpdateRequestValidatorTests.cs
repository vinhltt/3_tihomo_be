using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for RecurringTransactionTemplateUpdateRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho RecurringTransactionTemplateUpdateRequestValidator (VI)
/// </summary>
public class RecurringTransactionTemplateUpdateRequestValidatorTests
{
    private readonly RecurringTransactionTemplateUpdateRequestValidator _validator = new();

    private static RecurringTransactionTemplateUpdateRequest CreateValidRequest() => new()
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
    /// Frontend-shaped payload (DaysInAdvance 30, Custom frequency with 5 day interval) passes (EN)<br />
    /// Payload dạng frontend (DaysInAdvance 30, tần suất Custom với chu kỳ 5 ngày) hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Frontend_Shaped_Custom_Frequency_Payload()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = "Monthly rent";
        request.Amount = 5000m;
        request.StartDate = new DateTime(2026, 10, 9);
        request.Frequency = RecurrenceFrequency.Custom;
        request.TransactionType = RecurringTransactionType.Expense;
        request.CustomIntervalDays = 5;
        request.DaysInAdvance = 30;

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
    /// Name may be omitted but must not be blank when provided (EN)<br />
    /// Name có thể bỏ qua nhưng không được trống khi có giá trị (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData("Rent", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void Validator_Should_Reject_Blank_Name_When_Provided(string? name, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = name;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Name);
        else result.ShouldHaveValidationErrorFor(x => x.Name);
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
    /// StartDate may be omitted; the minimum date is rejected when provided (EN)<br />
    /// StartDate có thể bỏ qua; ngày nhỏ nhất bị từ chối khi có giá trị (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Default_StartDate_When_Provided()
    {
        // Arrange
        var request = CreateValidRequest();
        request.StartDate = DateTime.MinValue;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    /// <summary>
    /// A real StartDate or an omitted one passes (EN)<br />
    /// StartDate hợp lệ hoặc bỏ qua đều được chấp nhận (VI)
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Validator_Should_Accept_Omitted_Or_Real_StartDate(bool provided)
    {
        // Arrange
        var request = CreateValidRequest();
        request.StartDate = provided ? new DateTime(2026, 10, 9) : null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    /// <summary>
    /// Amount must be greater than zero when provided (EN)<br />
    /// Amount phải lớn hơn 0 khi có giá trị (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(0.01, true)]
    [InlineData(0d, false)]
    [InlineData(-1d, false)]
    public void Validator_Should_Require_Positive_Amount_When_Provided(double? amount, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = amount.HasValue ? (decimal)amount.Value : null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        else result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    /// <summary>
    /// Frequency may be omitted; Custom (value 0) is valid and undefined values are rejected (EN)<br />
    /// Frequency có thể bỏ qua; Custom (giá trị 0) hợp lệ và giá trị không định nghĩa bị từ chối (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(RecurrenceFrequency.Custom, true)]
    [InlineData(RecurrenceFrequency.Monthly, true)]
    [InlineData((RecurrenceFrequency)99, false)]
    public void Validator_Should_Enforce_Defined_Frequency_When_Provided(RecurrenceFrequency? frequency, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Frequency = frequency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Frequency);
        else result.ShouldHaveValidationErrorFor(x => x.Frequency);
    }

    /// <summary>
    /// TransactionType may be omitted; undefined values are rejected (EN)<br />
    /// TransactionType có thể bỏ qua; giá trị không định nghĩa bị từ chối (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(RecurringTransactionType.Income, true)]
    [InlineData((RecurringTransactionType)99, false)]
    public void Validator_Should_Enforce_Defined_TransactionType_When_Provided(RecurringTransactionType? type,
        bool valid)
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
    /// CustomIntervalDays must be positive when provided (EN)<br />
    /// CustomIntervalDays phải dương khi có giá trị (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(5, true)]
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
    /// DaysInAdvance must be between 1 and 365 inclusive when provided (EN)<br />
    /// DaysInAdvance phải nằm trong khoảng 1 đến 365 khi có giá trị (VI)
    /// </summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(30, true)]
    [InlineData(365, true)]
    [InlineData(366, false)]
    public void Validator_Should_Enforce_DaysInAdvance_Range_When_Provided(int? days, bool valid)
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
