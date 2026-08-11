using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.EntityFramework.BaseEfModels;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

/// <summary>
///     Budget entity for managing spending budgets (EN)<br />
///     Entity ngân sách để quản lý ngân sách chi tiêu (VI)
/// </summary>
public class Budget : BaseEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to user (EN)<br />
    ///     Khóa ngoại liên kết với người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Budget name (EN)<br />
    ///     Tên ngân sách (VI)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Budget description (EN)<br />
    ///     Mô tả ngân sách (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Budget category (EN)<br />
    ///     Danh mục ngân sách (VI)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    ///     Budget amount limit (EN)<br />
    ///     Giới hạn số tiền ngân sách (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Budget amount must be non-negative")]
    public decimal BudgetAmount { get; set; }

    /// <summary>
    ///     Amount spent so far (EN)<br />
    ///     Số tiền đã chi tiêu (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Spent amount must be non-negative")]
    public decimal SpentAmount { get; set; } = 0;

    /// <summary>
    ///     Remaining amount (EN)<br />
    ///     Số tiền còn lại (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount => BudgetAmount - SpentAmount;

    /// <summary>
    ///     Budget period type (EN)<br />
    ///     Loại chu kỳ ngân sách (VI)
    /// </summary>
    [Required]
    public BudgetPeriod Period { get; set; }

    /// <summary>
    ///     Budget start date (EN)<br />
    ///     Ngày bắt đầu ngân sách (VI)
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     Budget end date (EN)<br />
    ///     Ngày kết thúc ngân sách (VI)
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    ///     Budget status (EN)<br />
    ///     Trạng thái ngân sách (VI)
    /// </summary>
    [Required]
    public BudgetStatus Status { get; set; } = BudgetStatus.Active;

    /// <summary>
    ///     Alert threshold percentage (0-100) (EN)<br />
    ///     Ngưỡng cảnh báo theo phần trăm (0-100) (VI)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Alert threshold must be between 0 and 100")]
    public decimal? AlertThreshold { get; set; } = 80;

    /// <summary>
    ///     Whether to send notifications (EN)<br />
    ///     Có gửi thông báo hay không (VI)
    /// </summary>
    public bool EnableNotifications { get; set; } = true;

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Check if budget is over limit (EN)<br />
    ///     Kiểm tra ngân sách có vượt giới hạn không (VI)
    /// </summary>
    public bool IsOverBudget => SpentAmount > BudgetAmount;

    /// <summary>
    ///     Get spending percentage (EN)<br />
    ///     Lấy phần trăm chi tiêu (VI)
    /// </summary>
    public decimal SpendingPercentage => BudgetAmount > 0 ? SpentAmount / BudgetAmount * 100 : 0;

    /// <summary>
    ///     Check if alert threshold is reached (EN)<br />
    ///     Kiểm tra có đạt ngưỡng cảnh báo không (VI)
    /// </summary>
    public bool IsAlertThresholdReached => AlertThreshold.HasValue && SpendingPercentage >= AlertThreshold.Value;
}