using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="RecurringTransactionTemplateUpdateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="RecurringTransactionTemplateUpdateRequest" />. (VI)
/// </summary>
public class
    RecurringTransactionTemplateUpdateRequestValidator : AbstractValidator<RecurringTransactionTemplateUpdateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringTransactionTemplateUpdateRequestValidator" /> class. (EN)
    ///     <br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="RecurringTransactionTemplateUpdateRequestValidator" />. (VI)
    /// </summary>
    public RecurringTransactionTemplateUpdateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().When(x => x.Name != null).MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0).When(x => x.Amount.HasValue);
        RuleFor(x => x.StartDate).NotEmpty().When(x => x.StartDate.HasValue);
        RuleFor(x => x.Frequency).IsInEnum().When(x => x.Frequency.HasValue);
        RuleFor(x => x.TransactionType).IsInEnum().When(x => x.TransactionType.HasValue);
    }
}