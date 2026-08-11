namespace Shared.Contracts.DTOs;

/// <summary>
/// Sort direction enumeration
/// </summary>
public enum SortDirection
{
    Asc = 0,
    Desc = 1
}

/// <summary>
/// Sort descriptor for ordering
/// </summary>
public class SortDescriptor
{
    /// <summary>
    /// Field name to sort by
    /// </summary>
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// Sort direction
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.Asc;
}

/// <summary>
/// Pagination information
/// </summary>
public class Pagination
{
    /// <summary>
    /// Page index (1-based)
    /// </summary>
    public int PageIndex { get; set; } = 1;
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Total number of rows/records
    /// </summary>
    public int TotalRow { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPage => TotalRow > 0 ? (int)Math.Ceiling((double)TotalRow / PageSize) : 0;
}

/// <summary>
/// Interface for filter request bodies
/// </summary>
public interface IFilterBodyRequest
{
    /// <summary>
    /// Search term for general text search
    /// </summary>
    string? SearchTerm { get; set; }
    
    /// <summary>
    /// Search value for general text search (alias for SearchTerm)
    /// </summary>
    string? SearchValue { get; set; }
    
    /// <summary>
    /// Date from filter
    /// </summary>
    DateTime? DateFrom { get; set; }
    
    /// <summary>
    /// Date to filter
    /// </summary>
    DateTime? DateTo { get; set; }
    
    /// <summary>
    /// Pagination settings
    /// </summary>
    Pagination? Pagination { get; set; }
    
    /// <summary>
    /// List of sort descriptors for ordering
    /// </summary>
    List<SortDescriptor>? Orders { get; set; }
}

/// <summary>
/// Default implementation of filter request
/// </summary>
public class FilterBodyRequest : IFilterBodyRequest
{
    public string? SearchTerm { get; set; }
    public string? SearchValue { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Pagination? Pagination { get; set; } = new();
    public List<SortDescriptor>? Orders { get; set; } = new();
}
