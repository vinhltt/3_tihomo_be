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
            .NotEmpty()
            .Must(type => Enum.TryParse<AccountType>(type, true, out var accountType) && Enum.IsDefined(accountType));
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);

        // Code validation rules - only apply when Code is provided
        // Quy tắc xác thực Code - chỉ áp dụng khi Code được cung cấp
        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code)
                .MaximumLength(50)
                .WithMessage(CoreFinanceMessages.AccountCodeMaxLength);

            RuleFor(x => x.Code)
                .Matches(@"^[a-z0-9_]+$")
                .WithMessage(CoreFinanceMessages.AccountCodeSnakeCase);
        });

        // Reject empty string explicitly (null is OK, empty string is not)
        // Từ chối chuỗi rỗng một cách rõ ràng (null được chấp nhận, chuỗi rỗng thì không)
        RuleFor(x => x.Code)
            .NotEmpty()
            .When(x => x.Code != null)
            .WithMessage(CoreFinanceMessages.AccountCodeNotEmpty);
    }
}