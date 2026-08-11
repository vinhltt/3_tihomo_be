using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a view model for expected transaction data. (EN)<br />
///     Đại diện cho view model dữ liệu giao dịch dự kiến. (VI)
/// </summary>
public class ExpectedTransactionViewModel : BaseViewModel<Guid>
{
    /// <summary>
    ///     The ID of the recurring transaction template this expected transaction is based on. (EN)<br />
    ///     ID của mẫu giao dịch định kỳ mà giao dịch dự kiến này dựa trên. (VI)
    /// </summary>
    public Guid RecurringTransactionTemplateId { get; set; }

    /// <summary>
    ///     The ID of the user associated with the expected transaction (optional). (EN)<br />
    ///     ID của người dùng liên quan đến giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     The ID of the account associated with the expected transaction. (EN)<br />
    ///     ID của tài khoản liên quan đến giao dịch dự kiến. (VI)
    /// </summary>
    public Guid AccountId { get; set; }

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
    ///     The reason for the adjustment (optional). (EN)<br />
    ///     Lý do điều chỉnh (tùy chọn). (VI)
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
    ///     The name of the recurring transaction template this expected transaction is based on. (EN)<br />
    ///     Tên của mẫu giao dịch định kỳ mà giao dịch dự kiến này dựa trên. (VI)
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    ///     The name of the associated account. (EN)<br />
    ///     Tên của tài khoản liên kết. (VI)
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    ///     The type of the associated account. (EN)<br />
    ///     Loại của tài khoản liên kết. (VI)
    /// </summary>
    public AccountType? AccountType { get; set; }

    /// <summary>
    ///     Indicates if this expected transaction has been linked to an actual transaction. (EN)<br />
    ///     Cho biết giao dịch dự kiến này đã được liên kết với một giao dịch thực tế hay chưa. (VI)
    /// </summary>
    public bool HasActualTransaction { get; set; }

    /// <summary>
    ///     The number of days until the expected transaction is due. (EN)<br />
    ///     Số ngày còn lại cho đến ngày đáo hạn của giao dịch dự kiến. (VI)
    /// </summary>
    public int DaysUntilDue { get; set; }
}