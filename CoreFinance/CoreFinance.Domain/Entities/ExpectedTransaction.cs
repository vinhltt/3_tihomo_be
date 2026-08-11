using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreFinance.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Domain.Entities;

public class ExpectedTransaction : UserOwnedEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to RecurringTransactionTemplate (EN)
    ///     Khóa ngoại liên kết với mẫu giao dịch định kỳ (VI)
    /// </summary>
    public Guid? RecurringTransactionTemplateId { get; set; }



    /// <summary>
    ///     Foreign key linking to account (EN)
    ///     Khóa ngoại liên kết với tài khoản (VI)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    ///     Foreign key linking to actual transaction if completed (EN)
    ///     Khóa ngoại liên kết với giao dịch thực tế nếu đã hoàn thành (VI)
    /// </summary>
    public Guid? ActualTransactionId { get; set; }

    /// <summary>
    ///     Expected execution date (EN)
    ///     Ngày dự kiến thực hiện (VI)
    /// </summary>
    [Required]
    public DateTime ExpectedDate { get; set; }

    /// <summary>
    ///     Expected transaction amount (EN)
    ///     Số tiền giao dịch dự kiến (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Expected amount must be greater than 0")]
    public decimal ExpectedAmount { get; set; }

    /// <summary>
    ///     Transaction description (EN)
    ///     Mô tả giao dịch (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

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
    ///     Status of expected transaction (EN)
    ///     Trạng thái giao dịch dự kiến (VI)
    /// </summary>
    [Required]
    public ExpectedTransactionStatus Status { get; set; }

    /// <summary>
    ///     Whether this expected transaction was manually adjusted (EN)
    ///     Giao dịch dự kiến này có được điều chỉnh thủ công hay không (VI)
    /// </summary>
    [Required]
    public bool IsAdjusted { get; set; }

    /// <summary>
    ///     Original amount before adjustment (EN)
    ///     Số tiền gốc trước khi điều chỉnh (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Original amount must be greater than 0")]
    public decimal? OriginalAmount { get; set; }

    /// <summary>
    ///     Reason for adjustment or cancellation (EN)
    ///     Lý do điều chỉnh hoặc hủy bỏ (VI)
    /// </summary>
    [MaxLength(500)]
    public string? AdjustmentReason { get; set; }

    /// <summary>
    ///     Additional notes (EN)
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Date when expected transaction was generated (EN)
    ///     Ngày sinh giao dịch dự kiến (VI)
    /// </summary>
    [Required]
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    ///     Date when expected transaction was confirmed/cancelled (EN)
    ///     Ngày xác nhận/hủy bỏ giao dịch dự kiến (VI)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    ///     Navigation property: link to recurring transaction template (EN)
    ///     Thuộc tính điều hướng: liên kết với mẫu giao dịch định kỳ (VI)
    /// </summary>
    public RecurringTransactionTemplate? RecurringTransactionTemplate { get; set; }

    /// <summary>
    ///     Navigation property: link to account (EN)
    ///     Thuộc tính điều hướng: liên kết với tài khoản (VI)
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    ///     Navigation property: link to actual transaction if completed (EN)
    ///     Thuộc tính điều hướng: liên kết với giao dịch thực tế nếu hoàn thành (VI)
    /// </summary>
    public Transaction? ActualTransaction { get; set; }
}