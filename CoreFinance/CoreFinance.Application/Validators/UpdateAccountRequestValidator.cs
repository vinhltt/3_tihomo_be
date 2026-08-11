using CoreFinance.Application.DTOs.Account;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="AccountUpdateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="AccountUpdateRequest" />. (VI)
/// </summary>
public class UpdateAccountRequestValidator : AbstractValidator<AccountUpdateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateAccountRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="UpdateAccountRequestValidator" />. (VI)
    /// </summary>
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
    }
}