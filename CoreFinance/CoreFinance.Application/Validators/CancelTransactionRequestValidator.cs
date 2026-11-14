using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Constants;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="CancelTransactionRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="CancelTransactionRequest" />. (VI)
/// </summary>
public class CancelTransactionRequestValidator : AbstractValidator<CancelTransactionRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancelTransactionRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="CancelTransactionRequestValidator" />. (VI)
    /// </summary>
    public CancelTransactionRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(CoreFinanceMessages.ReasonRequired).WithErrorCode(CoreFinanceErrorCodes.InvalidReason)
            .MaximumLength(CoreFinanceMessages.MaxReasonLength).WithMessage(CoreFinanceMessages.ReasonRequired).WithErrorCode(CoreFinanceErrorCodes.InvalidReason);
    }
}