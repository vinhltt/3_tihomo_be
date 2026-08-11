using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlanningInvestment.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace PlanningInvestment.Domain.Entities;

/// <summary>
///     Represents a debt entity for debt management. (EN)<br />
///     Đại diện cho thực thể khoản nợ để quản lý nợ. (VI)
/// </summary>
public class Debt : BaseEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to user (EN)<br />
    ///     Khóa ngoại liên kết với người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Debt name for identification (EN)<br />
    ///     Tên khoản nợ để nhận biết (VI)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Description of the debt (EN)<br />
    ///     Mô tả về khoản nợ (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Type of debt (EN)<br />
    ///     Loại khoản nợ (VI)
    /// </summary>
    [Required]
    public DebtType DebtType { get; set; }

    /// <summary>
    ///     Original debt amount (EN)<br />
    ///     Số tiền nợ ban đầu (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Original amount must be greater than 0")]
    public decimal OriginalAmount { get; set; }

    /// <summary>
    ///     Current remaining balance (EN)<br />
    ///     Số dư còn lại hiện tại (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Current balance must be greater than or equal to 0")]
    public decimal CurrentBalance { get; set; }

    /// <summary>
    ///     Interest rate (annual percentage) (EN)<br />
    ///     Lãi suất (phần trăm hàng năm) (VI)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
    public decimal? InterestRate { get; set; }

    /// <summary>
    ///     Minimum monthly payment (EN)<br />
    ///     Khoản thanh toán tối thiểu hàng tháng (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Minimum payment must be greater than or equal to 0")]
    public decimal? MinimumPayment { get; set; }

    /// <summary>
    ///     Due date for monthly payment (EN)<br />
    ///     Ngày đáo hạn thanh toán hàng tháng (VI)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    ///     Creditor or lender name (EN)<br />
    ///     Tên chủ nợ hoặc người cho vay (VI)
    /// </summary>
    [MaxLength(200)]
    public string? Creditor { get; set; }

    /// <summary>
    ///     Account number or reference (EN)<br />
    ///     Số tài khoản hoặc tham chiếu (VI)
    /// </summary>
    [MaxLength(100)]
    public string? AccountNumber { get; set; }

    /// <summary>
    ///     Target payoff date (EN)<br />
    ///     Ngày mục tiêu trả hết nợ (VI)
    /// </summary>
    public DateTime? TargetPayoffDate { get; set; }

    /// <summary>
    ///     Whether the debt is active (EN)<br />
    ///     Khoản nợ có đang hoạt động hay không (VI)
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Calculated property: Amount paid so far (EN)<br />
    ///     Thuộc tính tính toán: Số tiền đã trả (VI)
    /// </summary>
    [NotMapped]
    public decimal AmountPaid => OriginalAmount - CurrentBalance;

    /// <summary>
    ///     Calculated property: Payment progress percentage (EN)<br />
    ///     Thuộc tính tính toán: Phần trăm tiến độ thanh toán (VI)
    /// </summary>
    [NotMapped]
    public decimal PaymentProgress => OriginalAmount > 0 ? AmountPaid / OriginalAmount * 100 : 0;

    /// <summary>
    ///     Calculated property: Whether debt is fully paid (EN)<br />
    ///     Thuộc tính tính toán: Khoản nợ đã được trả hết chưa (VI)
    /// </summary>
    [NotMapped]
    public bool IsFullyPaid => CurrentBalance <= 0;
}