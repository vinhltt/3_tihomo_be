using CoreFinance.Application.Validators;
using FluentValidation;

namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     Provides extension methods for registering application validators. (EN)<br />
///     Cung cấp các extension methods để đăng ký các validator của ứng dụng. (VI)
/// </summary>
public static class ApplicationValidatorExtensions
{
    /// <summary>
    ///     Registers all validators from the application assembly. (EN)<br />
    ///     Đăng ký tất cả các validator từ assembly của ứng dụng. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection instance. (EN)<br />
    ///     Instance của IServiceCollection. (VI)
    /// </param>
    public static void AddApplicationValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateAccountRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateAccountRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateTransactionRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTransactionRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RecurringTransactionTemplateCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RecurringTransactionTemplateUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ExpectedTransactionCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ExpectedTransactionUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ConfirmTransactionRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<AdjustTransactionRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CancelTransactionRequestValidator>();
    }
}