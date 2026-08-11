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
        // Add validation rules for other properties if needed
        // Example: RuleFor(x => x.ExpectedAmount).GreaterThanOrEqualTo(0).When(x => x.ExpectedAmount.HasValue);
        // Example: RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}