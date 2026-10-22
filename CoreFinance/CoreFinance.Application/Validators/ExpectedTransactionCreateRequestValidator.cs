using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Enums;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="ExpectedTransactionCreateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="ExpectedTransactionCreateRequest" />. (VI)
/// </summary>
public class ExpectedTransactionCreateRequestValidator : AbstractValidator<ExpectedTransactionCreateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpectedTransactionCreateRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="ExpectedTransactionCreateRequestValidator" />. (VI)
    /// </summary>
    public ExpectedTransactionCreateRequestValidator()
    {
        RuleFor(x => x.RecurringTransactionTemplateId)
            .Must(id => id != Guid.Empty).WithMessage(CoreFinanceMessages.TemplateIdMustNotBeEmpty);
        RuleFor(x => x.AccountId)
            .Must(id => id != Guid.Empty).WithMessage(CoreFinanceMessages.AccountIdMustNotBeEmpty);
        RuleFor(x => x.ExpectedDate).NotEmpty();
        RuleFor(x => x.ExpectedAmount).GreaterThan(0);
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.Status).Equal(ExpectedTransactionStatus.Pending)
            .WithMessage(CoreFinanceMessages.NewExpectedTransactionsMustBePending);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}