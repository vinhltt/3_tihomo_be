namespace CoreFinance.Domain.Constants;

/// <summary>
///     (EN) Stable machine-readable codes for CoreFinance owner authorization and expected-transaction commands.<br />
///     (VI) Mã ổn định để máy xử lý cho phân quyền owner và lệnh giao dịch dự kiến của CoreFinance.
/// </summary>
public static class CoreFinanceErrorCodes
{
    /// <summary>
    ///     (EN) Invalid model state, request validation or unsupported input values.<br />
    ///     (VI) Model-state, kiểm tra request hoặc giá trị đầu vào không hợp lệ/không được hỗ trợ.
    /// </summary>
    public const string InvalidRequest = "invalid_request";
    /// <summary>
    ///     (EN) A failed save or unexpected request failure, without exposing persistence details.<br />
    ///     (VI) Lưu thất bại hoặc lỗi request ngoài dự kiến, không lộ chi tiết lưu trữ.
    /// </summary>
    public const string PersistenceError = "persistence_error";
    /// <summary>
    ///     (EN) The requested period is absent or outside the current owner's scope.<br />
    ///     (VI) Kỳ được yêu cầu không tồn tại hoặc ngoài phạm vi owner hiện tại.
    /// </summary>
    public const string ExpectedTransactionNotFound = "expected_transaction_not_found";
    /// <summary>
    ///     (EN) A not-found exception for a period or reference, without identifying foreign resources.<br />
    ///     (VI) Exception không tìm thấy kỳ hoặc tham chiếu, không chỉ ra tài nguyên của owner khác.
    /// </summary>
    public const string ResourceNotFound = "resource_not_found";
    /// <summary>
    ///     (EN) The period's lifecycle state or existing link prevents the requested edit or command.<br />
    ///     (VI) Trạng thái vòng đời hoặc liên kết đã có của kỳ ngăn chỉnh sửa hay thực hiện lệnh.
    /// </summary>
    public const string StateConflict = "state_conflict";
    /// <summary>
    ///     (EN) The original concurrency token lost a race with another period write.<br />
    ///     (VI) Token đồng thời gốc không còn khớp do một thao tác ghi kỳ khác.
    /// </summary>
    public const string ConcurrencyConflict = "concurrency_conflict";
    /// <summary>
    ///     (EN) An actual transaction link is already reserved by another period, including a tombstone.<br />
    ///     (VI) Liên kết giao dịch thực đã được kỳ khác giữ chỗ, kể cả bản xóa mềm.
    /// </summary>
    public const string AlreadyLinked = "already_linked";
    /// <summary>
    ///     (EN) The template/calendar-day uniqueness constraint rejects a duplicate period.<br />
    ///     (VI) Ràng buộc duy nhất theo mẫu/ngày lịch từ chối kỳ trùng.
    /// </summary>
    public const string DuplicatePeriod = "duplicate_period";
    /// <summary>
    ///     (EN) Authentication challenge (401), or owner-access exception (401/403 according to authentication).<br />
    ///     (VI) Yêu cầu xác thực (401), hoặc exception truy cập owner (401/403 tùy trạng thái xác thực).
    /// </summary>
    public const string Unauthorized = "unauthorized";
    /// <summary>
    ///     (EN) An authenticated request fails the owner policy before action execution (403).<br />
    ///     (VI) Request đã xác thực không đạt chính sách owner trước khi chạy action (403).
    /// </summary>
    public const string Forbidden = "forbidden";
    /// <summary>
    ///     (EN) Record-payment was requested for a type other than income or expense.<br />
    ///     (VI) Yêu cầu ghi nhận thanh toán cho loại không phải thu hoặc chi.
    /// </summary>
    public const string UnsupportedTransactionType = "unsupported_transaction_type";
    /// <summary>
    ///     (EN) A payment or adjustment requires a strictly positive expected amount.<br />
    ///     (VI) Thanh toán hoặc điều chỉnh yêu cầu số tiền dự kiến lớn hơn 0.
    /// </summary>
    public const string InvalidAmount = "invalid_amount";
    /// <summary>
    ///     (EN) The supplied payment account differs from the period's bound account.<br />
    ///     (VI) Tài khoản thanh toán được gửi khác tài khoản đã gắn với kỳ.
    /// </summary>
    public const string AccountOverrideNotAllowed = "account_override_not_allowed";
    /// <summary>
    ///     (EN) Payment has neither a usable bound account nor a nonempty fallback selection.<br />
    ///     (VI) Thanh toán không có tài khoản đã gắn dùng được hoặc lựa chọn dự phòng với ID khác rỗng.
    /// </summary>
    public const string AccountRequired = "account_required";
    /// <summary>
    ///     (EN) The payment account is missing or not owned by the period's owner.<br />
    ///     (VI) Tài khoản thanh toán không tồn tại hoặc không thuộc owner của kỳ.
    /// </summary>
    public const string AccountNotFound = "account_not_found";
    /// <summary>
    ///     (EN) Link-existing confirmation received an empty actual-transaction identifier.<br />
    ///     (VI) Xác nhận liên kết giao dịch có sẵn nhận ID giao dịch thực rỗng.
    /// </summary>
    public const string InvalidActualTransaction = "invalid_actual_transaction";
    /// <summary>
    ///     (EN) The actual to link is missing, deleted or outside the current owner's scope.<br />
    ///     (VI) Giao dịch thực cần liên kết không tồn tại, đã xóa hoặc ngoài phạm vi owner hiện tại.
    /// </summary>
    public const string ActualTransactionNotFound = "actual_transaction_not_found";
    /// <summary>
    ///     (EN) Cancellation or adjustment lacks a nonblank reason of at most 500 characters.<br />
    ///     (VI) Hủy hoặc điều chỉnh thiếu lý do không chỉ chứa khoảng trắng, tối đa 500 ký tự.
    /// </summary>
    public const string InvalidReason = "invalid_reason";
    /// <summary>
    ///     (EN) A lifecycle command's transaction committed; this is not an error despite the catalogue name.<br />
    ///     (VI) Transaction của lệnh vòng đời đã commit; đây không phải lỗi dù tên danh mục là mã lỗi.
    /// </summary>
    public const string Success = "success";
}
