namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents a filter request body with pagination and sorting options. (EN)<br />
///     Đại diện cho nội dung yêu cầu lọc với các tùy chọn phân trang và sắp xếp. (VI)
/// </summary>
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
public class FilterBodyRequest : BodyRequest, IFilterBodyRequest
{
    /// <summary>
    ///     Gets or sets the language identifier for the request. (EN)<br />
    ///     Lấy hoặc đặt định danh ngôn ngữ cho yêu cầu. (VI)
    /// </summary>
    public string? LangId { get; set; }

    /// <summary>
    ///     Gets or sets the search value. (EN)<br />
    ///     Lấy hoặc đặt giá trị tìm kiếm. (VI)
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    ///     Gets or sets the filter criteria. (EN)<br />
    ///     Lấy hoặc đặt tiêu chí lọc. (VI)
    /// </summary>
    public FilterRequest? Filter { get; set; }

    /// <summary>
    ///     Gets or sets the sorting descriptors. (EN)<br />
    ///     Lấy hoặc đặt các mô tả sắp xếp. (VI)
    /// </summary>
    public IEnumerable<SortDescriptor>? Orders { get; set; }

    /// <summary>
    ///     Gets or sets the pagination information. (EN)<br />
    ///     Lấy hoặc đặt thông tin phân trang. (VI)
    /// </summary>
    public Pagination? Pagination { get; set; }
}

/// <summary>
///     Defines the interface for a filter request body with pagination and sorting options. (EN)<br />
///     Định nghĩa interface cho nội dung yêu cầu lọc với các tùy chọn phân trang và sắp xếp. (VI)
/// </summary>
public interface IFilterBodyRequest : IBodyRequest
{
    /// <summary>
    ///     Gets or sets the language identifier for the request. (EN)<br />
    ///     Lấy hoặc đặt định danh ngôn ngữ cho yêu cầu. (VI)
    /// </summary>
    public string? LangId { get; set; }

    /// <summary>
    ///     Gets or sets the search value. (EN)<br />
    ///     Lấy hoặc đặt giá trị tìm kiếm. (VI)
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    ///     Gets or sets the filter criteria. (EN)<br />
    ///     Lấy hoặc đặt tiêu chí lọc. (VI)
    /// </summary>
    public FilterRequest? Filter { get; set; }

    /// <summary>
    ///     Gets or sets the sorting descriptors. (EN)<br />
    ///     Lấy hoặc đặt các mô tả sắp xếp. (VI)
    /// </summary>
    public IEnumerable<SortDescriptor>? Orders { get; set; }

    /// <summary>
    ///     Gets or sets the pagination information. (EN)<br />
    ///     Lấy hoặc đặt thông tin phân trang. (VI)
    /// </summary>
    public Pagination? Pagination { get; set; }
}
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).