using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreFinance.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Domain.Entities;

public class RecurringTransactionTemplate : UserOwnedEntity<Guid>
{


    /// <summary>
    ///     Foreign key linking to account (EN)
    ///     Khóa ngoại liên kết với tài khoản (VI)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    ///     Template name for easy identification (EN)
    ///     Tên mẫu để dễ nhận biết (VI)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Transaction description template (EN)
    ///     Mẫu mô tả giao dịch (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Transaction amount (EN)
    ///     Số tiền giao dịch (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    ///     Transaction type (income, expense, transfer) (EN)
    ///     Loại giao dịch (thu, chi, chuyển khoản) (VI)
    /// </summary>
    [Required]
    public RecurringTransactionType TransactionType { get; set; }

    /// <summary>
    ///     Transaction category for classification (EN)
    ///     Danh mục giao dịch để phân loại (VI)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    ///     Recurrence frequency (daily, weekly, monthly, etc.) (EN)
    ///     Tần suất lặp lại (hàng ngày, tuần, tháng, v.v.) (VI)
    /// </summary>
    [Required]
    public RecurrenceFrequency Frequency { get; set; }

    /// <summary>
    ///     Custom interval in days (for custom frequency) (EN)
    ///     Khoảng cách tùy chỉnh tính bằng ngày (cho tần suất tùy chỉnh) (VI)
    /// </summary>
    [Range(1, 365, ErrorMessage = "Custom interval must be between 1 and 365 days")]
    public int? CustomIntervalDays { get; set; }

    /// <summary>
    ///     Next execution date (EN)
    ///     Ngày thực hiện tiếp theo (VI)
    /// </summary>
    [Required]
    public DateTime NextExecutionDate { get; set; }

    /// <summary>
    ///     Start date for recurring transactions (EN)
    ///     Ngày bắt đầu cho giao dịch định kỳ (VI)
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     End date for recurring transactions (optional) (EN)
    ///     Ngày kết thúc cho giao dịch định kỳ (tùy chọn) (VI)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     Cron expression for complex scheduling (optional) (EN)
    ///     Biểu thức cron cho lập lịch phức tạp (tùy chọn) (VI)
    /// </summary>
    [MaxLength(100)]
    public string? CronExpression { get; set; }

    /// <summary>
    ///     Whether template is active (EN)
    ///     Mẫu có đang hoạt động hay không (VI)
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    ///     Whether to auto-generate expected transactions (EN)
    ///     Có tự động sinh giao dịch dự kiến hay không (VI)
    /// </summary>
    [Required]
    public bool AutoGenerate { get; set; }

    /// <summary>
    ///     Number of days in advance to generate expected transactions (EN)
    ///     Số ngày trước để sinh giao dịch dự kiến (VI)
    /// </summary>
    [Required]
    [Range(1, 365, ErrorMessage = "Days in advance must be between 1 and 365")]
    public int DaysInAdvance { get; set; } = 30;

    /// <summary>
    ///     Additional notes for the template (EN)
    ///     Ghi chú bổ sung cho mẫu (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Navigation property: link to account (EN)
    ///     Thuộc tính điều hướng: liên kết với tài khoản (VI)
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    ///     Navigation property: expected transactions generated from this template (EN)
    ///     Thuộc tính điều hướng: giao dịch dự kiến được sinh từ mẫu này (VI)
    /// </summary>
    public ICollection<ExpectedTransaction>? ExpectedTransactions { get; set; }
}