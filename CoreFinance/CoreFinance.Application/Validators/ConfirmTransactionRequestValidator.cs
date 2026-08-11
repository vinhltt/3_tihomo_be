using CoreFinance.Application.DTOs.ExpectedTransaction;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="ConfirmTransactionRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="ConfirmTransactionRequest" />. (VI)
/// </summary>
public class ConfirmTransactionRequestValidator : AbstractValidator<ConfirmTransactionRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfirmTransactionRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="ConfirmTransactionRequestValidator" />. (VI)
    /// </summary>
    public ConfirmTransactionRequestValidator()
    {
        RuleFor(x => x.ActualTransactionId).NotEmpty();
    }
}