using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a request to update an existing expected transaction. (EN)<br />
///     Đại diện cho request cập nhật giao dịch dự kiến hiện có. (VI)
/// </summary>
public class ExpectedTransactionUpdateRequest : BaseUpdateRequest<Guid>
{
    /// <summary>
    ///     The updated expected date of the transaction (optional). (EN)<br />
    ///     Ngày dự kiến được cập nhật của giao dịch (tùy chọn). (VI)
    /// </summary>
    public DateTime? ExpectedDate { get; set; }

    /// <summary>
    ///     The updated expected amount of the transaction (optional). (EN)<br />
    ///     Số tiền dự kiến được cập nhật của giao dịch (tùy chọn). (VI)
    /// </summary>
    public decimal? ExpectedAmount { get; set; }

    /// <summary>
    ///     The updated description of the expected transaction (optional). (EN)<br />
    ///     Mô tả được cập nhật về giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The updated category of the expected transaction (optional). (EN)<br />
    ///     Danh mục được cập nhật của giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     The updated status of the expected transaction (optional). (EN)<br />
    ///     Trạng thái được cập nhật của giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public ExpectedTransactionStatus? Status { get; set; }

    /// <summary>
    ///     The reason for the adjustment (optional). (EN)<br />
    ///     Lý do điều chỉnh (tùy chọn). (VI)
    /// </summary>
    public string? AdjustmentReason { get; set; }

    /// <summary>
    ///     Additional updated notes about the expected transaction (optional). (EN)<br />
    ///     Ghi chú bổ sung được cập nhật về giao dịch dự kiến (tùy chọn). (VI)
    /// </summary>
    public string? Notes { get; set; }
}