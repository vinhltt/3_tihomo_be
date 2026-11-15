using CoreFinance.Application.DTOs.Account;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Enums;
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
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(CoreFinanceMessages.AccountTypeInvalid)
            .Must(type => IsDefinedAccountTypeName(type!))
            .WithMessage(CoreFinanceMessages.AccountTypeInvalid);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        this.RuleForAccountCode(x => x.Code);
    }

    // Accepts only enum names, never numeric or comma-joined values. Surrounding whitespace is trimmed because the
    // AutoMapper string-to-enum mapping trims it too, so every accepted value maps to the named member.
    private static bool IsDefinedAccountTypeName(string type) =>
        Enum.GetNames<AccountType>().Contains(type.Trim(), StringComparer.OrdinalIgnoreCase);
}