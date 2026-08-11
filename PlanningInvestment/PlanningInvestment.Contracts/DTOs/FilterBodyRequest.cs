namespace PlanningInvestment.Contracts.DTOs;

/// <summary>
/// Filter body request for pagination and filtering
/// Request body filter cho phân trang và lọc
/// </summary>
public class FilterBodyRequest : IFilterBodyRequest
{
    /// <summary>
    /// Search value / Giá trị tìm kiếm
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    /// Page number / Số trang
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size / Kích thước trang
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field / Trường sắp xếp
    /// </summary>
    public string? SortField { get; set; }

    /// <summary>
    /// Sort direction / Hướng sắp xếp
    /// </summary>
    public string? SortDirection { get; set; } = "asc";
} 