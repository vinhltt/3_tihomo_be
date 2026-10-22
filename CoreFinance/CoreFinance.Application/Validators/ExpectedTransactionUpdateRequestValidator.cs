using CoreFinance.Application.DTOs.ExpectedTransaction;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="ExpectedTransactionUpdateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="ExpectedTransactionUpdateRequest" />. (VI)
/// </summary>
public class ExpectedTransactionUpdateRequestValidator : AbstractValidator<ExpectedTransactionUpdateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpectedTransactionUpdateRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="ExpectedTransactionUpdateRequestValidator" />. (VI)
    /// </summary>
    public ExpectedTransactionUpdateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ExpectedDate).NotEmpty().When(x => x.ExpectedDate.HasValue);
        RuleFor(x => x.ExpectedAmount).GreaterThan(0).When(x => x.ExpectedAmount.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.AdjustmentReason).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}