using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MoneyManagement.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace MoneyManagement.Domain.Entities;

/// <summary>
///     Jar entity for money management using jar method (EN)<br />
///     Entity lọ để quản lý tiền bằng phương pháp lọ (VI)
/// </summary>
public class Jar : BaseEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to user (EN)<br />
    ///     Khóa ngoại liên kết với người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Jar name (EN)<br />
    ///     Tên lọ (VI)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Jar description (EN)<br />
    ///     Mô tả lọ (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Jar type (EN)<br />
    ///     Loại lọ (VI)
    /// </summary>
    [Required]
    public JarType JarType { get; set; }

    /// <summary>
    ///     Current balance in jar (EN)<br />
    ///     Số dư hiện tại trong lọ (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative")]
    public decimal Balance { get; set; } = 0;

    /// <summary>
    ///     Target amount for this jar (EN)<br />
    ///     Số tiền mục tiêu cho lọ này (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Target amount must be non-negative")]
    public decimal? TargetAmount { get; set; }

    /// <summary>
    ///     Percentage allocation from income (EN)<br />
    ///     Phần trăm phân bổ từ thu nhập (VI)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
    public decimal? AllocationPercentage { get; set; }

    /// <summary>
    ///     Whether jar is active (EN)<br />
    ///     Lọ có đang hoạt động không (VI)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Color code for UI display (EN)<br />
    ///     Mã màu để hiển thị UI (VI)
    /// </summary>
    [MaxLength(7)]
    public string? ColorCode { get; set; }

    /// <summary>
    ///     Icon name for UI display (EN)<br />
    ///     Tên icon để hiển thị UI (VI)
    /// </summary>
    [MaxLength(50)]
    public string? IconName { get; set; }

    /// <summary>
    ///     Display order (EN)<br />
    ///     Thứ tự hiển thị (VI)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Check if target is reached (EN)<br />
    ///     Kiểm tra có đạt mục tiêu không (VI)
    /// </summary>
    public bool IsTargetReached => TargetAmount.HasValue && Balance >= TargetAmount.Value;

    /// <summary>
    ///     Get progress percentage towards target (EN)<br />
    ///     Lấy phần trăm tiến độ hướng tới mục tiêu (VI)
    /// </summary>
    public decimal ProgressPercentage => TargetAmount.HasValue && TargetAmount.Value > 0
        ? Math.Min(Balance / TargetAmount.Value * 100, 100)
        : 0;

    /// <summary>
    ///     Get remaining amount to reach target (EN)<br />
    ///     Lấy số tiền còn lại để đạt mục tiêu (VI)
    /// </summary>
    public decimal RemainingToTarget => TargetAmount.HasValue
        ? Math.Max(TargetAmount.Value - Balance, 0)
        : 0;
}