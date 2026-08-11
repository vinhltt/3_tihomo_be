using Shared.EntityFramework.Enums;

namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents the details of a filter condition. (EN)<br />
///     Đại diện cho chi tiết của một điều kiện lọc. (VI)
/// </summary>
public class FilterDetailsRequest
{
    /// <summary>
    ///     Gets or sets the name of the attribute to filter on. (EN)<br />
    ///     Lấy hoặc đặt tên thuộc tính để lọc. (VI)
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    ///     Gets or sets the value to filter by. (EN)<br />
    ///     Lấy hoặc đặt giá trị để lọc. (VI)
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the type of filter to apply (e.g., Equals, Contains, GreaterThan). (EN)<br />
    ///     Lấy hoặc đặt kiểu lọc để áp dụng (ví dụ: Bằng, Chứa, Lớn hơn). (VI)
    /// </summary>
    public FilterType FilterType { get; set; }
}