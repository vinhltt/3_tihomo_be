using Shared.EntityFramework.BaseEfModels;

namespace Shared.EntityFramework.DTOs;

/// <summary>
///     Represents base pagination information for a collection of data. (EN)<br />
///     Đại diện cho thông tin phân trang cơ sở cho một tập hợp dữ liệu. (VI)
/// </summary>
/// <typeparam name="T">The type of data in the collection.</typeparam>
public class BasePaging<T> : IBasePaging<T>
{
    /// <summary>
    ///     Gets or sets the pagination details. (EN)<br />
    ///     Lấy hoặc đặt chi tiết phân trang. (VI)
    /// </summary>
    public Pagination Pagination { get; set; } = new();

    /// <summary>
    ///     Gets or sets the collection of data for the current page. (EN)<br />
    ///     Lấy hoặc đặt tập hợp dữ liệu cho trang hiện tại. (VI)
    /// </summary>
    public IEnumerable<T>? Data { get; set; }
}

/// <summary>
///     Defines the interface for base pagination information. (EN)<br />
///     Định nghĩa interface cho thông tin phân trang cơ sở. (VI)
/// </summary>
/// <typeparam name="T">The type of data in the collection.</typeparam>
public interface IBasePaging<T>
{
    /// <summary>
    ///     Gets or sets the pagination details. (EN)<br />
    ///     Lấy hoặc đặt chi tiết phân trang. (VI)
    /// </summary>
    public Pagination Pagination { get; set; }

    /// <summary>
    ///     Gets or sets the collection of data for the current page. (EN)<br />
    ///     Lấy hoặc đặt tập hợp dữ liệu cho trang hiện tại. (VI)
    /// </summary>
    public IEnumerable<T>? Data { get; set; }
}