using CoreFinance.Application.DTOs.ExpectedTransaction;
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
        RuleFor(x => x.RecurringTransactionTemplateId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.ExpectedDate).NotEmpty();
        RuleFor(x => x.ExpectedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}