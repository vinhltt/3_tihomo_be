using CoreFinance.Api.Infrastructures.ServicesExtensions;
using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.DTOs.Transaction;
using FluentValidation;

namespace CoreFinance.Api.Tests;

/// <summary>
/// Registration tests for AddApplicationValidators: every application validator resolves exactly once (EN)<br />
/// Kiểm thử đăng ký cho AddApplicationValidators: mỗi validator của ứng dụng resolve đúng một lần (VI)
/// </summary>
public class ApplicationValidatorRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddApplicationValidators();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Each of the 12 request DTOs resolves exactly one validator (EN)<br />
    /// Mỗi DTO trong 12 DTO request resolve đúng một validator (VI)
    /// </summary>
    [Theory]
    [InlineData(typeof(AccountCreateRequest))]
    [InlineData(typeof(AccountUpdateRequest))]
    [InlineData(typeof(TransactionCreateRequest))]
    [InlineData(typeof(TransactionUpdateRequest))]
    [InlineData(typeof(RecurringTransactionTemplateCreateRequest))]
    [InlineData(typeof(RecurringTransactionTemplateUpdateRequest))]
    [InlineData(typeof(ExpectedTransactionCreateRequest))]
    [InlineData(typeof(ExpectedTransactionUpdateRequest))]
    [InlineData(typeof(ConfirmTransactionRequest))]
    [InlineData(typeof(CancelTransactionRequest))]
    [InlineData(typeof(AdjustTransactionRequest))]
    [InlineData(typeof(RecordPaymentRequest))]
    public void AddApplicationValidators_ShouldRegisterExactlyOneValidator_WhenDtoHasValidator(Type dtoType)
    {
        // Arrange
        using var provider = BuildProvider();
        var collectionType = typeof(IEnumerable<>).MakeGenericType(typeof(IValidator<>).MakeGenericType(dtoType));

        // Act
        var validators = ((System.Collections.IEnumerable)provider.GetRequiredService(collectionType))
            .Cast<object>().ToList();

        // Assert
        validators.Should().HaveCount(1);
    }

    /// <summary>
    /// The single registered validator for each DTO is the one the application assembly defines for it (EN)<br />
    /// Validator duy nhất được đăng ký cho mỗi DTO là validator mà assembly ứng dụng định nghĩa cho DTO đó (VI)
    /// </summary>
    [Theory]
    [InlineData(typeof(AccountCreateRequest), "CreateAccountRequestValidator")]
    [InlineData(typeof(AccountUpdateRequest), "UpdateAccountRequestValidator")]
    [InlineData(typeof(TransactionCreateRequest), "CreateTransactionRequestValidator")]
    [InlineData(typeof(TransactionUpdateRequest), "UpdateTransactionRequestValidator")]
    [InlineData(typeof(RecurringTransactionTemplateCreateRequest), "RecurringTransactionTemplateCreateRequestValidator")]
    [InlineData(typeof(RecurringTransactionTemplateUpdateRequest), "RecurringTransactionTemplateUpdateRequestValidator")]
    [InlineData(typeof(ExpectedTransactionCreateRequest), "ExpectedTransactionCreateRequestValidator")]
    [InlineData(typeof(ExpectedTransactionUpdateRequest), "ExpectedTransactionUpdateRequestValidator")]
    [InlineData(typeof(ConfirmTransactionRequest), "ConfirmTransactionRequestValidator")]
    [InlineData(typeof(CancelTransactionRequest), "CancelTransactionRequestValidator")]
    [InlineData(typeof(AdjustTransactionRequest), "AdjustTransactionRequestValidator")]
    [InlineData(typeof(RecordPaymentRequest), "RecordPaymentRequestValidator")]
    public void AddApplicationValidators_ShouldResolveMatchingValidatorType_WhenDtoHasValidator(
        Type dtoType, string validatorTypeName)
    {
        // Arrange
        using var provider = BuildProvider();
        var validatorType = typeof(IValidator<>).MakeGenericType(dtoType);

        // Act
        var validator = provider.GetRequiredService(validatorType);

        // Assert
        validator.GetType().Name.Should().Be(validatorTypeName);
    }
}
