namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents a filter request with logical operator and details. (EN)<br />
///     Đại diện cho một yêu cầu lọc với toán tử logic và chi tiết. (VI)
/// </summary>
public class FilterRequest
{
    /// <summary>
    ///     Gets or sets the logical operator for combining filter details (e.g., And, Or). (EN)<br />
    ///     Lấy hoặc đặt toán tử logic để kết hợp các chi tiết lọc (ví dụ: And, Or). (VI)
    /// </summary>
    public FilterLogicalOperator LogicalOperator { get; set; }

    /// <summary>
    ///     Gets or sets the collection of filter details. (EN)<br />
    ///     Lấy hoặc đặt tập hợp các chi tiết lọc. (VI)
    /// </summary>
    public IEnumerable<FilterDetailsRequest>? Details { get; set; }
}