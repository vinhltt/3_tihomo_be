using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for updating an existing jar (EN)<br />
///     Model yêu cầu để cập nhật lọ hiện có (VI)
/// </summary>
public class UpdateJarRequestDto
{
    /// <summary>
    ///     Jar name (EN)<br />
    ///     Tên lọ (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Jar name cannot exceed 200 characters")]
    public required string Name { get; set; }

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
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color code must be a valid hex color")]
    public string? ColorCode { get; set; }

    /// <summary>
    ///     Icon name for UI display (EN)<br />
    ///     Tên icon để hiển thị UI (VI)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Icon name cannot exceed 100 characters")]
    public string? IconName { get; set; }

    /// <summary>
    ///     Whether the jar is active (EN)<br />
    ///     Lọ có đang hoạt động không (VI)
    /// </summary>
    public bool? IsActive { get; set; }
}