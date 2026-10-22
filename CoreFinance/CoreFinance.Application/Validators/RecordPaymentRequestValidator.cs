using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Constants;
using FluentValidation;

namespace CoreFinance.Application.Validators;

/// <summary>
///     (EN) Validates the optional payment account identifier; ownership, required fallback and bound-account rules
///     depend on the stored period and are enforced by the payment service.<br />
///     (VI) Kiểm tra ID tài khoản thanh toán tùy chọn; quyền sở hữu, yêu cầu tài khoản dự phòng và quy tắc tài khoản đã gắn
///     phụ thuộc kỳ được lưu và do dịch vụ thanh toán kiểm tra.
/// </summary>
public sealed class RecordPaymentRequestValidator : AbstractValidator<RecordPaymentRequest>
{
    /// <summary>
    ///     (EN) Allows null or a nonempty account identifier, rejects Guid.Empty, and does not change payment amount or date.<br />
    ///     (VI) Cho phép null hoặc ID tài khoản khác rỗng, từ chối Guid.Empty và không thay đổi số tiền hay ngày thanh toán.
    /// </summary>
    public RecordPaymentRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .Must(id => id != Guid.Empty).WithMessage(CoreFinanceMessages.AccountIdMustNotBeEmpty);
    }
}
