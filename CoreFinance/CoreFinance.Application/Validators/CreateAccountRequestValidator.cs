using CoreFinance.Application.DTOs.Account;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Validates the <see cref="AccountCreateRequest" />. (EN)<br />
///     Thực hiện xác thực cho <see cref="AccountCreateRequest" />. (VI)
/// </summary>
public class CreateAccountRequestValidator : AbstractValidator<AccountCreateRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateAccountRequestValidator" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="CreateAccountRequestValidator" />. (VI)
    /// </summary>
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        this.RuleForAccountCode(x => x.Code);

        // TODO: Add uniqueness check in T012 when repository is implemented
        // RuleFor(x => x.Code)
        //     .MustAsync(async (code, cancellation) =>
        //     {
        //         var existing = await _accountRepository.GetByCodeAsync(code);
        //         return existing == null;
        //     })
        //     .WithMessage("Account code already exists / Mã tài khoản đã tồn tại");
    }
}