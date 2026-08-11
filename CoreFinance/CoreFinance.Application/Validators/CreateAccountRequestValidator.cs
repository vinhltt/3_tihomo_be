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
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);

        // Code validation rules - only apply when Code is provided
        // Quy tắc xác thực Code - chỉ áp dụng khi Code được cung cấp
        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code)
                .MaximumLength(50)
                .WithMessage("Account code must not exceed 50 characters / Mã tài khoản không được vượt quá 50 ký tự");

            RuleFor(x => x.Code)
                .Matches(@"^[a-z0-9_]+$")
                .WithMessage("Account code must be snake_case (lowercase letters, numbers, underscores only) / Mã tài khoản phải là snake_case (chỉ chữ thường, số, gạch dưới)");

            // TODO: Add uniqueness check in T012 when repository is implemented
            // RuleFor(x => x.Code)
            //     .MustAsync(async (code, cancellation) =>
            //     {
            //         var existing = await _accountRepository.GetByCodeAsync(code);
            //         return existing == null;
            //     })
            //     .WithMessage("Account code already exists / Mã tài khoản đã tồn tại");
        });

        // Reject empty string explicitly (null is OK, empty string is not)
        // Từ chối chuỗi rỗng một cách rõ ràng (null được chấp nhận, chuỗi rỗng thì không)
        RuleFor(x => x.Code)
            .NotEmpty()
            .When(x => x.Code != null)
            .WithMessage("Account code cannot be empty string / Mã tài khoản không được là chuỗi rỗng");
    }
}