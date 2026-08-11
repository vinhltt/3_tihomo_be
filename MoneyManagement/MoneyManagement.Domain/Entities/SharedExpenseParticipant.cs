using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.EntityFramework.BaseEfModels;

namespace MoneyManagement.Domain.Entities;

/// <summary>
///     Shared expense participant entity for tracking individual shares (EN)<br />
///     Entity người tham gia chi tiêu chung để theo dõi phần chia sẻ cá nhân (VI)
/// </summary>
public class SharedExpenseParticipant : BaseEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to shared expense (EN)<br />
    ///     Khóa ngoại liên kết với chi tiêu chung (VI)
    /// </summary>
    public Guid? SharedExpenseId { get; set; }

    /// <summary>
    ///     Foreign key linking to user (EN)<br />
    ///     Khóa ngoại liên kết với người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Participant name (for non-users) (EN)<br />
    ///     Tên người tham gia (cho người không phải user) (VI)
    /// </summary>
    [MaxLength(200)]
    public string? ParticipantName { get; set; }

    /// <summary>
    ///     Participant email (EN)<br />
    ///     Email người tham gia (VI)
    /// </summary>
    [MaxLength(200)]
    public string? Email { get; set; }

    /// <summary>
    ///     Participant phone number (EN)<br />
    ///     Số điện thoại người tham gia (VI)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Share amount for this participant (EN)<br />
    ///     Số tiền chia sẻ cho người tham gia này (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Share amount must be non-negative")]
    public decimal ShareAmount { get; set; }

    /// <summary>
    ///     Amount already paid by this participant (EN)<br />
    ///     Số tiền người tham gia này đã trả (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Paid amount must be non-negative")]
    public decimal PaidAmount { get; set; } = 0;

    /// <summary>
    ///     Remaining amount this participant owes (EN)<br />
    ///     Số tiền còn lại người tham gia này nợ (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OwedAmount => ShareAmount - PaidAmount;

    /// <summary>
    ///     Whether this participant has settled their share (EN)<br />
    ///     Người tham gia này đã thanh toán phần của họ chưa (VI)
    /// </summary>
    public bool IsSettled => PaidAmount >= ShareAmount;

    /// <summary>
    ///     Date when participant settled their share (EN)<br />
    ///     Ngày người tham gia thanh toán phần của họ (VI)
    /// </summary>
    public DateTime? SettledDate { get; set; }

    /// <summary>
    ///     Payment method used (EN)<br />
    ///     Phương thức thanh toán đã sử dụng (VI)
    /// </summary>
    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    ///     Additional notes for this participant (EN)<br />
    ///     Ghi chú bổ sung cho người tham gia này (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Navigation property to shared expense (EN)<br />
    ///     Thuộc tính điều hướng tới chi tiêu chung (VI)
    /// </summary>
    public virtual SharedExpense? SharedExpense { get; set; }

    /// <summary>
    ///     Get payment percentage (EN)<br />
    ///     Lấy phần trăm thanh toán (VI)
    /// </summary>
    public decimal PaymentPercentage => ShareAmount > 0 ? PaidAmount / ShareAmount * 100 : 0;
}