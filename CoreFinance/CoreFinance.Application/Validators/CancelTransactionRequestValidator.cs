using CoreFinance.Application.DTOs.ExpectedTransaction;
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
        RuleFor(x => x.Reason).NotEmpty().When(x => !string.IsNullOrEmpty(x.Reason)); // Validate if reason is provided
    }
}