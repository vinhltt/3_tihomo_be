using CoreFinance.Domain.Enums;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a request to create a new expected transaction. (EN)<br />
///     Đại diện cho request tạo giao dịch dự kiến mới. (VI)
/// </summary>
/// <remarks>
///     (EN) Creation validates Pending state and any supplied owned references; it does not create an actual transaction.
///     The server supplies ownership and generation time regardless of the client's UserId.<br />
///     (VI) Khi tạo, dịch vụ kiểm tra trạng thái Pending và quyền sở hữu các tham chiếu được gửi; không tạo giao dịch thực.
///     Máy chủ xác định owner và thời điểm sinh, không phụ thuộc UserId do client gửi.
/// </remarks>
public class ExpectedTransactionCreateRequest : BaseCreateRequest
{
    /// <summary>
    ///     An optional owned recurring template identifier; null is allowed, Guid.Empty is not. (EN)<br />
    ///     ID mẫu định kỳ tùy chọn thuộc owner hiện tại; cho phép null nhưng không chấp nhận Guid.Empty. (VI)
    /// </summary>
    public Guid? RecurringTransactionTemplateId { get; set; }

    /// <summary>
    ///     An optional client value ignored for ownership; the repository assigns the current owner. (EN)<br />
    ///     Giá trị client tùy chọn, bị bỏ qua khi xác định quyền sở hữu; repository gán owner hiện tại. (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     An optional owned account identifier; null is allowed, Guid.Empty is not. (EN)<br />
    ///     ID tài khoản tùy chọn thuộc owner hiện tại; cho phép null nhưng không chấp nhận Guid.Empty. (VI)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    ///     The required nominal expected date, interpreted as a calendar date for UTC+7 due-state calculations. (EN)<br />
    ///     Ngày dự kiến danh nghĩa bắt buộc, được xem là ngày lịch khi tính trạng thái đến hạn theo UTC+7. (VI)
    /// </summary>
    public DateTime ExpectedDate { get; set; }

    /// <summary>
    ///     The strictly positive expected amount of the transaction. (EN)<br />
    ///     Số tiền dự kiến của giao dịch, bắt buộc lớn hơn 0. (VI)
    /// </summary>
    public decimal ExpectedAmount { get; set; }

    /// <summary>
    ///     A description of the expected transaction (optional). (EN)<br />
    ///     Mô tả về giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The type of the recurring transaction (e.g., income, expense). (EN)<br />
    ///     Loại giao dịch định kỳ (ví dụ: thu nhập, chi tiêu). (VI)
    /// </summary>
    public RecurringTransactionType TransactionType { get; set; }

    /// <summary>
    ///     The category of the expected transaction (optional). (EN)<br />
    ///     Danh mục của giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     The initial status; only Pending is accepted when creating a period. (EN)<br />
    ///     Trạng thái ban đầu; chỉ chấp nhận Pending khi tạo kỳ. (VI)
    /// </summary>
    public ExpectedTransactionStatus Status { get; set; } = ExpectedTransactionStatus.Pending;

    /// <summary>
    ///     Additional notes about the expected transaction (optional). (EN)<br />
    ///     Ghi chú bổ sung về giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Notes { get; set; }
}