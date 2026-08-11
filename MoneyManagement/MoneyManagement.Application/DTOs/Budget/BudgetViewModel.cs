using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Budget;

/// <summary>
///     Budget view model for displaying budget information (EN)<br />
///     View model ngân sách để hiển thị thông tin ngân sách (VI)
/// </summary>
public class BudgetViewModel
{
    /// <summary>
    ///     Budget identifier (EN)<br />
    ///     Định danh ngân sách (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     User identifier (EN)<br />
    ///     Định danh người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

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
    ///     Amount spent so far (EN)<br />
    ///     Số tiền đã chi tiêu (VI)
    /// </summary>
    public decimal SpentAmount { get; set; }

    /// <summary>
    ///     Remaining amount (EN)<br />
    ///     Số tiền còn lại (VI)
    /// </summary>
    public decimal RemainingAmount { get; set; }

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
    ///     Alert threshold percentage (EN)<br />
    ///     Ngưỡng cảnh báo theo phần trăm (VI)
    /// </summary>
    public decimal? AlertThreshold { get; set; }

    /// <summary>
    ///     Whether notifications are enabled (EN)<br />
    ///     Có bật thông báo hay không (VI)
    /// </summary>
    public bool EnableNotifications { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Whether budget is over limit (EN)<br />
    ///     Ngân sách có vượt giới hạn không (VI)
    /// </summary>
    public bool IsOverBudget { get; set; }

    /// <summary>
    ///     Spending percentage (EN)<br />
    ///     Phần trăm chi tiêu (VI)
    /// </summary>
    public decimal SpendingPercentage { get; set; }

    /// <summary>
    ///     Whether alert threshold is reached (EN)<br />
    ///     Có đạt ngưỡng cảnh báo không (VI)
    /// </summary>
    public bool IsAlertThresholdReached { get; set; }

    /// <summary>
    ///     Creation date (EN)<br />
    ///     Ngày tạo (VI)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    ///     Last update date (EN)<br />
    ///     Ngày cập nhật cuối (VI)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}