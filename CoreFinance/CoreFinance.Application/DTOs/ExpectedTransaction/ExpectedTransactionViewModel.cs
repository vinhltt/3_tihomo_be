using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a view model for expected transaction data. (EN)<br />
///     Đại diện cho view model dữ liệu giao dịch dự kiến. (VI)
/// </summary>
/// <remarks>
///     (EN) Queries derive date-based fields in SQL from the current UTC+7 business date before filtering or paging.
///     Navigation names and account type are exposed only when the related entity belongs to the period's owner.<br />
///     (VI) Truy vấn tính các trường theo ngày trong SQL từ ngày nghiệp vụ hiện tại UTC+7 trước khi lọc hoặc phân trang.
///     Tên tham chiếu và loại tài khoản chỉ được hiển thị khi thực thể liên quan thuộc cùng owner với kỳ.
/// </remarks>
public class ExpectedTransactionViewModel : BaseViewModel<Guid>
{
    /// <summary>
    ///     The optional recurring template identifier stored on the period. (EN)<br />
    ///     ID mẫu định kỳ tùy chọn được lưu trên kỳ. (VI)
    /// </summary>
    public Guid? RecurringTransactionTemplateId { get; set; }

    /// <summary>
    ///     The server-controlled owner identifier; nullable for stored legacy data. (EN)<br />
    ///     ID owner do máy chủ quản lý; có thể null đối với dữ liệu cũ đã lưu. (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     The optional account identifier stored on the period. (EN)<br />
    ///     ID tài khoản tùy chọn được lưu trên kỳ. (VI)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    ///     The ID of the actual transaction linked to this expected transaction (optional). (EN)<br />
    ///     ID của giao dịch thực tế được liên kết với giao dịch dự kiến này (tùy chọn). (VI)
    /// </summary>
    public Guid? ActualTransactionId { get; set; }

    /// <summary>
    ///     The expected date of the transaction. (EN)<br />
    ///     Ngày dự kiến của giao dịch. (VI)
    /// </summary>
    public DateTime ExpectedDate { get; set; }

    /// <summary>
    ///     The expected amount of the transaction. (EN)<br />
    ///     Số tiền dự kiến của giao dịch. (VI)
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
    ///     The current status of the expected transaction (e.g., Pending, Confirmed, Cancelled). (EN)<br />
    ///     Trạng thái hiện tại của giao dịch dự kiến (ví dụ: Đang chờ, Đã xác nhận, Đã hủy). (VI)
    /// </summary>
    public ExpectedTransactionStatus Status { get; set; }

    /// <summary>
    ///     (EN) Response-only derived display state: persisted states 0..3, or 4 for Pending before today in UTC+7.
    ///     Overdue is not stored and does not transition or cancel the period.<br />
    ///     (VI) Trạng thái hiển thị chỉ dẫn xuất trong phản hồi: trạng thái lưu trữ 0..3, hoặc 4 cho Pending trước hôm nay
    ///     theo UTC+7. Overdue không được lưu và không làm chuyển trạng thái hay hủy kỳ.
    /// </summary>
    public int DisplayStatus { get; set; }

    /// <summary>
    ///     Indicates if the expected transaction amount has been adjusted. (EN)<br />
    ///     Cho biết số tiền giao dịch dự kiến đã được điều chỉnh hay chưa. (VI)
    /// </summary>
    public bool IsAdjusted { get; set; }

    /// <summary>
    ///     The original expected amount before any adjustment (optional). (EN)<br />
    ///     Số tiền dự kiến ban đầu trước khi điều chỉnh (tùy chọn). (VI)
    /// </summary>
    public decimal? OriginalAmount { get; set; }

    /// <summary>
    ///     The reason for the latest adjustment or cancellation (optional). (EN)<br />
    ///     Lý do điều chỉnh gần nhất hoặc hủy kỳ (tùy chọn). (VI)
    /// </summary>
    public string? AdjustmentReason { get; set; }

    /// <summary>
    ///     Additional notes about the expected transaction (optional). (EN)<br />
    ///     Ghi chú bổ sung về giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     The date and time the expected transaction was generated. (EN)<br />
    ///     Ngày và giờ giao dịch dự kiến được sinh ra. (VI)
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    ///     The date and time the expected transaction was processed (confirmed or cancelled) (optional). (EN)<br />
    ///     Ngày và giờ giao dịch dự kiến được xử lý (xác nhận hoặc hủy) (tùy chọn). (VI)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    ///     The template name when its owner matches the period; empty when the reference is absent or foreign. (EN)<br />
    ///     Tên mẫu khi cùng owner với kỳ; rỗng nếu không có tham chiếu hoặc tham chiếu thuộc owner khác. (VI)
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    ///     The account name when its owner matches the period; empty when the reference is absent or foreign. (EN)<br />
    ///     Tên tài khoản khi cùng owner với kỳ; rỗng nếu không có tham chiếu hoặc tham chiếu thuộc owner khác. (VI)
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    ///     The associated account type, or null when the account is absent or foreign. (EN)<br />
    ///     Loại tài khoản liên quan, hoặc null khi tài khoản không tồn tại hoặc thuộc owner khác. (VI)
    /// </summary>
    public AccountType? AccountType { get; set; }

    /// <summary>
    ///     Indicates if this expected transaction has been linked to an actual transaction. (EN)<br />
    ///     Cho biết giao dịch dự kiến này đã được liên kết với một giao dịch thực tế hay chưa. (VI)
    /// </summary>
    public bool HasActualTransaction { get; set; }

    /// <summary>
    ///     ExpectedDate minus today UTC+7 in calendar days; negative values indicate a past due date. (EN)<br />
    ///     ExpectedDate trừ hôm nay UTC+7 theo số ngày lịch; giá trị âm biểu thị ngày hạn đã qua. (VI)
    /// </summary>
    public int DaysUntilDue { get; set; }
}