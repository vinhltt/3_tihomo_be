using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Budget;

/// <summary>
///     Request model for updating an existing budget (EN)<br />
///     Model yêu cầu để cập nhật ngân sách hiện có (VI)
/// </summary>
public class UpdateBudgetRequest
{
    /// <summary>
    ///     Budget identifier (EN)<br />
    ///     Định danh ngân sách (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Budget name (EN)<br />
    ///     Tên ngân sách (VI)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Budget description (EN)<br />
    ///     Mô tả ngân sách (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Budget category (EN)<br />
    ///     Danh mục ngân sách (VI)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     Budget amount limit (EN)<br />
    ///     Giới hạn số tiền ngân sách (VI)
    /// </summary>
    public decimal BudgetAmount { get; set; }

    /// <summary>
    ///     Budget period type (EN)<br />
    ///     Loại chu kỳ ngân sách (VI)
    /// </summary>
    public BudgetPeriod Period { get; set; }

    /// <summary>
    ///     Budget start date (EN)<br />
    ///     Ngày bắt đầu ngân sách (VI)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     Budget end date (EN)<br />
    ///     Ngày kết thúc ngân sách (VI)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    ///     Budget status (EN)<br />
    ///     Trạng thái ngân sách (VI)
    /// </summary>
    public BudgetStatus Status { get; set; }

    /// <summary>
    ///     Alert threshold percentage (0-100) (EN)<br />
    ///     Ngưỡng cảnh báo theo phần trăm (0-100) (VI)
    /// </summary>
    public decimal? AlertThreshold { get; set; }

    /// <summary>
    ///     Whether to enable notifications (EN)<br />
    ///     Có bật thông báo hay không (VI)
    /// </summary>
    public bool EnableNotifications { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    public string? Notes { get; set; }
}