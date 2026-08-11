using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for updating an existing jar (EN)<br />
///     Model yêu cầu để cập nhật lọ hiện có (VI)
/// </summary>
public class UpdateJarRequest
{
    /// <summary>
    ///     Jar identifier (EN)<br />
    ///     Định danh lọ (VI)
    /// </summary>
    [Required(ErrorMessage = "Jar ID is required")]
    public Guid Id { get; set; }

    /// <summary>
    ///     Jar name (EN)<br />
    ///     Tên lọ (VI)
    /// </summary>
    [Required(ErrorMessage = "Jar name is required")]
    [MaxLength(200, ErrorMessage = "Jar name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Jar description (EN)<br />
    ///     Mô tả lọ (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Target amount for this jar (EN)<br />
    ///     Số tiền mục tiêu cho lọ này (VI)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Target amount must be non-negative")]
    public decimal? TargetAmount { get; set; }

    /// <summary>
    ///     Percentage allocation from income (EN)<br />
    ///     Phần trăm phân bổ từ thu nhập (VI)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Allocation percentage must be between 0 and 100")]
    public decimal? AllocationPercentage { get; set; }

    /// <summary>
    ///     Color code for UI display (EN)<br />
    ///     Mã màu để hiển thị UI (VI)
    /// </summary>
    [MaxLength(7, ErrorMessage = "Color code cannot exceed 7 characters")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color code format")]
    public string? ColorCode { get; set; }

    /// <summary>
    ///     Icon name for UI display (EN)<br />
    ///     Tên icon để hiển thị UI (VI)
    /// </summary>
    [MaxLength(50, ErrorMessage = "Icon name cannot exceed 50 characters")]
    public string? IconName { get; set; }

    /// <summary>
    ///     Display order (EN)<br />
    ///     Thứ tự hiển thị (VI)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative")]
    public int DisplayOrder { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}