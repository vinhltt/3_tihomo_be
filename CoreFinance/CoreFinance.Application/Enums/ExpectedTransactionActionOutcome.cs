namespace CoreFinance.Application.Enums;

/// <summary>
///     (EN) Application command outcomes mapped to the expected-transaction API's local HTTP contract, not persisted states.<br />
///     (VI) Kết quả lệnh tầng application được ánh xạ sang hợp đồng HTTP cục bộ của API giao dịch dự kiến, không phải trạng thái lưu trữ.
/// </summary>
public enum ExpectedTransactionActionOutcome
{
    /// <summary>
    ///     (EN) The command's transaction committed successfully.<br />
    ///     (VI) Transaction của lệnh đã commit thành công.
    /// </summary>
    Success,
    /// <summary>
    ///     (EN) The period or referenced resource is missing or not owned; maps to HTTP 404.<br />
    ///     (VI) Kỳ hoặc tài nguyên tham chiếu không tồn tại hay không cùng owner; ánh xạ thành HTTP 404.
    /// </summary>
    NotFound,
    /// <summary>
    ///     (EN) Command input is invalid or unsupported; maps to HTTP 400.<br />
    ///     (VI) Đầu vào lệnh không hợp lệ hoặc không được hỗ trợ; ánh xạ thành HTTP 400.
    /// </summary>
    InvalidRequest,
    /// <summary>
    ///     (EN) A lifecycle, existing-link or concurrency conflict prevents the command; maps to HTTP 409.<br />
    ///     (VI) Xung đột vòng đời, liên kết đã có hoặc truy cập đồng thời ngăn lệnh thực hiện; ánh xạ thành HTTP 409.
    /// </summary>
    Conflict,
    /// <summary>
    ///     (EN) The operation could not be persisted; maps to HTTP 500 without persistence details.<br />
    ///     (VI) Không thể lưu thao tác; ánh xạ thành HTTP 500 mà không lộ chi tiết lưu trữ.
    /// </summary>
    PersistenceError
}
