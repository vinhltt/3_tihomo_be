using CoreFinance.Application.DTOs.ExpectedTransaction;
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
        RuleFor(x => x.NewAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().When(x => !string.IsNullOrEmpty(x.Reason)); // Validate if reason is provided
    }
}