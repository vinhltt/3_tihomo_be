using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     View model for displaying jar information (EN)<br />
///     View model để hiển thị thông tin lọ (VI)
/// </summary>
public class JarViewModel
{
    /// <summary>
    ///     Jar identifier (EN)<br />
    ///     Định danh lọ (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     User identifier (EN)<br />
    ///     Định danh người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Jar name (EN)<br />
    ///     Tên lọ (VI)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Jar description (EN)<br />
    ///     Mô tả lọ (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Jar type (EN)<br />
    ///     Loại lọ (VI)
    /// </summary>
    public JarType JarType { get; set; }

    /// <summary>
    ///     Current balance in jar (EN)<br />
    ///     Số dư hiện tại trong lọ (VI)
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Target amount for this jar (EN)<br />
    ///     Số tiền mục tiêu cho lọ này (VI)
    /// </summary>
    public decimal? TargetAmount { get; set; }

    /// <summary>
    ///     Percentage allocation from income (EN)<br />
    ///     Phần trăm phân bổ từ thu nhập (VI)
    /// </summary>
    public decimal? AllocationPercentage { get; set; }

    /// <summary>
    ///     Whether jar is active (EN)<br />
    ///     Lọ có đang hoạt động không (VI)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Color code for UI display (EN)<br />
    ///     Mã màu để hiển thị UI (VI)
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    ///     Icon name for UI display (EN)<br />
    ///     Tên icon để hiển thị UI (VI)
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    ///     Display order (EN)<br />
    ///     Thứ tự hiển thị (VI)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Check if target is reached (EN)<br />
    ///     Kiểm tra có đạt mục tiêu không (VI)
    /// </summary>
    public bool IsTargetReached { get; set; }

    /// <summary>
    ///     Progress percentage towards target (EN)<br />
    ///     Phần trăm tiến độ hướng tới mục tiêu (VI)
    /// </summary>
    public decimal ProgressPercentage { get; set; }

    /// <summary>
    ///     Remaining amount to reach target (EN)<br />
    ///     Số tiền còn lại để đạt mục tiêu (VI)
    /// </summary>
    public decimal RemainingToTarget { get; set; }

    /// <summary>
    ///     Creation date (EN)<br />
    ///     Ngày tạo (VI)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Last update date (EN)<br />
    ///     Ngày cập nhật cuối (VI)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}