using Shared.EntityFramework.Enums;

namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents a single filter condition. (EN)<br />
///     Đại diện cho một điều kiện lọc đơn lẻ. (VI)
/// </summary>
public class FilterDescriptor
{
    /// <summary>
    ///     Gets or sets the field to filter on. (EN)<br />
    ///     Lấy hoặc đặt trường để lọc. (VI)
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    ///     Gets or sets the values to filter by. (EN)<br />
    ///     Lấy hoặc đặt các giá trị để lọc. (VI)
    /// </summary>
    public string?[]? Values { get; set; }

    /// <summary>
    ///     Gets or sets the filter operator (e.g., Equals, Contains, GreaterThan). (EN)<br />
    ///     Lấy hoặc đặt toán tử lọc (ví dụ: Bằng, Chứa, Lớn hơn). (VI)
    /// </summary>
    public FilterType Operator { get; set; }

    /// <summary>
    ///     Gets or sets the logical operator for combining this filter with others (e.g., And, Or). (EN)<br />
    ///     Lấy hoặc đặt toán tử logic để kết hợp bộ lọc này với các bộ lọc khác (ví dụ: And, Or). (VI)
    /// </summary>
    public FilterLogicalOperator LogicalOperator { get; set; }
}