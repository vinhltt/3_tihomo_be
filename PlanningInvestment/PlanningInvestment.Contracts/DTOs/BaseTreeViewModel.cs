namespace CoreFinance.Contracts.DTOs;

/// <summary>
/// Defines the interface for a base tree structure view model. (EN)<br/>
/// Định nghĩa interface cho view model cấu trúc cây cơ sở. (VI)
/// </summary>
/// <typeparam name="T">The type of the data in the tree node.</typeparam>
public interface IBaseTreeViewModel<T>
{
    /// <summary>
    /// Gets or sets the data associated with the tree node. (EN)<br/>
    /// Lấy hoặc đặt dữ liệu liên quan đến nút cây. (VI)
    /// </summary>
    public T Data { get; set; }
    /// <summary>
    /// Gets or sets the list of child nodes. (EN)<br/>
    /// Lấy hoặc đặt danh sách các nút con. (VI)
    /// </summary>
    public List<IBaseTreeViewModel<T>> Children { get; set; }
}