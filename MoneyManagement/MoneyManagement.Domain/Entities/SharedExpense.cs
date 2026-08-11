using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MoneyManagement.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace MoneyManagement.Domain.Entities;

/// <summary>
///     Shared expense entity for managing group expenses (EN)<br />
///     Entity chi tiêu chung để quản lý chi tiêu nhóm (VI)
/// </summary>
public class SharedExpense : BaseEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to user who created the expense (EN)<br />
    ///     Khóa ngoại liên kết với người dùng tạo chi tiêu (VI)
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    ///     Expense title (EN)<br />
    ///     Tiêu đề chi tiêu (VI)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Expense description (EN)<br />
    ///     Mô tả chi tiêu (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Total expense amount (EN)<br />
    ///     Tổng số tiền chi tiêu (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Total amount must be non-negative")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Amount already settled (EN)<br />
    ///     Số tiền đã thanh toán (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Settled amount must be non-negative")]
    public decimal SettledAmount { get; set; } = 0;

    /// <summary>
    ///     Remaining amount to be settled (EN)<br />
    ///     Số tiền còn lại cần thanh toán (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount => TotalAmount - SettledAmount;

    /// <summary>
    ///     Expense date (EN)<br />
    ///     Ngày chi tiêu (VI)
    /// </summary>
    [Required]
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    ///     Expense category (EN)<br />
    ///     Danh mục chi tiêu (VI)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    ///     Expense status (EN)<br />
    ///     Trạng thái chi tiêu (VI)
    /// </summary>
    [Required]
    public SharedExpenseStatus Status { get; set; } = SharedExpenseStatus.Pending;

    /// <summary>
    ///     Group name or identifier (EN)<br />
    ///     Tên nhóm hoặc định danh (VI)
    /// </summary>
    [MaxLength(200)]
    public string? GroupName { get; set; }

    /// <summary>
    ///     Currency code (EN)<br />
    ///     Mã tiền tệ (VI)
    /// </summary>
    [MaxLength(3)]
    public string? CurrencyCode { get; set; } = "VND";

    /// <summary>
    ///     Receipt or proof image URL (EN)<br />
    ///     URL hình ảnh hóa đơn hoặc bằng chứng (VI)
    /// </summary>
    [MaxLength(500)]
    public string? ReceiptImageUrl { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Navigation property for expense participants (EN)<br />
    ///     Thuộc tính điều hướng cho người tham gia chi tiêu (VI)
    /// </summary>
    public virtual ICollection<SharedExpenseParticipant> Participants { get; set; } =
        new List<SharedExpenseParticipant>();

    /// <summary>
    ///     Check if expense is fully settled (EN)<br />
    ///     Kiểm tra chi tiêu đã thanh toán đầy đủ chưa (VI)
    /// </summary>
    public bool IsFullySettled => SettledAmount >= TotalAmount;

    /// <summary>
    ///     Get settlement percentage (EN)<br />
    ///     Lấy phần trăm thanh toán (VI)
    /// </summary>
    public decimal SettlementPercentage => TotalAmount > 0 ? SettledAmount / TotalAmount * 100 : 0;
}