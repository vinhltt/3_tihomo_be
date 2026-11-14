using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Constants;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="AdjustTransactionRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="AdjustTransactionRequest" />. (VI)
/// </summary>
public class AdjustTransactionRequestValidator : AbstractValidator<AdjustTransactionRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AdjustTransactionRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="AdjustTransactionRequestValidator" />. (VI)
    /// </summary>
    public AdjustTransactionRequestValidator()
    {
        RuleFor(x => x.NewAmount)
            .GreaterThan(0).WithMessage(CoreFinanceMessages.ExpectedAmountMustBePositive).WithErrorCode(CoreFinanceErrorCodes.InvalidAmount);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(CoreFinanceMessages.ReasonRequired).WithErrorCode(CoreFinanceErrorCodes.InvalidReason)
            .MaximumLength(CoreFinanceMessages.MaxReasonLength).WithMessage(CoreFinanceMessages.ReasonRequired).WithErrorCode(CoreFinanceErrorCodes.InvalidReason);
    }
}