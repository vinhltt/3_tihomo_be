using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Summary model for jar allocation information (EN)<br />
///     Model tóm tắt cho thông tin phân bổ lọ (VI)
/// </summary>
public class JarAllocationSummaryDto
{
    /// <summary>
    ///     User ID who owns these jars (EN)<br />
    ///     ID người dùng sở hữu các lọ này (VI)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Total balance across all jars (EN)<br />
    ///     Tổng số dư của tất cả các lọ (VI)
    /// </summary>
    public decimal TotalBalance { get; set; }

    /// <summary>
    ///     Total target amount across all jars (EN)<br />
    ///     Tổng số tiền mục tiêu của tất cả các lọ (VI)
    /// </summary>
    public decimal TotalTargetAmount { get; set; }

    /// <summary>
    ///     Overall progress percentage (EN)<br />
    ///     Phần trăm tiến độ tổng thể (VI)
    /// </summary>
    public decimal OverallProgressPercentage { get; set; }

    /// <summary>
    ///     List of jar allocations (EN)<br />
    ///     Danh sách phân bổ các lọ (VI)
    /// </summary>
    public List<JarAllocationItemDto> JarAllocations { get; set; } = [];

    /// <summary>
    ///     Default Six Jars allocation percentages (EN)<br />
    ///     Phần trăm phân bổ mặc định cho Six Jars (VI)
    /// </summary>
    public Dictionary<JarType, decimal> DefaultAllocations { get; set; } = new();

    /// <summary>
    ///     Total allocation percentage used (EN)<br />
    ///     Tổng phần trăm phân bổ đã sử dụng (VI)
    /// </summary>
    public decimal TotalAllocationPercentage { get; set; }

    /// <summary>
    ///     Available allocation percentage (EN)<br />
    ///     Phần trăm phân bổ còn lại (VI)
    /// </summary>
    public decimal AvailableAllocationPercentage { get; set; }

    /// <summary>
    ///     Last update date (EN)<br />
    ///     Ngày cập nhật cuối (VI)
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
}

/// <summary>
///     Individual jar allocation item (EN)<br />
///     Mục phân bổ cho lọ riêng lẻ (VI)
/// </summary>
public class JarAllocationItemDto
{
    /// <summary>
    ///     Jar ID (EN)<br />
    ///     ID lọ (VI)
    /// </summary>
    public Guid JarId { get; set; }

    /// <summary>
    ///     Jar name (EN)<br />
    ///     Tên lọ (VI)
    /// </summary>
    public string JarName { get; set; } = string.Empty;

    /// <summary>
    ///     Jar type (EN)<br />
    ///     Loại lọ (VI)
    /// </summary>
    public JarType JarType { get; set; }

    /// <summary>
    ///     Current balance (EN)<br />
    ///     Số dư hiện tại (VI)
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    ///     Target amount (EN)<br />
    ///     Số tiền mục tiêu (VI)
    /// </summary>
    public decimal? TargetAmount { get; set; }

    /// <summary>
    ///     Allocation percentage (EN)<br />
    ///     Phần trăm phân bổ (VI)
    /// </summary>
    public decimal? AllocationPercentage { get; set; }

    /// <summary>
    ///     Progress percentage towards target (EN)<br />
    ///     Phần trăm tiến độ đạt mục tiêu (VI)
    /// </summary>
    public decimal ProgressPercentage { get; set; }

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
}