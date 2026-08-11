using CoreFinance.Application.DTOs.Transaction;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="TransactionCreateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="TransactionCreateRequest" />. (VI)
/// </summary>
public class CreateTransactionRequestValidator : AbstractValidator<TransactionCreateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateTransactionRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="CreateTransactionRequestValidator" />. (VI)
    /// </summary>
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.RevenueAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SpentAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryType).IsInEnum();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}