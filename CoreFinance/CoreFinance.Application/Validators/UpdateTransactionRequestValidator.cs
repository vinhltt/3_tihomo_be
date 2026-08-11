using CoreFinance.Application.DTOs.Transaction;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="TransactionUpdateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="TransactionUpdateRequest" />. (VI)
/// </summary>
public class UpdateTransactionRequestValidator : AbstractValidator<TransactionUpdateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateTransactionRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="UpdateTransactionRequestValidator" />. (VI)
    /// </summary>
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.RevenueAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SpentAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryType).IsInEnum();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}