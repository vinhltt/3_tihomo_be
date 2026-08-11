using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="RecurringTransactionTemplateCreateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="RecurringTransactionTemplateCreateRequest" />. (VI)
/// </summary>
public class
    RecurringTransactionTemplateCreateRequestValidator : AbstractValidator<RecurringTransactionTemplateCreateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringTransactionTemplateCreateRequestValidator" /> class. (EN)
    ///     <br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="RecurringTransactionTemplateCreateRequestValidator" />. (VI)
    /// </summary>
    public RecurringTransactionTemplateCreateRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.TransactionType).IsInEnum();
    }
}