using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Response model for jar information (EN)<br />
///     Model phản hồi cho thông tin lọ (VI)
/// </summary>
public class JarResponseDto
{
    /// <summary>
    ///     Jar unique identifier (EN)<br />
    ///     Định danh duy nhất của lọ (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     User ID who owns this jar (EN)<br />
    ///     ID người dùng sở hữu lọ này (VI)
    /// </summary>
    public Guid UserId { get; set; }

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
    ///     Current balance in the jar (EN)<br />
    ///     Số dư hiện tại trong lọ (VI)
    /// </summary>
    public decimal CurrentBalance { get; set; }

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
    ///     Whether the jar is active (EN)<br />
    ///     Lọ có đang hoạt động không (VI)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Progress percentage towards target (0-100) (EN)<br />
    ///     Phần trăm tiến độ đạt mục tiêu (0-100) (VI)
    /// </summary>
    public decimal ProgressPercentage { get; set; }

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