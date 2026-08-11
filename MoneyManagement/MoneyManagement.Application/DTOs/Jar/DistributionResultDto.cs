namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Result model for income distribution operations (EN)<br />
///     Model kết quả cho các thao tác phân phối thu nhập (VI)
/// </summary>
public class DistributionResultDto
{
    /// <summary>
    ///     Total income amount distributed (EN)<br />
    ///     Tổng số tiền thu nhập đã phân phối (VI)
    /// </summary>
    public decimal TotalIncomeDistributed { get; set; }

    /// <summary>
    ///     List of jar distributions (EN)<br />
    ///     Danh sách phân phối cho các lọ (VI)
    /// </summary>
    public List<JarDistributionItemDto> JarDistributions { get; set; } = [];

    /// <summary>
    ///     Distribution date and time (EN)<br />
    ///     Ngày giờ phân phối (VI)
    /// </summary>
    public DateTime DistributedAt { get; set; }

    /// <summary>
    ///     Description for this distribution (EN)<br />
    ///     Mô tả cho việc phân phối này (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Whether the distribution was successful (EN)<br />
    ///     Việc phân phối có thành công không (VI)
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    ///     Error message if distribution failed (EN)<br />
    ///     Thông báo lỗi nếu phân phối thất bại (VI)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Remaining amount not distributed (EN)<br />
    ///     Số tiền còn lại chưa được phân phối (VI)
    /// </summary>
    public decimal RemainingAmount { get; set; }
}

/// <summary>
///     Individual jar distribution item (EN)<br />
///     Mục phân phối cho lọ riêng lẻ (VI)
/// </summary>
public class JarDistributionItemDto
{
    /// <summary>
    ///     Jar information (EN)<br />
    ///     Thông tin lọ (VI)
    /// </summary>
    public JarResponseDto Jar { get; set; } = null!;

    /// <summary>
    ///     Amount allocated to this jar (EN)<br />
    ///     Số tiền được phân bổ cho lọ này (VI)
    /// </summary>
    public decimal AllocatedAmount { get; set; }

    /// <summary>
    ///     Allocation percentage used (EN)<br />
    ///     Phần trăm phân bổ được sử dụng (VI)
    /// </summary>
    public decimal AllocationPercentage { get; set; }

    /// <summary>
    ///     Previous balance before allocation (EN)<br />
    ///     Số dư trước khi phân bổ (VI)
    /// </summary>
    public decimal PreviousBalance { get; set; }

    /// <summary>
    ///     New balance after allocation (EN)<br />
    ///     Số dư mới sau khi phân bổ (VI)
    /// </summary>
    public decimal NewBalance { get; set; }
}