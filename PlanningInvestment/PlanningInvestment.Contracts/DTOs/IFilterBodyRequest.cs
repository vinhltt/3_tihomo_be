namespace PlanningInvestment.Contracts.DTOs;

/// <summary>
/// Interface for filter body request
/// Interface cho request body filter
/// </summary>
public interface IFilterBodyRequest
{
    /// <summary>
    /// Search value / Giá trị tìm kiếm
    /// </summary>
    string? SearchValue { get; set; }

    /// <summary>
    /// Page number / Số trang
    /// </summary>
    int PageNumber { get; set; }

    /// <summary>
    /// Page size / Kích thước trang
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Sort field / Trường sắp xếp
    /// </summary>
    string? SortField { get; set; }

    /// <summary>
    /// Sort direction / Hướng sắp xếp
    /// </summary>
    string? SortDirection { get; set; }
} 