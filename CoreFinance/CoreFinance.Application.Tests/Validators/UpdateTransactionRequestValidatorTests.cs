using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for UpdateTransactionRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho UpdateTransactionRequestValidator (VI)
/// </summary>
public class UpdateTransactionRequestValidatorTests
{
    private readonly UpdateTransactionRequestValidator _validator = new();

    private static TransactionUpdateRequest CreateValidRequest() => new()
    {
        Id = Guid.NewGuid(),
        AccountId = Guid.NewGuid(),
        TransactionDate = new DateTime(2026, 10, 9),
        RevenueAmount = 0,
        SpentAmount = 150m,
        CategoryType = CategoryType.Expense,
        Balance = 0
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
    /// A negative running Balance is a valid frontend payload (EN)<br />
    /// Balance âm là payload frontend hợp lệ (VI)
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-100.5)]
    [InlineData(0)]
    public void Validator_Should_Accept_Any_Balance_Including_Negative(double balance)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Balance = (decimal)balance;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Balance);
        result.IsValid.Should().BeTrue();
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
    /// TransactionDate must not be the default date (EN)<br />
    /// TransactionDate không được là ngày mặc định (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Default_TransactionDate()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TransactionDate = default;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TransactionDate);
    }

    /// <summary>
    /// RevenueAmount accepts zero and positive values, rejects negatives (EN)<br />
    /// RevenueAmount chấp nhận 0 và số dương, từ chối số âm (VI)
    /// </summary>
    [Theory]
    [InlineData(0, true)]
    [InlineData(0.01, true)]
    [InlineData(-0.01, false)]
    public void Validator_Should_Enforce_Non_Negative_RevenueAmount(double amount, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.RevenueAmount = (decimal)amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.RevenueAmount);
        else result.ShouldHaveValidationErrorFor(x => x.RevenueAmount);
    }

    /// <summary>
    /// SpentAmount accepts zero and positive values, rejects negatives (EN)<br />
    /// SpentAmount chấp nhận 0 và số dương, từ chối số âm (VI)
    /// </summary>
    [Theory]
    [InlineData(0, true)]
    [InlineData(0.01, true)]
    [InlineData(-0.01, false)]
    public void Validator_Should_Enforce_Non_Negative_SpentAmount(double amount, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.SpentAmount = (decimal)amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.SpentAmount);
        else result.ShouldHaveValidationErrorFor(x => x.SpentAmount);
    }

    public static IEnumerable<object[]> DefinedCategoryTypes =>
        Enum.GetValues<CategoryType>().Select(categoryType => new object[] { categoryType });

    /// <summary>
    /// CategoryType accepts every defined value, including the zero value (EN)<br />
    /// CategoryType chấp nhận mọi giá trị đã định nghĩa, kể cả giá trị 0 (VI)
    /// </summary>
    [Theory]
    [MemberData(nameof(DefinedCategoryTypes))]
    public void Validator_Should_Accept_Defined_CategoryType(CategoryType categoryType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.CategoryType = categoryType;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryType);
    }

    /// <summary>
    /// CategoryType rejects undefined values (EN)<br />
    /// CategoryType từ chối giá trị không định nghĩa (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Undefined_CategoryType()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CategoryType = (CategoryType)99;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryType);
    }
}
