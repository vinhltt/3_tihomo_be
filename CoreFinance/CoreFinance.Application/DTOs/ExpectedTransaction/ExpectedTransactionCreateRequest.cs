using CoreFinance.Domain.Enums;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a request to create a new expected transaction. (EN)<br />
///     Đại diện cho request tạo giao dịch dự kiến mới. (VI)
/// </summary>
public class ExpectedTransactionCreateRequest : BaseCreateRequest
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
    public ExpectedTransactionStatus Status { get; set; } = ExpectedTransactionStatus.Pending;

    /// <summary>
    ///     Additional notes about the expected transaction (optional). (EN)<br />
    ///     Ghi chú bổ sung về giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Notes { get; set; }
}