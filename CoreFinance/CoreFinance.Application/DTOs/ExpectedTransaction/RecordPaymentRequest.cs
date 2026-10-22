namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     (EN) Selects an owned fallback account for an unbound period; amount and transaction date always come from the period.<br />
///     (VI) Chọn tài khoản dự phòng cùng owner cho kỳ chưa gắn tài khoản; số tiền và ngày giao dịch luôn lấy từ kỳ.
/// </summary>
/// <remarks>
///     (EN) Only an owned, unlinked Pending income or expense period can create a payment.
///     Transfer periods must link an existing transaction or be cancelled instead.<br />
///     (VI) Chỉ kỳ thu hoặc chi Pending thuộc owner hiện tại và chưa liên kết mới được tạo thanh toán.
///     Kỳ chuyển khoản phải liên kết giao dịch có sẵn hoặc được hủy.
/// </remarks>
public sealed class RecordPaymentRequest
{
    /// <summary>
    ///     (EN) The owned account to use when the period is unbound. Null is structurally valid, but the service requires
    ///     a nonempty owned fallback for an unbound period. For a bound period, omission is allowed and a supplied value
    ///     must match its account. Guid.Empty is rejected by validation.<br />
    ///     (VI) Tài khoản cùng owner dùng khi kỳ chưa gắn tài khoản. Null hợp lệ về cấu trúc, nhưng dịch vụ yêu cầu
    ///     tài khoản dự phòng cùng owner với ID khác rỗng cho kỳ chưa gắn tài khoản. Với kỳ đã gắn tài khoản, có thể bỏ qua
    ///     giá trị này; nếu gửi thì phải khớp tài khoản của kỳ. Validator từ chối Guid.Empty.
    /// </summary>
    public Guid? AccountId { get; set; }
}
