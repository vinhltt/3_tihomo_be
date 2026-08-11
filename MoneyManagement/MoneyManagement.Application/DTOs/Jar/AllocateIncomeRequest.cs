using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for allocating income across jars using 6 Jars method (EN)<br />
///     Model yêu cầu để phân bổ thu nhập vào các lọ sử dụng phương pháp 6 lọ (VI)
/// </summary>
public class AllocateIncomeRequest
{
    /// <summary>
    ///     Total income amount to allocate (EN)<br />
    ///     Tổng số tiền thu nhập cần phân bổ (VI)
    /// </summary>
    [Required(ErrorMessage = "Income amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Income amount must be greater than 0")]
    public decimal IncomeAmount { get; set; }

    /// <summary>
    ///     Custom allocation ratios (optional - if not provided, uses default 6 Jars ratios) (EN)<br />
    ///     Tỷ lệ phân bổ tùy chỉnh (tùy chọn - nếu không cung cấp, sử dụng tỷ lệ 6 lọ mặc định) (VI)
    /// </summary>
    public Dictionary<string, decimal>? CustomRatios { get; set; }

    /// <summary>
    ///     Description for this allocation (EN)<br />
    ///     Mô tả cho việc phân bổ này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Whether to use custom jar selection instead of all jars (EN)<br />
    ///     Có sử dụng lựa chọn lọ tùy chỉnh thay vì tất cả lọ không (VI)
    /// </summary>
    public bool UseCustomJarSelection { get; set; } = false;

    /// <summary>
    ///     List of jar IDs to allocate to (if UseCustomJarSelection is true) (EN)<br />
    ///     Danh sách ID lọ để phân bổ (nếu UseCustomJarSelection là true) (VI)
    /// </summary>
    public List<Guid>? SelectedJarIds { get; set; }
}

/// <summary>
///     Result model for income allocation operation (EN)<br />
///     Model kết quả cho thao tác phân bổ thu nhập (VI)
/// </summary>
public class IncomeAllocationResult
{
    /// <summary>
    ///     Total amount allocated (EN)<br />
    ///     Tổng số tiền đã phân bổ (VI)
    /// </summary>
    public decimal TotalAllocated { get; set; }

    /// <summary>
    ///     List of jar allocations (EN)<br />
    ///     Danh sách phân bổ lọ (VI)
    /// </summary>
    public List<JarAllocationDetail> JarAllocations { get; set; } = [];

    /// <summary>
    ///     Success status (EN)<br />
    ///     Trạng thái thành công (VI)
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    ///     Result message (EN)<br />
    ///     Thông báo kết quả (VI)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Allocation timestamp (EN)<br />
    ///     Thời gian phân bổ (VI)
    /// </summary>
    public DateTime AllocationTime { get; set; }
}

/// <summary>
///     Details of allocation for a specific jar (EN)<br />
///     Chi tiết phân bổ cho một lọ cụ thể (VI)
/// </summary>
public class JarAllocationDetail
{
    /// <summary>
    ///     Jar information (EN)<br />
    ///     Thông tin lọ (VI)
    /// </summary>
    public JarViewModel Jar { get; set; } = new();

    /// <summary>
    ///     Amount allocated to this jar (EN)<br />
    ///     Số tiền phân bổ cho lọ này (VI)
    /// </summary>
    public decimal AllocatedAmount { get; set; }

    /// <summary>
    ///     Allocation percentage used (EN)<br />
    ///     Phần trăm phân bổ được sử dụng (VI)
    /// </summary>
    public decimal AllocationPercentage { get; set; }

    /// <summary>
    ///     Balance before allocation (EN)<br />
    ///     Số dư trước khi phân bổ (VI)
    /// </summary>
    public decimal BalanceBefore { get; set; }

    /// <summary>
    ///     Balance after allocation (EN)<br />
    ///     Số dư sau khi phân bổ (VI)
    /// </summary>
    public decimal BalanceAfter { get; set; }
}