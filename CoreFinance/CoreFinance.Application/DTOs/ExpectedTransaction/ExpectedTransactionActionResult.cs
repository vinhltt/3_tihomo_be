using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Enums;

namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     (EN) An expected-transaction command result returned as Success only after commit; failed results never carry
///     an actual transaction that was rolled back.<br />
///     (VI) Kết quả lệnh giao dịch dự kiến chỉ trả Success sau commit; kết quả thất bại không bao giờ chứa
///     giao dịch thực đã rollback.
/// </summary>
/// <remarks>
///     (EN) Payment success exposes both IDs and the newly created actual. Confirm/cancel/adjust expose only success
///     and message at the controller; their service results do not promise populated projections. Caller-requested
///     cancellation and missing owner identity propagate as exceptions, not outcomes. Disposal failure after a successful
///     commit retains that committed success.<br />
///     (VI) Thanh toán thành công trả cả hai ID và giao dịch thực mới tạo. Xác nhận/hủy/điều chỉnh chỉ trả success và
///     message tại controller; kết quả dịch vụ không bảo đảm có projection. Yêu cầu hủy từ caller và thiếu danh tính owner
///     được truyền ra bằng exception, không chuyển thành outcome. Lỗi giải phóng tài nguyên sau commit thành công vẫn
///     giữ kết quả đã commit.
/// </remarks>
public sealed record ExpectedTransactionActionResult
{
    /// <summary>
    ///     (EN) The committed success or classified failure outcome.<br />
    ///     (VI) Kết quả thành công đã commit hoặc lỗi đã phân loại.
    /// </summary>
    public ExpectedTransactionActionOutcome Outcome { get; init; }
    /// <summary>
    ///     (EN) The stable machine-readable command code.<br />
    ///     (VI) Mã lệnh ổn định để máy xử lý.
    /// </summary>
    public string Code { get; init; } = string.Empty;
    /// <summary>
    ///     (EN) The human-readable success or failure message.<br />
    ///     (VI) Thông báo thành công hoặc thất bại để người dùng đọc.
    /// </summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>
    ///     (EN) The requested period ID; on failure it does not prove an owned period exists.<br />
    ///     (VI) ID kỳ được yêu cầu; khi thất bại, giá trị này không chứng minh có kỳ thuộc owner hiện tại.
    /// </summary>
    public Guid ExpectedTransactionId { get; init; }
    /// <summary>
    ///     (EN) The committed actual link on successful payment or confirmation; otherwise normally null.<br />
    ///     (VI) ID giao dịch thực đã liên kết và commit khi thanh toán hoặc xác nhận thành công; trường hợp khác thường null.
    /// </summary>
    public Guid? ActualTransactionId { get; init; }
    /// <summary>
    ///     (EN) The new actual on committed payment success; null for link-existing, other commands and failures.<br />
    ///     (VI) Giao dịch thực mới khi thanh toán đã commit thành công; null khi liên kết giao dịch có sẵn, lệnh khác hoặc thất bại.
    /// </summary>
    public TransactionViewModel? ActualTransaction { get; init; }
    /// <summary>
    ///     (EN) An optional period projection; current command results leave it null even on success.<br />
    ///     (VI) Projection kỳ tùy chọn; kết quả lệnh hiện tại để null kể cả khi thành công.
    /// </summary>
    public ExpectedTransactionViewModel? Transaction { get; init; }
}

/// <summary>
///     (EN) A lifecycle conflict on the expected-transaction CRUD path, which retains its exception contract.<br />
///     (VI) Xung đột vòng đời trên đường CRUD giao dịch dự kiến; đường này giữ hợp đồng dùng exception.
/// </summary>
/// <param name="message">(EN) The lifecycle conflict explanation.<br />(VI) Nội dung giải thích xung đột vòng đời.</param>
public sealed class ExpectedTransactionConflictException(string message) : InvalidOperationException(message);
