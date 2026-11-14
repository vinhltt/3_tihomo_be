using System.Linq.Expressions;
using CoreFinance.Domain.Constants;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     Shares the account <c>Code</c> rules between the account create and update validators. (EN)<br />
///     Dùng chung các rule <c>Code</c> của tài khoản giữa validator tạo và cập nhật tài khoản. (VI)
/// </summary>
public static class AccountCodeRuleExtensions
{
    /// <summary>
    ///     Registers the optional account code rules: when provided, at most 50 characters and snake_case; an empty
    ///     string is rejected while null is allowed. (EN)<br />
    ///     Đăng ký các rule mã tài khoản tùy chọn: khi có giá trị thì tối đa 50 ký tự và theo snake_case; chuỗi rỗng bị
    ///     từ chối còn null được chấp nhận. (VI)
    /// </summary>
    /// <param name="validator">
    ///     The validator that receives the rules. (EN)<br />
    ///     Validator nhận các rule. (VI)
    /// </param>
    /// <param name="codeSelector">
    ///     Selects the account code property of the request. (EN)<br />
    ///     Chọn thuộc tính mã tài khoản của request. (VI)
    /// </param>
    /// <typeparam name="T">
    ///     The request type being validated. (EN)<br />
    ///     Kiểu request được xác thực. (VI)
    /// </typeparam>
    public static void RuleForAccountCode<T>(this AbstractValidator<T> validator,
        Expression<Func<T, string?>> codeSelector)
    {
        var getCode = codeSelector.Compile();

        // Code validation rules - only apply when Code is provided
        // Quy tắc xác thực Code - chỉ áp dụng khi Code được cung cấp
        validator.When(request => !string.IsNullOrWhiteSpace(getCode(request)), () =>
        {
            validator.RuleFor(codeSelector)
                .MaximumLength(50)
                .WithMessage(CoreFinanceMessages.AccountCodeMaxLength);

            validator.RuleFor(codeSelector)
                .Matches(@"^[a-z0-9_]+$")
                .WithMessage(CoreFinanceMessages.AccountCodeSnakeCase);
        });

        // Reject empty string explicitly (null is OK, empty string is not)
        // Từ chối chuỗi rỗng một cách rõ ràng (null được chấp nhận, chuỗi rỗng thì không)
        validator.RuleFor(codeSelector)
            .NotEmpty()
            .When(request => getCode(request) != null)
            .WithMessage(CoreFinanceMessages.AccountCodeNotEmpty);
    }
}
